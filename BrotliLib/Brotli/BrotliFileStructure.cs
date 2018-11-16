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
            return Serializer.FromBits(new BitStream(bytes).GetReader(), null);
        }

        // Data

        public WindowSize WindowSize { get; set; }
        public readonly IList<MetaBlock> MetaBlocks = new List<MetaBlock>();
        
        private BrotliFileStructure(){}

        public BitStream Serialize(){
            BitStream stream = new BitStream();
            Serializer.ToBits(stream.GetWriter(), this, null);
            return stream;
        }

        public override string ToString(){
            return Serialize().ToString();
        }

        // Serialization

        private static readonly IBitSerializer<BrotliFileStructure, object> Serializer = new BitSerializer<BrotliFileStructure, object>(
            fromBits: (reader, context) => {
                BrotliGlobalState state = new BrotliGlobalState();

                BrotliFileStructure bfs = new BrotliFileStructure{
                    WindowSize = WindowSize.Serializer.FromBits(reader, state)
                };

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
                BrotliGlobalState state = new BrotliGlobalState();

                WindowSize.Serializer.ToBits(writer, obj.WindowSize, state);

                foreach(MetaBlock metaBlock in obj.MetaBlocks){
                    MetaBlock.Serializer.ToBits(writer, metaBlock, state);
                }
            }
        );
    }
}
