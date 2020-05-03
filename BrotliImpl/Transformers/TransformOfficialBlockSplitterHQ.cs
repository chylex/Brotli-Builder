using System;
using System.Collections.Generic;
using System.Linq;
using BrotliLib.Brotli;
using BrotliLib.Brotli.Components;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Brotli.Components.Header;
using BrotliLib.Brotli.Encode;
using BrotliLib.Brotli.Encode.Build;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Collections;
using BrotliLib.Numbers;

namespace BrotliImpl.Transformers{
    class TransformOfficialBlockSplitterHQ : BrotliTransformerCompressed{
        protected override (MetaBlock, BrotliGlobalState) Transform(MetaBlock.Compressed original, BrotliGlobalState state, BrotliCompressionParameters parameters){
            var builder = new CompressedMetaBlockBuilder(original, state);

            var literals = builder.InsertCopyCommands.SelectMany(command => command.Literals).ToList();
            SplitVector(literals, Literal.TreeContext, symbolsPerHistogram: 544, maxHistograms: 100, samplingStrideLength: 70, blockSwitchCost: 28.1);

            return builder.Build(parameters);
        }

        private const int MinLengthForBlockSplitting = 128;
        private const int Iters = 3; // 10 for quality 11

        private void SplitVector<T>(List<T> sequence, HuffmanTree<T>.Context context, int symbolsPerHistogram, int maxHistograms, int samplingStrideLength, double blockSwitchCost) where T : IComparable<T>{
            int sequenceLength = sequence.Count;

            if (sequenceLength < MinLengthForBlockSplitting){
                return;
            }

            int numHistograms = Math.Min(maxHistograms, (sequenceLength / symbolsPerHistogram) + 1);
            var histograms = FrequencyList<T>.Array(numHistograms);

            InitialEntropyCodes(sequence, samplingStrideLength, histograms);
            RefineEntropyCodes(sequence, samplingStrideLength, histograms);

            int bitmapLen = (numHistograms + 7) / 8;
            byte[] blockIds = new byte[sequenceLength];
            double[] insertCost = new double[context.AlphabetSize.SymbolCount * numHistograms];
            double[] cost = new double[numHistograms];
            byte[] switchSignal = new byte[sequenceLength * bitmapLen];
            short[] newId = new short[numHistograms];

            int numBlocks;

            /* TODO
            for(int iter = 0; iter < Iters; iter++){
                numBlocks = FindBlocks(sequence, context, histograms, blockSwitchCost, insertCost, cost, switchSignal, blockIds);
                numHistograms = RemapBlockIds(blockIds, sequenceLength, newId, numHistograms);
                BuildBlockHistograms(sequence, blockIds, numHistograms, histograms);
            }

            ClusterBlocks(sequence, numBlocks, blockIds);*/
        }

        private void InitialEntropyCodes<T>(List<T> sequence, int samplingStrideLength, FrequencyList<T>[] histograms) where T : IComparable<T>{
            int sequenceLength = sequence.Count;
            int numHistograms = histograms.Length;
            
            uint seed = 7;
            int blockLength = sequenceLength / numHistograms;

            for(int i = 0; i < numHistograms; i++){
                int pos = sequenceLength * i / numHistograms;

                if (i != 0){
                    pos += (int)(MyRand(ref seed) % blockLength);
                }

                if (pos + samplingStrideLength >= sequenceLength){
                    pos = sequenceLength - samplingStrideLength - 1;
                }

                HistogramAddVector(histograms[i], sequence, pos, samplingStrideLength);
            }
        }

        private void RefineEntropyCodes<T>(List<T> sequence, int samplingStrideLength, FrequencyList<T>[] histograms) where T : IComparable<T>{
            int sequenceLength = sequence.Count;
            int numHistograms = histograms.Length;

            const int iterMul = 2;
            const int iterAdd = 100;

            uint seed = 7;
            int iters = iterMul * sequenceLength / samplingStrideLength + iterAdd;
            iters = ((iters + numHistograms - 1) / numHistograms) * numHistograms;

            for(int iter = 0; iter < iters; iter++){
                FrequencyList<T> sample = new FrequencyList<T>();
                RandomSample(ref seed, sequence, samplingStrideLength, sample);
                HistogramAddHistogram(histograms[iter % numHistograms], sample);
            }
        }

        private int FindBlocks<T>(List<T> sequence, FrequencyList<T>[] histograms, HuffmanTree<T>.Context context, double blockSwitchCost, double[] insertCost, double[] cost, byte[] switchSignal, byte[] blockIds) where T : IComparable<T>{
            int numHistograms = histograms.Length;
            int bitmapLen = (numHistograms + 7) / 8;

            if (numHistograms <= 1){
                for(int i = 0; i < blockIds.Length; i++){
                    blockIds[i] = 0;
                }

                return 1;
            }

            for(int i = 0; i < insertCost.Length; i++){
                insertCost[i] = 0;
            }

            for(int i = 0; i < cost.Length; i++){
                cost[i] = 0;
            }

            for(int i = 0; i < switchSignal.Length; i++){
                switchSignal[i] = 0;
            }

            for(int i = 0; i < numHistograms; i++){
                insertCost[i] = Log2.Floor(histograms[i].Sum);
            }

            for(int i = context.AlphabetSize.SymbolCount; i != 0;){
                --i;

                for(int j = 0; j < numHistograms; j++){
                    insertCost[i * numHistograms + j] = insertCost[j] - BitCost(histograms[j][context.BitsToSymbol(i)]);
                }
            }

            for(int i = 0; i < sequence.Count; i++){
                // TODO
            }

            return 0;
        }

        private void RandomSample<T>(ref uint seed, List<T> sequence, int samplingStrideLength, FrequencyList<T> sample) where T : IComparable<T>{
            int pos = 0;
            int sequenceLength = sequence.Count;

            if (samplingStrideLength >= sequenceLength){
                samplingStrideLength = sequenceLength;
            }
            else{
                pos = (int)(MyRand(ref seed) % (sequenceLength - samplingStrideLength + 1));
            }

            HistogramAddVector(sample, sequence, pos, samplingStrideLength);
        }

        private static uint MyRand(ref uint seed){
            seed = unchecked(seed * 16807U);
            return seed;
        }

        private static double BitCost(int count){
            return count == 0 ? -2.0 : Log2.Floor(count);
        }

        private static void HistogramAddVector<T>(FrequencyList<T> histogram, List<T> sequence, int start, int count) where T : IComparable<T>{
            for(int index = start; index < start + count; index++){
                histogram.Add(sequence[index]);
            }
        }

        private static void HistogramAddHistogram<T>(FrequencyList<T> histogram, FrequencyList<T> sample) where T : IComparable<T>{
            foreach(var freq in sample.HuffmanFreq){
                histogram[freq.Symbol] += freq.Frequency;
            }
        }
    }
}
