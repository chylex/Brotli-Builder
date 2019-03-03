using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Utils;
using BrotliLib.IO;

namespace BrotliLib.Brotli.Components.Contents.Compressed{
    public sealed class InsertCopyCommand{
        public const int ImpliedDistanceCodeZero = int.MaxValue;

        public InsertCopyLengths Lengths { get; }
        public IReadOnlyList<Literal> Literals { get; }
        public int? CopyDistance { get; }

        public InsertCopyCommand(IReadOnlyList<Literal> literals, int copyLength){
            this.Lengths = new InsertCopyLengths(literals.Count, copyLength);
            this.Literals = literals;
            this.CopyDistance = null;
        }

        public InsertCopyCommand(IReadOnlyList<Literal> literals, int copyLength, int copyDistance){
            this.Lengths = new InsertCopyLengths(literals.Count, copyLength);
            this.Literals = literals;
            this.CopyDistance = copyDistance;
        }

        // Object

        public override bool Equals(object obj){
            return obj is InsertCopyCommand command &&
                   EqualityComparer<InsertCopyLengths>.Default.Equals(Lengths, command.Lengths) &&
                   EqualityComparer<IReadOnlyList<Literal>>.Default.Equals(Literals, command.Literals) &&
                   EqualityComparer<int?>.Default.Equals(CopyDistance, command.CopyDistance);
        }

        public override int GetHashCode(){
            unchecked{
                var hashCode = -558486850;
                hashCode = hashCode * -1521134295 + EqualityComparer<InsertCopyLengths>.Default.GetHashCode(Lengths);
                hashCode = hashCode * -1521134295 + EqualityComparer<IReadOnlyList<Literal>>.Default.GetHashCode(Literals);
                hashCode = hashCode * -1521134295 + EqualityComparer<int?>.Default.GetHashCode(CopyDistance);
                return hashCode;
            }
        }

        // Serialization

        internal static readonly IBitSerializer<InsertCopyCommand, CompressedMetaBlockContents.DataContext> Serializer = new BitSerializer<InsertCopyCommand, CompressedMetaBlockContents.DataContext>(
            fromBits: (reader, context) => {
                MetaBlockCompressionHeader header = context.Header;
                BrotliGlobalState state = context.State;
                
                // Insert&copy lengths
                
                int icBlockID = context.NextBlockID(Category.InsertCopy);
                InsertCopyLengthCode icCode = header.InsertCopyTrees[icBlockID].Root.LookupValue(reader);
                InsertCopyLengths icValues = InsertCopyLengths.Serializer.FromBits(reader, icCode);

                int insertLength = icValues.InsertLength;
                int copyLength = icValues.CopyLength;
                
                // Literals

                Literal[] literals = new Literal[insertLength];
                
                for(int insertIndex = 0; insertIndex < insertLength; insertIndex++){
                    int blockID = context.NextBlockID(Category.Literal);
                    int contextID = context.NextLiteralContextID(header.LiteralCtxModes[blockID]);
                    int treeID = header.LiteralCtxMap.DetermineTreeID(blockID, contextID);

                    Literal literal = header.LiteralTrees[treeID].Root.LookupValue(reader);

                    literals[insertIndex] = literal;
                    context.WriteLiteral(literal);
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

                    DistanceCode distanceCode = header.DistanceTrees[treeID].Root.LookupValue(reader);
                    distanceValue = DistanceCode.Serializer.FromBits(reader, distanceCode.MakeContext(state));
                }

                context.WriteCopy(copyLength, distanceValue, useDistanceCodeZero);

                return new InsertCopyCommand(literals, copyLength, useDistanceCodeZero ? ImpliedDistanceCodeZero : distanceValue);
            },

            toBits: (writer, obj, context) => {
                MetaBlockCompressionHeader header = context.Header;
                BrotliGlobalState state = context.State;
                
                bool endsAfterLiterals = obj.CopyDistance == null;
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
                    int contextID = context.NextLiteralContextID(header.LiteralCtxModes[blockID]);
                    int treeID = header.LiteralCtxMap.DetermineTreeID(blockID, contextID);

                    writer.WriteBits(header.LiteralTrees[treeID].FindPath(literal));
                    context.WriteLiteral(literal);
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
                    copyDistance = obj.CopyDistance.Value;

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
