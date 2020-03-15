using BrotliLib.Brotli.Components.Header;

namespace BrotliLib.Brotli.Components{
    partial class MetaBlock{
        /// <inheritdoc />
        /// <summary>
        /// <code>ISLAST = 1, ISLASTEMPTY = 1</code>
        /// </summary>
        public class LastEmpty : MetaBlock{
            public new static Marked Marked { get; } = new LastEmpty().Mark(isLast: true);

            public LastEmpty() : base(DataLength.Empty){}

            public override void Decompress(BrotliGlobalState state){}

            public override bool Equals(object obj){
                return obj is LastEmpty;
            }

            public override int GetHashCode(){
                return ParentHashCode();
            }
        }
    }
}
