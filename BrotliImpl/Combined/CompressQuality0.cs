using System;
using BrotliImpl.Utils;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Collections;

namespace BrotliImpl.Combined{
    /// <summary>
    /// Partial reimplementation of official Brotli compression, quality level 0.
    ///
    /// The official implementation updates histograms of length and distance trees for each upcoming meta-block,
    /// this implementation uses the initial predefined trees for every meta-block.
    /// 
    /// Due to meta-block merging heuristics depending on Huffman trees, which are generated differently to the official implementation,
    /// only most small files will match the official compressor. However, the insert&amp;copy command generation algorithm is identical.
    /// </summary>
    public partial class CompressQuality0 : BrotliEncodePipeline{
        private static readonly WindowSize SupportedWindowSize = new WindowSize(18);

        protected override WindowSize DetermineWindowSize(byte[] bytes){
            return SupportedWindowSize;
        }

        protected override BrotliCompressionParameters SetupCompressionParameters(byte[] bytes){
            return new BrotliCompressionParameters.Builder{
                GenerateLengthCodeTree   = _ => Huffman.PredefinedLengthCodes,
                GenerateDistanceCodeTree = _ => Huffman.PredefinedDistanceCodes
            }.Build();
        }

        protected override IBrotliEncoder CreateEncoder(byte[] bytes, BrotliFileParameters fileParameters){
            return new Encoder(bytes);
        }

        protected override void FinalizeStructure(BrotliFileStructure structure){
            structure.MetaBlocks.Add(new MetaBlock.LastEmpty());
        }

        /// <summary>
        /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (BrotliCompressFragmentFastImpl).
        /// </summary>
        private class Encoder : IBrotliEncoder{
            private const int FirstBlockSize = 3 << 15;
            private const int MergeBlockSize = 1 << 16;
            private const int MinMatchLen = 5;
            private const int MinRatio = 980;

            private readonly byte[] input;
            private int inputSize;

            private readonly HashTable table;

            private HuffmanTree<Literal>? literalTree;
            private int literalRatio;

            private int blockSize;
            private int totalBlockSize;
            
            private readonly int baseIp;
            private int ip;
            private int ipEnd;
            private int metaBlockStart;
            private int nextEmit;
            private int lastDistance;

            private int ipLimit;
            private uint nextHash;

            private int nextIp;
            private int skip;
            private int candidate;
            private int outputTo;

            private int nextIpStart;

            private enum NextStep{
                EmitCommands,
                EmitCommandsNextHash,
                Trawl,
                EmitRemainder,
                OutputCompressed,
                OutputUncompressed
            }

            public Encoder(byte[] input){
                this.input = input;
                this.inputSize = input.Length;

                this.table = new HashTable(input);

                this.blockSize = Math.Min(inputSize, FirstBlockSize);
                this.totalBlockSize = blockSize;

                this.metaBlockStart = 0;
                this.baseIp = 0;
                this.nextEmit = 0;
            }

            public (MetaBlock, BrotliEncodeInfo) Encode(BrotliEncodeInfo info){
                (literalTree, literalRatio) = Huffman.EstimateLiteralRatio(new ArraySegment<byte>(input, ip, blockSize));

                var builder = info.NewBuilder();
                var nextStep = NextStep.EmitCommands;

                while(true){
                    switch(nextStep){
                        case NextStep.EmitCommands:
                            nextStep = EmitCommands();
                            break;

                        case NextStep.EmitCommandsNextHash:
                            nextStep = EmitCommandsNextHash();
                            break;

                        case NextStep.Trawl:
                            nextStep = Trawl(builder);
                            break;

                        case NextStep.EmitRemainder:
                            nextStep = EmitRemainder(builder);
                            break;

                        case NextStep.OutputCompressed:
                            OnNextBlock();

                            return builder.Build(info, new BrotliCompressionParameters.Builder(info.CompressionParameters){
                                GenerateLiteralTree = _ => literalTree
                            }.Build());

                        case NextStep.OutputUncompressed:
                            var metaBlock = new MetaBlock.Uncompressed(input, metaBlockStart, outputTo - metaBlockStart);

                            inputSize -= metaBlock.DataLength.UncompressedBytes;
                            nextIpStart = outputTo;
                            nextEmit = nextIpStart;
                            OnNextBlock();

                            return (metaBlock, info.WithOutputtedMetaBock(metaBlock));
                    }
                }
            }

            private NextStep EmitCommands(){
                ip = nextIpStart;
                ipEnd = ip + blockSize;
                lastDistance = -1;

                if (blockSize >= WindowSize.MarginBytes){
                    int lenLimit = Math.Min(blockSize - MinMatchLen, inputSize - WindowSize.MarginBytes);

                    ipLimit = nextIpStart + lenLimit;
                    nextHash = table.Hash(++ip);
                    return NextStep.EmitCommandsNextHash;
                }
                else{
                    return NextStep.EmitRemainder;
                }
            }

            private NextStep EmitCommandsNextHash(){
                nextIp = ip;
                skip = 32;

                return NextStep.Trawl;
            }

