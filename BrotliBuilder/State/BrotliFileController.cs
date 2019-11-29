using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Output;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Markers;
using BrotliLib.Serialization;

namespace BrotliBuilder.State{
    sealed class BrotliFileController{
        public BrotliSerializationParameters SerializationParameters { get; } = BrotliSerializationParameters.Default;
        public BrotliCompressionParameters CompressionParameters { get; } = BrotliCompressionParameters.Default;

        public MarkerLevel BitMarkerLevel { get; set; } = MarkerLevel.Verbose;
        
        public event EventHandler<StateChangedEventArgs>? StateChanged;
        public BrotliFileStructure? CurrentFile => (state as BrotliFileState.Loaded)?.File;
        private BrotliFileState state = new BrotliFileState.NoFile();

        private readonly AsyncWorker worker;
        private int currentToken;

        private readonly List<BrotliFileState> replay = new List<BrotliFileState>();

        public BrotliFileController(string name){
            worker = new AsyncWorker(name);
        }

        // State handling

        private void StartWorker(Action<int> work){
            int token = ++currentToken;
            worker.Start(() => work(token));
        }

        private void UpdateState(int token, BrotliFileState newState) => worker.Sync(() => {
            if (token != currentToken){
                Debug.WriteLine("Ignored state change due to mismatched token (" + token + " != " + currentToken + ")");
                return;
            }

            BrotliFileState prevState = state;
            state = newState;

            if (state is BrotliFileState.Starting){
                replay.Clear();
            }

            replay.Add(state);

            StateChanged?.Invoke(this, new StateChangedEventArgs(prevState, newState));
        });

        // Public triggers

        public void ReplayOver(BrotliFileController target){
            int token = ++target.currentToken;

            foreach(var replayedState in replay){
                target.UpdateState(token, replayedState);
            }
        }

        public void ResetToNothing(){
            worker.Abort();
            UpdateState(++currentToken, new BrotliFileState.NoFile());
        }

        public void ResetToWaiting(){
            worker.Abort();
            UpdateState(++currentToken, new BrotliFileState.Waiting());
        }

        public void ResetToEmpty(){
            LoadStructure(BrotliFileStructure.NewEmpty());
        }
        
        #pragma warning disable IDE0011 // Add braces

        public void LoadFile(string path) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());

            if (!TryReadFile(token, path, out byte[] bytes)) return;
            BitStream bits = new BitStream(bytes);
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), null));

            if (!TryDeserialize(token, new BitStream(bytes), out BrotliFileStructure structure, out MarkerRoot markerRoot, out Stopwatch swDeserialize)) return;
            UpdateState(token, new BrotliFileState.HasMarkers(markerRoot));
            UpdateState(token, new BrotliFileState.HasStructure(structure, swDeserialize));

            if (!TryDecompress(token, structure, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.HasOutput(null, output.AsBytes, swOutput));

            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, markerRoot));
        });

        public void LoadStructure(BrotliFileStructure structure, byte[]? checkAgainst = null) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());
            UpdateState(token, new BrotliFileState.HasStructure(structure, null));

            if (!TrySerialize(token, structure, out BitStream bits, out Stopwatch swSerialization)) return;
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), swSerialization));

            if (!TryDeserialize(token, bits, out BrotliFileStructure _, out MarkerRoot markerRoot, out Stopwatch _)) return;
            UpdateState(token, new BrotliFileState.HasMarkers(markerRoot));

            if (!TryDecompress(token, structure, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.HasOutput(checkAgainst, output.AsBytes, swOutput));

            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, markerRoot));
        });

        public void EncodeFile(string path, BrotliFileParameters parameters, IBrotliEncoder encoder) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());

            if (!TryReadFile(token, path, out byte[] bytes)) return;
            if (!TryEncode(token, bytes, parameters, encoder, out BrotliFileStructure structure, out Stopwatch swEncode)) return;
            UpdateState(token, new BrotliFileState.HasStructure(structure, swEncode));

            if (!TrySerialize(token, structure, out BitStream bits, out Stopwatch swSerialization)) return;
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), swSerialization));
            
            if (!TryDeserialize(token, bits, out BrotliFileStructure _, out MarkerRoot markerRoot, out Stopwatch _)) return;
            UpdateState(token, new BrotliFileState.HasMarkers(markerRoot));

            if (!TryDecompress(token, structure, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.HasOutput(bytes, output.AsBytes, swOutput));

            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, markerRoot));
        });

        private void TransformInternal(BrotliFileStructure structure, IBrotliTransformer transformer) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());

            TryDecompress(token, structure, out BrotliOutputStored prevOutput, out Stopwatch _);

            if (!TryTransform(token, structure, transformer, out structure, out Stopwatch swTransform)) return;
            UpdateState(token, new BrotliFileState.HasStructure(structure, swTransform));

            if (!TrySerialize(token, structure, out BitStream bits, out Stopwatch swSerialization)) return;
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), swSerialization));
            
            if (!TryDeserialize(token, bits, out BrotliFileStructure _, out MarkerRoot markerRoot, out Stopwatch _)) return;
            UpdateState(token, new BrotliFileState.HasMarkers(markerRoot));

            if (!TryDecompress(token, structure, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.HasOutput(prevOutput?.AsBytes ?? Array.Empty<byte>(), output.AsBytes, swOutput));

            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, markerRoot));
        });

        #pragma warning restore IDE0011 // Add braces

        public bool Transform(IBrotliTransformer transformer){
            if (state is BrotliFileState.Loaded loaded){
                TransformInternal(loaded.File, transformer);
                return true;
            }

            return false;
        }

        // Steps

        private bool OnError(int token, ErrorType source, Exception exception){
            UpdateState(token, new BrotliFileState.Error(source, exception));
            return false;
        }

        private bool TryReadFile(int token, string path, out byte[] bytes){
            try{
                bytes = File.ReadAllBytes(path);
                return true;
            }catch(Exception ex){
                bytes = null!;
                return OnError(token, ErrorType.ReadingFile, ex);
            }
        }

        private bool TryDeserialize(int token, BitStream bits, out BrotliFileStructure structure, out MarkerRoot markerRoot, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                (structure, markerRoot) = BrotliFileStructure.FromBytes(bits, BitMarkerLevel);
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                structure = null!;
                markerRoot = null!;
                stopwatch = null!;
                return OnError(token, ErrorType.DeserializingFile, ex);
            }
        }

        private bool TrySerialize(int token, BrotliFileStructure structure, out BitStream bits, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                bits = structure.Serialize(SerializationParameters);
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                bits = null!;
                stopwatch = null!;
                return OnError(token, ErrorType.SerializingStructure, ex);
            }
        }

        private bool TryEncode(int token, byte[] bytes, BrotliFileParameters parameters, IBrotliEncoder encoder, out BrotliFileStructure file, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                file = BrotliFileStructure.FromEncoder(parameters, CompressionParameters, bytes, encoder);
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                file = null!;
                stopwatch = null!;
                return OnError(token, ErrorType.EncodingBytes, ex);
            }
        }

        private bool TryTransform(int token, BrotliFileStructure structure, IBrotliTransformer transformer, out BrotliFileStructure transformed, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                transformed = structure.Transform(transformer, CompressionParameters);
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                transformed = null!;
                stopwatch = null!;
                return OnError(token, ErrorType.TransformingStructure, ex);
            }
        }

        private bool TryDecompress(int token, BrotliFileStructure structure, out BrotliOutputStored output, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                output = structure.Decompress();
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                output = null!;
                stopwatch = null!;
                return OnError(token, ErrorType.DecompressingStructure, ex);
            }
        }
    }
}
