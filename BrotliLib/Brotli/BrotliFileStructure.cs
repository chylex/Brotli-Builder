using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.IO;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Represents the object structure of a Brotli-compressed file.
    /// https://tools.ietf.org/html/rfc7932#section-2
    /// </summary>
    public sealed class BrotliFileStructure{
        public static BrotliFileStructure NewEmpty(){
            BrotliFileStructure bfs = new BrotliFileStructure{
                WindowSize = WindowSize.Default
            };

            bfs.MetaBlocks.Add(new MetaBlock.LastEmpty());
            return bfs;
        }

        public static BrotliFileStructure FromBytes(byte[] bytes){
            return Serializer.FromBits(new BitStream(bytes).GetReader(), new BrotliGlobalState());
        }

        // Data

        public WindowSize WindowSize { get; set; }
        public readonly IList<MetaBlock> MetaBlocks = new List<MetaBlock>();
        
        private BrotliFileStructure(){}

        public BitStream Serialize(){
            BitStream stream = new BitStream();
            BrotliGlobalState state = new BrotliGlobalState();
            Serializer.ToBits(stream.GetWriter(), this, state);
            return stream;
        }

        public BrotliGlobalState GetDecompressionState(){
            BrotliGlobalState state = new BrotliGlobalState();
            Serializer.FromBits(Serialize().GetReader(), state);
            return state;
        }

        public override string ToString(){
            return Serialize().ToString();
        }

        // Serialization

        private static readonly IBitSerializer<BrotliFileStructure, BrotliGlobalState> Serializer = new BitSerializer<BrotliFileStructure, BrotliGlobalState>(
            fromBits: (reader, context) => {
                BrotliFileStructure bfs = new BrotliFileStructure{
                    WindowSize = WindowSize.Serializer.FromBits(reader, context)
                };

                while(true){
                    MetaBlock metaBlock = MetaBlock.Serializer.FromBits(reader, context);
                    bfs.MetaBlocks.Add(metaBlock);

                    if (metaBlock.IsLast){
                        break;
                    }
                }

                return bfs;
            },

            toBits: (writer, obj, context) => {
                WindowSize.Serializer.ToBits(writer, obj.WindowSize, context);

                foreach(MetaBlock metaBlock in obj.MetaBlocks){
                    MetaBlock.Serializer.ToBits(writer, metaBlock, context);
                }
            }
        );
    }
}