            private NextStep Trawl(CompressedMetaBlockBuilder builder){
                do{
                    uint hash = nextHash;
                    int bytesBetweenHashLookups = (skip++) >> 5;

                    ip = nextIp;
                    nextIp = ip + bytesBetweenHashLookups;

                    if (nextIp > ipLimit){
                        return NextStep.EmitRemainder;
                    }

                    nextHash = table.Hash(nextIp);
                    candidate = ip - lastDistance;

                    if (Match.Check(input, ip, candidate, MinMatchLen) && candidate < ip){
                        table[hash] = ip - baseIp;
                        break;
                    }

                    candidate = baseIp + table[hash];
                    table[hash] = ip - baseIp;
                }while(!Match.Check(input, ip, candidate, MinMatchLen));

                if (ip - candidate > SupportedWindowSize.Bytes){
                    return NextStep.Trawl;
                }

                {
                    int @base = ip;
                    int matched = MinMatchLen + Match.DetermineLength(input, candidate + MinMatchLen, ip + MinMatchLen, ipEnd - ip - MinMatchLen);
                    int distance = @base - candidate;
                    int insert = @base - nextEmit;

                    ip += matched;

                    if (insert >= 6210 && ShouldUseUncompressedMode(insert)){
                        outputTo = @base;
                        return NextStep.OutputUncompressed;
                    }

                    if (distance == lastDistance){
                        builder.AddInsertCopy(Literal.FromBytes(input, nextEmit, insert), 2, DistanceInfo.ExplicitCodeZero);
                    }
                    else{
                        builder.AddInsertCopy(Literal.FromBytes(input, nextEmit, insert), 2, distance);
                        lastDistance = distance;
                    }

                    builder.AddCopy(matched - 2, distance);

                    nextEmit = ip;

                    if (ip >= ipLimit){
                        return NextStep.EmitRemainder;
                    }
                    
                    candidate = table.UpdateAndGetCandidate(ip, baseIp);
                }

                while(Match.Check(input, ip, candidate, MinMatchLen)){
                    int @base = ip;
                    int matched = MinMatchLen + Match.DetermineLength(input, candidate + MinMatchLen, ip + MinMatchLen, ipEnd - ip - MinMatchLen);

                    if (ip - candidate > SupportedWindowSize.Bytes){
                        break;
                    }

                    ip += matched;
                    lastDistance = @base - candidate;

                    builder.AddCopy(matched, lastDistance);
                    nextEmit = ip;

                    if (ip >= ipLimit){
                        return NextStep.EmitRemainder;
                    }

                    candidate = table.UpdateAndGetCandidate(ip, baseIp);
                }

                nextHash = table.Hash(++ip);
                return NextStep.EmitCommandsNextHash;
            }

            private NextStep EmitRemainder(CompressedMetaBlockBuilder builder){
                nextIpStart += blockSize;
                inputSize -= blockSize;
                blockSize = Math.Min(inputSize, MergeBlockSize);

                if (inputSize > 0 && totalBlockSize + blockSize <= (1 << 20) && ShouldMergeBlock(nextIpStart, blockSize)){
                    totalBlockSize += blockSize;
                    return NextStep.EmitCommands;
                }

                if (nextEmit < ipEnd){
                    int insert = ipEnd - nextEmit;

                    if (insert >= 6210 && ShouldUseUncompressedMode(insert)){
                        outputTo = ipEnd;
                        return NextStep.OutputUncompressed;
                    }
                    else{
                        builder.AddInsertFinal(Literal.FromBytes(input, nextEmit, insert));
                    }
                }

                nextEmit = ipEnd;
                return NextStep.OutputCompressed;
            }

            private void OnNextBlock(){
                if (inputSize > 0){
                    metaBlockStart = nextIpStart;
                    blockSize = Math.Min(inputSize, FirstBlockSize);
                    totalBlockSize = blockSize;
                }
            }

            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (ShouldUseUncompressedMode).
            /// </summary>
            private bool ShouldUseUncompressedMode(int insert){
                int compressed = nextEmit - metaBlockStart;

                if (compressed * 50 > insert){
                    return false;
                }

                return literalRatio > MinRatio;
            }
            
            /// <summary>
            /// Adapted from https://github.com/google/brotli/blob/master/c/enc/compress_fragment.c (ShouldMergeBlock).
            /// </summary>
            private bool ShouldMergeBlock(int ip, int blockSize){
                const int sampleRate = 43;
                FrequencyList<byte> bytes = new FrequencyList<byte>();

                for(int offset = 0; offset < blockSize; offset += sampleRate){
                    bytes.Add(input[ip + offset]);
                }

                int total = (blockSize + sampleRate - 1) / sampleRate;
                double r = ((total == 0 ? 0 : Math.Log(total, 2.0) + 0.5) * total) + 200;

                foreach(var symbol in bytes){
                    var samples = bytes[symbol];
                    var literal = new Literal(symbol);

                    r -= samples * (literalTree!.FindPath(literal).Length + Math.Log(samples, 2.0));
                }

                return r >= 0.0;
            }
        }
    }
}
