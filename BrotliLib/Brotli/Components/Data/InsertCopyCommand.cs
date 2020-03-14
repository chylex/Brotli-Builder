using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Compressed;
using BrotliLib.Brotli.Utils;
using BrotliLib.Collections;
using BrotliLib.Markers;
using BrotliLib.Markers.Serialization;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli.Components.Data{
    public sealed class InsertCopyCommand{
        public IReadOnlyList<Literal> Literals { get; }

        public int CopyLength { get; }
        public DistanceInfo CopyDistance { get; }

        public InsertCopyLengths Lengths => new InsertCopyLengths(Literals.Count, CopyLength);
        
        public InsertCopyCommand(IList<Literal> literals, int copyLength = InsertCopyLengths.MinCopyLength, DistanceInfo copyDistance = DistanceInfo.EndsAfterLiterals){
            InsertCopyLengths.CheckBounds(literals.Count, copyLength);

            this.Literals = literals.ToArray();
            this.CopyLength = copyLength;
            this.CopyDistance = copyDistance;
        }
        
        public InsertCopyCommand(IList<Literal> literals, int copyLength, int copyDistance) : this(literals, copyLength, (DistanceInfo)copyDistance){}

        // Object

        public override bool Equals(object obj){
            return obj is InsertCopyCommand command &&
                   CollectionHelper.Equal(Literals, command.Literals) &&
                   CopyLength == command.CopyLength &&
                   CopyDistance == command.CopyDistance;
        }

        public override int GetHashCode(){
            return HashCode.Combine(CollectionHelper.HashCode(Literals), CopyLength, CopyDistance);
        }

        public override string ToString(){
            return Lengths + ", CopyDistance = " + CopyDistance;
        }

        // Serialization

        internal static readonly BitDeserializer<InsertCopyCommand, CompressedData.DataContext> Deserialize = MarkedBitDeserializer.Title<InsertCopyCommand, CompressedData.DataContext>(
            "Insert & Copy Command",

            (reader, context) => {
                CompressedHeader header = context.Header;
                BrotliGlobalState state = context.State;
                
                // Insert&copy lengths
                
                int icBlockID = context.NextBlockID(Category.InsertCopy);
                var icLengthCode = reader.ReadValue(header.InsertCopyTrees[icBlockID].Root, "length code");
                var icLengthValues = reader.ReadValue(InsertCopyLengths.Deserialize, icLengthCode, "length values");

                int insertLength = icLengthValues.InsertLength;
                int copyLength = icLengthValues.CopyLength;
                
                // Literals

                Literal[] literals = insertLength == 0 ? Array.Empty<Literal>() : new Literal[insertLength];
                
                for(int insertIndex = 0; insertIndex < insertLength; insertIndex++){
                    int blockID = context.NextBlockID(Category.Literal);
                    int contextID = state.NextLiteralContextID(header.LiteralCtxModes[blockID]);
                    int treeID = header.LiteralCtxMap.DetermineTreeID(blockID, contextID);

                    Literal literal;

                    if (reader.MarkerLevel == MarkerLevel.Verbose){
                        literal = reader.ReadValue(header.LiteralTrees[treeID].Root, "literal");
                    }
                    else{
                        literal = header.LiteralTrees[treeID].Root.LookupValue(reader);
                    }

                    literals[insertIndex] = literal;
                    context.WriteLiteral(in literal);
                }

                if (!context.NeedsMoreData){
                    return new InsertCopyCommand(literals, copyLength);
                }

                // Distance
                
                DistanceInfo distanceInfo;

                if (icLengthCode.UseDistanceCodeZero){
                    distanceInfo = DistanceInfo.ImplicitCodeZero;
                }
                else{
                    int blockID = context.NextBlockID(Category.Distance);
                    int contextID = icLengthValues.DistanceContextID;
                    int treeID = header.DistanceCtxMap.DetermineTreeID(blockID, contextID);

                    var distanceCode = reader.ReadValue(header.DistanceTrees[treeID].Root, "distance code");
                    distanceInfo = reader.ReadValue(DistanceCode.Deserialize, distanceCode.MakeContext(state), "distance value");
                }

                if (reader.MarkerLevel == MarkerLevel.Verbose){
                    context.WriteCopyWithMarker(reader, copyLength, distanceInfo);
                }
                else{
                    context.WriteCopy(copyLength, distanceInfo);
                }

                return new InsertCopyCommand(literals, copyLength, distanceInfo);
            }
        );

        internal static readonly BitSerializer<InsertCopyCommand, CompressedData.DataContext> Serialize = (writer, obj, context) => {
            CompressedHeader header = context.Header;
            BrotliGlobalState state = context.State;
            
            bool endsAfterLiterals = obj.CopyDistance == DistanceInfo.EndsAfterLiterals;
            bool implicitDistanceCodeZero = obj.CopyDistance == DistanceInfo.ImplicitCodeZero;

            // Insert&copy lengths

            InsertCopyLengths icLengths = obj.Lengths;
            int icBlockID = context.NextBlockID(Category.InsertCopy);

            var icLengthEntry = endsAfterLiterals ? header.InsertCopyTrees[icBlockID].FindShortest(icLengths, (code, lengths) => lengths.CanEncodeUsing(code)) 
                                                  : header.InsertCopyTrees[icBlockID].FindShortest(icLengths, implicitDistanceCodeZero, (code, lengths, dcz) => dcz == code.UseDistanceCodeZero && lengths.CanEncodeUsing(code));

            writer.WriteBits(icLengthEntry.Value);
            InsertCopyLengths.Serialize(writer, icLengths, icLengthEntry.Key);
            
            // Literals
            
            for(int insertIndex = 0; insertIndex < icLengths.InsertLength; insertIndex++){
                var literal = obj.Literals[insertIndex];

                int blockID = context.NextBlockID(Category.Literal);
                int contextID = state.NextLiteralContextID(header.LiteralCtxModes[blockID]);
                int treeID = header.LiteralCtxMap.DetermineTreeID(blockID, contextID);

                writer.WriteBits(header.LiteralTrees[treeID].FindPath(literal));
                context.WriteLiteral(in literal);
            }

            // Distance

            if (endsAfterLiterals){
                return;
            }

            DistanceInfo distanceInfo = obj.CopyDistance;
            
            if (distanceInfo != DistanceInfo.ImplicitCodeZero){
                int blockID = context.NextBlockID(Category.Distance);
                int contextID = icLengths.DistanceContextID;
                int treeID = header.DistanceCtxMap.DetermineTreeID(blockID, contextID);

                var distanceEntry = header.DistanceTrees[treeID].FindShortest(distanceInfo, state, (code, dist, st) => dist.CanEncodeUsing(code, st), code => code.ExtraBits);
                var distanceCode = distanceEntry.Key;

                writer.WriteBits(distanceEntry.Value);
                DistanceCode.Serialize(writer, distanceInfo, distanceCode.MakeContext(state));
            }
            
            context.WriteCopy(icLengths.CopyLength, distanceInfo);
        };
    }
}
