using System.Collections.Generic;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components.Contents.Compressed;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Dictionary.Index;
using BrotliLib.Brotli.Encode;

namespace BrotliImpl.Encoders.Utils{
    abstract class Copy{
        internal abstract int OutputLength { get; }
        internal abstract int AddCommand(BrotliFileParameters parameters, CompressedMetaBlockBuilder builder, IList<Literal> literals);

        internal class BackReference : Copy{
            internal override int OutputLength => length;

            private readonly int length;
            private readonly int distance;

            public BackReference(int length, int distance){
                this.length = length;
                this.distance = distance;
            }

            internal override int AddCommand(BrotliFileParameters parameters, CompressedMetaBlockBuilder builder, IList<Literal> literals){
                builder.AddInsertCopy(new InsertCopyCommand(literals, length, distance));
                return length;
            }
        }

        internal class Dictionary : Copy{
            internal override int OutputLength => entry.OutputLength;

            private readonly DictionaryIndexEntry entry;

            public Dictionary(DictionaryIndexEntry entry){
                this.entry = entry;
            }

            internal override int AddCommand(BrotliFileParameters parameters, CompressedMetaBlockBuilder builder, IList<Literal> literals){
                builder.AddInsertCopy(literals, entry);
                return entry.OutputLength;
            }
        }
    }
}
