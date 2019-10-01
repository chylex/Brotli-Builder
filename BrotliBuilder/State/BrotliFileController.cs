using System;
using System.Diagnostics;
using System.IO;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.State.Output;
using BrotliLib.IO;

namespace BrotliBuilder.State{
    sealed class BrotliFileController{
        public bool EnableBitMarkers { get; set; } = true;
        
        public event EventHandler<StateChangedEventArgs> StateChanged;
        public BrotliFileStructure CurrentFile => (state as BrotliFileState.Loaded)?.File;
        private BrotliFileState state = new BrotliFileState.NoFile();

        private readonly AsyncWorker worker;
        private int currentToken;

        public BrotliFileController(string name){
            worker = new AsyncWorker{ Name = name };
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

            StateChanged?.Invoke(this, new StateChangedEventArgs(prevState, newState));
        });

        // Public triggers

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

        public void LoadFile(string path) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());

            if (!TryReadFile(token, path, out byte[] bytes)) return;
            BitStream bits = new BitStream(bytes);
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), null));

            if (!TryDeserialize(token, bytes, out BrotliFileStructure structure)) return;
            UpdateState(token, new BrotliFileState.HasStructure(structure));

            if (!TryGetDecompressionState(token, structure, bits, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, swOutput));
        });

        public void LoadStructure(BrotliFileStructure structure) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());
            UpdateState(token, new BrotliFileState.HasStructure(structure));

            if (!TrySerialize(token, structure, out BitStream bits, out Stopwatch stopwatch)) return;
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), stopwatch));

            if (!TryGetDecompressionState(token, structure, bits, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, swOutput));
        });

        public void EncodeFile(string path, BrotliFileParameters parameters, IBrotliEncoder encoder) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());

            if (!TryReadFile(token, path, out byte[] bytes)) return;
            if (!TryEncode(token, bytes, parameters, encoder, out BrotliFileStructure structure)) return;
            UpdateState(token, new BrotliFileState.HasStructure(structure));

            if (!TrySerialize(token, structure, out BitStream bits, out Stopwatch swSerialization)) return;
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), swSerialization));

            if (!TryGetDecompressionState(token, structure, bits, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, swOutput));
        });

        private void TransformInternal(BrotliFileStructure structure, IBrotliTransformer transformer) => StartWorker(token => {
            UpdateState(token, new BrotliFileState.Starting());

            if (!TryTransform(token, structure, transformer, out structure)) return;
            UpdateState(token, new BrotliFileState.HasStructure(structure));

            if (!TrySerialize(token, structure, out BitStream bits, out Stopwatch swSerialization)) return;
            UpdateState(token, new BrotliFileState.HasBits(bits.ToString(), swSerialization));

            if (!TryGetDecompressionState(token, structure, bits, out BrotliOutputStored output, out Stopwatch swOutput)) return;
            UpdateState(token, new BrotliFileState.Loaded(structure, bits, output, swOutput));
        });

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
                bytes = null;
                return OnError(token, ErrorType.ReadingFile, ex);
            }
        }

        private bool TryDeserialize(int token, byte[] bytes, out BrotliFileStructure structure){
            try{
                structure = BrotliFileStructure.FromBytes(bytes);
                return true;
            }catch(Exception ex){
                structure = null;
                return OnError(token, ErrorType.DeserializingFile, ex);
            }
        }

        private bool TrySerialize(int token, BrotliFileStructure structure, out BitStream bits, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                bits = structure.Serialize();
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                bits = null;
                stopwatch = null;
                return OnError(token, ErrorType.SerializingStructure, ex);
            }
        }

        private bool TryEncode(int token, byte[] bytes, BrotliFileParameters parameters, IBrotliEncoder encoder, out BrotliFileStructure file){
            try{
                file = BrotliFileStructure.FromEncoder(parameters, encoder, bytes);
                return true;
            }catch(Exception ex){
                file = null;
                return OnError(token, ErrorType.EncodingBytes, ex);
            }
        }

        private bool TryTransform(int token, BrotliFileStructure structure, IBrotliTransformer transformer, out BrotliFileStructure transformed){
            try{
                transformed = structure.Transform(transformer);
                return true;
            }catch(Exception ex){
                transformed = null;
                return OnError(token, ErrorType.TransformingStructure, ex);
            }
        }

        private bool TryGetDecompressionState(int token, BrotliFileStructure structure, BitStream bits, out BrotliOutputStored output, out Stopwatch stopwatch){
            try{
                stopwatch = Stopwatch.StartNew();
                output = structure.GetDecompressionState(bits, EnableBitMarkers);
                stopwatch.Stop();
                return true;
            }catch(Exception ex){
                output = null;
                stopwatch = null;
                return OnError(token, ErrorType.DecompressingStructure, ex);
            }
        }
    }
}
