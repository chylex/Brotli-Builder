using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BrotliLib.Brotli.Dictionary.Index{
    internal static class DictionaryIndexHelper{
        internal sealed class Writer : IDisposable{
            public int Position{
                get{
                    writer.Flush();
                    return (int)writer.BaseStream.Position;
                }
            }

            private readonly BinaryWriter writer;

            public Writer(Stream stream, Encoding encoding, bool leaveOpen){
                this.writer = new BinaryWriter(stream, encoding, leaveOpen);
            }

            public void Dispose(){
                writer.Dispose();
            }

            public void WriteByte(int value) => writer.Write((byte)value);
            public void WriteInt(int value) => writer.Write(value);

            public void WriteVarInt(int value){
                if (value < 0){
                    throw new ArgumentOutOfRangeException(nameof(value), "Negative varints in dictionary index files are not supported.");
                }

                while(value >= 128){
                    WriteByte((value & 0b_0111_1111) | 0b_1000_0000);
                    value >>= 7;
                }

                WriteByte(value);
            }

            public void WriteArray<T>(IReadOnlyList<T> array, Action<T> writer){
                if (array == null){
                    WriteVarInt(0);
                }
                else{
                    WriteVarInt(array.Count);

                    foreach(T element in array){
                        writer(element);
                    }
                }
            }
        }

        internal sealed class Reader : IDisposable{
            private readonly BinaryReader reader;

            public Reader(Stream stream, Encoding encoding, bool leaveOpen){
                this.reader = new BinaryReader(stream, encoding, leaveOpen);
            }

            public void Dispose(){
                reader.Dispose();
            }

            public byte ReadByte() => reader.ReadByte();
            public int ReadInt() => reader.ReadInt32();

            public int ReadVarInt(){
                int value = 0;
                int shift = 0;
                byte b;

                do{
                    value |= ((b = reader.ReadByte()) & 0b_0111_1111) << shift;
                    shift += 7;
                }while((b & 0b_1000_0000) != 0);

                return value;
            }

            public T[] ReadArray<T>(Func<T> reader){
                int length = ReadVarInt();
                return length == 0 ? null : Enumerable.Range(0, length).Select(_ => reader()).ToArray();
            }
        }
    }
}
