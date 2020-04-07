using System.Collections.Generic;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.Encode.Build;

namespace BrotliImpl.Utils{
    abstract class Copy{
        internal abstract int OutputLength { get; }
        internal abstract int AddCommand(CompressedMetaBlockBuilder builder, IList<Literal> literals);

        internal class BackReference : Copy{
            internal override int OutputLength { get; }
            internal int Distance { get; }

            public BackReference(int length, int distance){
                this.OutputLength = length;
                this.Distance = distance;
            }

            internal override int AddCommand(CompressedMetaBlockBuilder builder, IList<Literal> literals){
                builder.AddInsertCopy(literals, OutputLength, Distance);
                return OutputLength;
            }
        }

        internal class Dictionary : Copy{
            internal override int OutputLength => entry.OutputLength;

            private readonly DictionaryIndexEntry entry;

            public Dictionary(DictionaryIndexEntry entry){
                this.entry = entry;
            }

            internal override int AddCommand(CompressedMetaBlockBuilder builder, IList<Literal> literals){
                builder.AddInsertCopy(literals, in entry);
                return entry.OutputLength;
            }
        }
    }
}
