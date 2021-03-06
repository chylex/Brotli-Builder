﻿using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Streaming;
using BrotliLib.Markers;
using BrotliLib.Markers.Builders;
using BrotliLib.Serialization;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Represents the object structure of a Brotli-compressed file.
    /// https://tools.ietf.org/html/rfc7932#section-2
    /// </summary>
    public sealed class BrotliFileStructure{
        public static BrotliFileStructure NewEmpty(){
            var bfs = new BrotliFileStructure(BrotliFileParameters.Default);
            bfs.Fixup();
            return bfs;
        }

        public static BrotliFileStructure FromBytes(BitStream bits, BrotliDictionary? dictionary = null, in MarkerSettings markerSettings = default){
            var reader = BrotliFileReader.FromBytes(bits, dictionary, markerSettings);
            var bfs = new BrotliFileStructure(reader.Parameters);

            reader.ForEachRemainingMetaBlock(bfs.MetaBlocks.Add);
            return bfs;
        }

        public static BrotliFileStructure FromBytes(byte[] bytes, BrotliDictionary? dictionary = null, in MarkerSettings markerSettings = default){
            return FromBytes(new BitStream(bytes), dictionary, markerSettings);
        }

        public static (BrotliFileStructure Structure, MarkerRoot MarkerRoot) FromBytes(BitStream bits, MarkerLevel markerLevel, BrotliDictionary? dictionary = null){
            var markerBuilder = new MarkerRootBuilder();
            var structure = FromBytes(bits, dictionary, new MarkerSettings(markerLevel, markerBuilder));

            return (structure, markerBuilder.Root);
        }

        public static (BrotliFileStructure Structure, MarkerRoot MarkerRoot) FromBytes(byte[] bytes, MarkerLevel markerLevel, BrotliDictionary? dictionary = null){
            return FromBytes(new BitStream(bytes), markerLevel, dictionary);
        }

        public static BrotliFileStructure FromEncoder(BrotliFileParameters fileParameters, BrotliCompressionParameters compressionParameters, byte[] bytes, IBrotliEncoder encoder, params IBrotliTransformer[] transformers){
            return new BrotliEncodePipeline.Simple(fileParameters.WindowSize, compressionParameters, encoder, transformers).Apply(bytes, fileParameters.Dictionary);
        }

        // Instance

        public BrotliFileParameters Parameters { get; set; }
        public List<MetaBlock> MetaBlocks { get; }
        
        public BrotliFileStructure(BrotliFileParameters parameters){
            this.Parameters = parameters;
            this.MetaBlocks = new List<MetaBlock>();
        }

        public BrotliFileStructure Transform(IBrotliTransformer transformer, BrotliCompressionParameters compressionParameters){
            var transformed = new BrotliFileStructure(Parameters);
            var state = new BrotliGlobalState(Parameters);

            foreach(MetaBlock original in MetaBlocks){
                foreach(var (transformedMetaBlock, transformedState) in transformer.Transform(original, state, compressionParameters)){
                    transformed.MetaBlocks.Add(transformedMetaBlock);
                    state = transformedState; // we only care about the last state
                }
            }

            transformed.Fixup();
            return transformed;
        }

        public void Fixup(){
            MetaBlocks.RemoveAll(mb => mb is MetaBlock.LastEmpty);

            if (MetaBlocks.Count == 0 || MetaBlocks[^1] is MetaBlock.Uncompressed){
                MetaBlocks.Add(new MetaBlock.LastEmpty());
            }
        }

        public BrotliOutputStored Decompress(){
            var output = new BrotliOutputStored();
            var state = new BrotliGlobalState(Parameters, output);

            foreach(var metaBlock in MetaBlocks){
                metaBlock.Decompress(state);
            }

            return output;
        }

        public BitStream Serialize(BrotliSerializationParameters serializationParameters){
            var writer = new BrotliFileWriter(Parameters, serializationParameters);

            foreach(MetaBlock metaBlock in MetaBlocks){
                writer.WriteMetaBlock(metaBlock);
            }

            return writer.Close();
        }
    }
}
