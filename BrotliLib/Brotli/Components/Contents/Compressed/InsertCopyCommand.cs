using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.Brotli.Markers;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
    public sealed class InsertCopyCommand{
        public const int ImpliedDistanceCodeZero = int.MaxValue;
        public const int MissingCopyDistance = int.MinValue;

        public IReadOnlyList<Literal> Literals { get; }
        public int CopyLength { get; }
        public int CopyDistance { get; }

        public InsertCopyLengths Lengths => new InsertCopyLengths(Literals.Count, CopyLength);

        public InsertCopyCommand(IReadOnlyList<Literal> literals, int copyLength){
            this.Literals = literals;
            this.CopyLength = copyLength;
            this.CopyDistance = MissingCopyDistance;
        }

        public InsertCopyCommand(IReadOnlyList<Literal> literals, int copyLength, int copyDistance){
            this.Literals = literals;
            this.CopyLength = copyLength;
            this.CopyDistance = copyDistance;
        }

        // Object

        public override bool Equals(object obj){
            return obj is InsertCopyCommand command &&
                   EqualityComparer<IReadOnlyList<Literal>>.Default.Equals(Literals, command.Literals) &&
                   CopyLength == command.CopyLength &&
                   EqualityComparer<int?>.Default.Equals(CopyDistance, command.CopyDistance);
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -1468049732;
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<Literal>>.Default.GetHashCode(Literals);
                hashCode = hashCode * -1521134295 + CopyLength.GetHashCode();
                hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(CopyDistance);
                return hashCode;
            }
        }

        // Serialization

        internal static readonly IBitSerializer<InsertCopyCommand, CompressedMetaBlockContents.DataContext> Serializer = new MarkedBitSerializer<InsertCopyCommand, CompressedMetaBlockContents.DataContext>(
            markerTitle: "Insert & Copy Command",

            fromBits: (reader, context) => {
                MetaBlockCompressionHeader header = context.Header;
                BrotliGlobalState state = context.State;
                
                // Insert&copy lengths
                
                int icBlockID = context.NextBlockID(Category.InsertCopy);
                InsertCopyLengthCode icCode = reader.ReadValue(header.InsertCopyTrees[icBlockID].Root, "length code");
                InsertCopyLengths icValues = reader.ReadStructure(InsertCopyLengths.Serializer, icCode, "length values");

                int insertLength = icValues.InsertLength;
                int copyLength = icValues.CopyLength;
                
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
                
                int distanceValue;
                bool useDistanceCodeZero = icCode.UseDistanceCodeZero;

                if (useDistanceCodeZero){
                    distanceValue = state.DistanceBuffer.Front;
                }
                else{
                    int blockID = context.NextBlockID(Category.Distance);
                    int contextID = Math.Min(3, copyLength - 2);
                    int treeID = header.DistanceCtxMap.DetermineTreeID(blockID, contextID);

                    DistanceCode distanceCode = reader.ReadValue(header.DistanceTrees[treeID].Root, "distance code");
                    distanceValue = reader.ReadValue(DistanceCode.Serializer, distanceCode.MakeContext(state), "distance value");
                }

                context.WriteCopy(copyLength, distanceValue, useDistanceCodeZero);

                return new InsertCopyCommand(literals, copyLength, useDistanceCodeZero ? ImpliedDistanceCodeZero : distanceValue);
            },

            toBits: (writer, obj, context) => {
                MetaBlockCompressionHeader header = context.Header;
                BrotliGlobalState state = context.State;
                
                bool endsAfterLiterals = obj.CopyDistance == MissingCopyDistance;
                bool useDistanceCodeZero = obj.CopyDistance == ImpliedDistanceCodeZero;

                // Insert&copy lengths

                InsertCopyLengths icLengths = obj.Lengths;
                int icBlockID = context.NextBlockID(Category.InsertCopy);
                var icEntry = header.InsertCopyTrees[icBlockID].FindEntry(code => icLengths.CanEncodeUsing(code) && (useDistanceCodeZero == code.UseDistanceCodeZero || endsAfterLiterals));
                var icCode = icEntry.Key;

                writer.WriteBits(icEntry.Value);
                InsertCopyLengths.Serializer.ToBits(writer, icLengths, icCode);
                
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

                int copyDistance;

                if (useDistanceCodeZero){
                    copyDistance = state.DistanceBuffer.Front;
                }
                else{
                    copyDistance = obj.CopyDistance;

                    int blockID = context.NextBlockID(Category.Distance);
                    int contextID = Math.Min(3, icLengths.CopyLength - 2);
                    int treeID = header.DistanceCtxMap.DetermineTreeID(blockID, contextID);

                    var distanceEntry = header.DistanceTrees[treeID].FindEntry(code => code.CanEncodeValue(state, copyDistance));
                    var distanceCode = distanceEntry.Key;

                    writer.WriteBits(distanceEntry.Value);
                    DistanceCode.Serializer.ToBits(writer, copyDistance, distanceCode.MakeContext(state));
                }
                
                context.WriteCopy(icLengths.CopyLength, copyDistance, useDistanceCodeZero);
            }
        );
    }
}
