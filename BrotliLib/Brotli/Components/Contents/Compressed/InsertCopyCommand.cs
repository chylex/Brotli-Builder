using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.Brotli.State;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
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

        // Cloning

        private InsertCopyCommand(InsertCopyCommand original, DistanceInfo newDistanceInfo){
            this.Literals = original.Literals;
            this.CopyLength = original.CopyLength;
            this.CopyDistance = newDistanceInfo;
        }

        public InsertCopyCommand WithImplicitDistanceCodeZero(){
            return CopyDistance == DistanceInfo.ImplicitCodeZero ? this : new InsertCopyCommand(this, DistanceInfo.ImplicitCodeZero);
        }

        // Object

        public override bool Equals(object obj){
            return obj is InsertCopyCommand command &&
                   EqualityComparer<IReadOnlyList<Literal>>.Default.Equals(Literals, command.Literals) &&
                   CopyLength == command.CopyLength &&
                   CopyDistance == command.CopyDistance;
        }

        public override int GetHashCode() {
            unchecked{
                var hashCode = -1468049732;
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<Literal>>.Default.GetHashCode(Literals);
                hashCode = hashCode * -1521134295 + CopyLength.GetHashCode();
                hashCode = hashCode * -1521134295 + CopyDistance.GetHashCode();
                return hashCode;
            }
        }

        // Serialization

        internal static readonly BitDeserializer<InsertCopyCommand, CompressedMetaBlockContents.DataContext> Deserialize = MarkedBitDeserializer.Title<InsertCopyCommand, CompressedMetaBlockContents.DataContext>(
            "Insert & Copy Command",

            (reader, context) => {
                MetaBlockCompressionHeader header = context.Header;
                BrotliGlobalState state = context.State;
                
                // Insert&copy lengths
                
                int icBlockID = context.NextBlockID(Category.InsertCopy);
                InsertCopyLengthCode icLengthCode = reader.ReadValue(header.InsertCopyTrees[icBlockID].Root, "length code");
                InsertCopyLengths icLengthValues = reader.ReadStructure(InsertCopyLengths.Deserialize, icLengthCode, "length values");

                int insertLength = icLengthValues.InsertLength;
                int copyLength = icLengthValues.CopyLength;
                
                // Literals

                Literal[] literals = new Literal[insertLength];
                
                for(int insertIndex = 0; insertIndex < insertLength; insertIndex++){
                    int blockID = context.NextBlockID(Category.Literal);
                    int contextID = state.NextLiteralContextID(header.LiteralCtxModes[blockID]);
                    int treeID = header.LiteralCtxMap.DetermineTreeID(blockID, contextID);

                    Literal literal = reader.ReadValue(header.LiteralTrees[treeID].Root, "literal");

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

                    DistanceCode distanceCode = reader.ReadValue(header.DistanceTrees[treeID].Root, "distance code");
                    distanceInfo = reader.ReadValue(DistanceCode.Deserialize, distanceCode.MakeContext(state), "distance value");
                }

                context.WriteCopy(copyLength, distanceInfo);

                return new InsertCopyCommand(literals, copyLength, distanceInfo);
            }
        );

        internal static readonly BitSerializer<InsertCopyCommand, CompressedMetaBlockContents.DataContext> Serialize = (writer, obj, context) => {
            MetaBlockCompressionHeader header = context.Header;
            BrotliGlobalState state = context.State;
            
            bool endsAfterLiterals = obj.CopyDistance == DistanceInfo.EndsAfterLiterals;
            bool implicitDistanceCodeZero = obj.CopyDistance == DistanceInfo.ImplicitCodeZero;

            // Insert&copy lengths

            InsertCopyLengths icLengthValues = obj.Lengths;
            int icBlockID = context.NextBlockID(Category.InsertCopy);
            var icLengthEntry = header.InsertCopyTrees[icBlockID].FindEntry(code => icLengthValues.CanEncodeUsing(code) && (implicitDistanceCodeZero == code.UseDistanceCodeZero || endsAfterLiterals));
            var icLengthCode = icLengthEntry.Key;

            writer.WriteBits(icLengthEntry.Value);
            InsertCopyLengths.Serialize(writer, icLengthValues, icLengthCode);
            
            // Literals
            
            foreach(Literal literal in obj.Literals){
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
                int contextID = icLengthValues.DistanceContextID;
                int treeID = header.DistanceCtxMap.DetermineTreeID(blockID, contextID);

                var distanceEntry = header.DistanceTrees[treeID].Where(kvp => distanceInfo.CanEncodeUsing(kvp.Key, state)).OrderBy(kvp => kvp.Value.Length + kvp.Key.ExtraBits).First();
                var distanceCode = distanceEntry.Key;

                writer.WriteBits(distanceEntry.Value);
                DistanceCode.Serialize(writer, distanceInfo, distanceCode.MakeContext(state));
            }
            
            context.WriteCopy(icLengthValues.CopyLength, distanceInfo);
        };
    }
}
