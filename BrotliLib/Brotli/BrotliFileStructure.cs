using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Encode;
using BrotliLib.IO;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Represents the object structure of a Brotli-compressed file.
    /// https://tools.ietf.org/html/rfc7932#section-2
    /// </summary>
    public sealed class BrotliFileStructure{
        public static BrotliFileStructure NewEmpty(){
            BrotliFileStructure bfs = new BrotliFileStructure(WindowSize.Default);
            bfs.MetaBlocks.Add(new MetaBlock.LastEmpty());
            return bfs;
        }

        public static BrotliFileStructure FromBytes(byte[] bytes){
            return Serializer.FromBits(new BitStream(bytes).GetReader(), null);
        }

        public static BrotliFileStructure FromEncoder(WindowSize windowSize, IBrotliEncoder encoder, byte[] bytes){
            BrotliFileStructure bfs = new BrotliFileStructure(windowSize);

            foreach(MetaBlock metaBlock in encoder.GenerateMetaBlocks(bytes)){
                bfs.MetaBlocks.Add(metaBlock);
            }

            return bfs;
        }

        // Data

        public WindowSize WindowSize { get; set; }
        public readonly IList<MetaBlock> MetaBlocks = new List<MetaBlock>();
        
        private BrotliFileStructure(WindowSize windowSize){
            this.WindowSize = windowSize;
        }

        private BrotliGlobalState CreateNewContext(){
            return new BrotliGlobalState(BrotliDefaultDictionary.Embedded, WindowSize);
        }

        public void Fixup(){
            for(int index = 0, last = MetaBlocks.Count - 1; index <= last; index++){
                MetaBlocks[index].IsLast = index == last;
            }
        }

        public BitStream Serialize(){
            BitStream stream = new BitStream();
            Serializer.ToBits(stream.GetWriter(), this, null);
            return stream;
        }

        public BrotliGlobalState GetDecompressionState(BitStream bitStream){
            BrotliGlobalState state = CreateNewContext();
            Serializer.FromBits(bitStream.GetReader(), state);
            return state;
        }

        public override string ToString(){
            return Serialize().ToString();
        }

        // Serialization

        private static readonly IBitSerializer<BrotliFileStructure, BrotliGlobalState> Serializer = new BitSerializer<BrotliFileStructure, BrotliGlobalState>(
            fromBits: (reader, context) => {
                WindowSize windowSize = WindowSize.Serializer.FromBits(reader, NoContext.Value);

                BrotliFileStructure bfs = new BrotliFileStructure(windowSize);
                BrotliGlobalState state = context ?? bfs.CreateNewContext();

                while(true){
                    MetaBlock metaBlock = MetaBlock.Serializer.FromBits(reader, state);
                    bfs.MetaBlocks.Add(metaBlock);

                    if (metaBlock.IsLast){
                        break;
                    }
                }

                return bfs;
            },

            toBits: (writer, obj, context) => {
                WindowSize.Serializer.ToBits(writer, obj.WindowSize, NoContext.Value);

                BrotliGlobalState state = context ?? obj.CreateNewContext();

                foreach(MetaBlock metaBlock in obj.MetaBlocks){
                    MetaBlock.Serializer.ToBits(writer, metaBlock, state);
                }
            }
        );
    }
}
