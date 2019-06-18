using System;
using System.Collections.Generic;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Markers.Reader;
using BrotliLib.Brotli.State;
using BrotliLib.Brotli.State.Output;
using BrotliLib.IO;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Represents the object structure of a Brotli-compressed file.
    /// https://tools.ietf.org/html/rfc7932#section-2
    /// </summary>
    public sealed class BrotliFileStructure{
        public static BrotliFileStructure NewEmpty(){
            var bfs = new BrotliFileStructure(new BrotliFileParameters());
            bfs.MetaBlocks.Add(new MetaBlock.LastEmpty());
            return bfs;
        }

        public static BrotliFileStructure FromBytes(byte[] bytes){
            return DoDeserialize(CreateReader(new BitStream(bytes), enableMarkers: false), new FileContext(BrotliDefaultDictionary.Embedded, windowSize => new BrotliOutputWindowed(windowSize)));
        }

        public static BrotliFileStructure FromEncoder(BrotliFileParameters parameters, IBrotliEncoder encoder, byte[] bytes){
            var bfs = new BrotliFileStructure(parameters);

            foreach(MetaBlock metaBlock in encoder.GenerateMetaBlocks(parameters, bytes)){
                bfs.MetaBlocks.Add(metaBlock);
            }

            bfs.Fixup();
            return bfs;
        }

        // Data

        public BrotliFileParameters Parameters { get; set; }
        public IList<MetaBlock> MetaBlocks { get; }
        
        private BrotliFileStructure(BrotliFileParameters parameters){
            this.Parameters = parameters;
            this.MetaBlocks = new List<MetaBlock>();
        }

        public BrotliFileStructure Transform(IBrotliTransformer transformer){
            var copy = new BrotliFileStructure(Parameters);
            var state = new BrotliGlobalState(Parameters, new BrotliOutputWindowed(Parameters.WindowSize));

            foreach(MetaBlock original in MetaBlocks){
                foreach(MetaBlock transformed in transformer.Transform(original, state)){ // TODO figure out how to handle state
                    copy.MetaBlocks.Add(transformed);
                    MetaBlock.Serialize(new BitStream().GetWriter(), transformed, state); // TODO null writer
                }
            }

            copy.Fixup();
            return copy;
        }

        public void Fixup(){
            for(int index = 0, last = MetaBlocks.Count - 1; index <= last; index++){
                MetaBlocks[index].IsLast = index == last;
            }
        }

        public BitStream Serialize(){
            BitStream stream = new BitStream();
            DoSerialize(stream.GetWriter(), this, new FileContext(Parameters.Dictionary, new BrotliOutputWindowed(Parameters.WindowSize)));
            return stream;
        }

        public BrotliOutputStored GetDecompressionState(BitStream bitStream, bool enableMarkers){
            var outputState = new BrotliOutputStored();

            IMarkedBitReader reader = CreateReader(bitStream, enableMarkers);
            DoDeserialize(reader, new FileContext(Parameters.Dictionary, outputState));

            outputState.BitMarkerRoot = reader.MarkerRoot;
            return outputState;
        }

        public override string ToString(){
            return Serialize().ToString();
        }

        // Serialization

        private class FileContext{
            public BrotliDictionary Dictionary { get; }
            public Func<WindowSize, IBrotliOutputState> OutputState { get; }

            public FileContext(BrotliDictionary dictionary, Func<WindowSize, IBrotliOutputState> outputState){
                Dictionary = dictionary;
                OutputState = outputState;
            }

            public FileContext(BrotliDictionary dictionary, IBrotliOutputState outputState) : this(dictionary, _ => outputState){}
        }

        private static IMarkedBitReader CreateReader(BitStream bitStream, bool enableMarkers){
            return enableMarkers ? new MarkedBitReader(bitStream.GetReader()) : (IMarkedBitReader)new MarkedBitReaderDummy(bitStream.GetReader());
        }

        private static readonly BitDeserializer<BrotliFileStructure, FileContext> DoDeserialize = (reader, context) => {
            WindowSize windowSize = WindowSize.Deserialize(reader, NoContext.Value);

            var parameters = new BrotliFileParameters(windowSize, context.Dictionary);
            var structure = new BrotliFileStructure(parameters);

            var state = new BrotliGlobalState(parameters, context.OutputState(windowSize));

            while(true){
                MetaBlock metaBlock = MetaBlock.Deserialize(reader, state);
                structure.MetaBlocks.Add(metaBlock);

                if (metaBlock.IsLast){
                    break;
                }
            }

            return structure;
        };

        private static readonly BitSerializer<BrotliFileStructure, FileContext> DoSerialize = (writer, obj, context) => {
            var parameters = obj.Parameters;
            var windowSize = parameters.WindowSize;
            
            WindowSize.Serialize(writer, windowSize, NoContext.Value);

            var state = new BrotliGlobalState(parameters, context.OutputState(windowSize));

            foreach(MetaBlock metaBlock in obj.MetaBlocks){
                MetaBlock.Serialize(writer, metaBlock, state);
            }
        };
    }
}
