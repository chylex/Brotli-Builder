﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;
using BrotliLib.Brotli.State.Output;
using BrotliLib.IO;
using BrotliLib.Markers;
using FastColoredTextBoxNS;

namespace BrotliBuilder.Components{
    public partial class BrotliFilePanel : UserControl{
        private const bool EnableBitMarkers = true;

        public string LabelPrefix{
            get{
                return labelPrefix;
            }

            set{
                labelPrefix = value;
                loadWorker.Name = $"BrotliFilePanel ({value})";
                ResetLabels();
            }
        }

        public bool WordWrapOutput{
            set => textBoxOutput.WordWrap = value;
        }

        private readonly AsyncWorker loadWorker = new AsyncWorker();
        
        private string labelPrefix = null;

        public BrotliFilePanel(){
            InitializeComponent();
        }

        public void LoadBrotliFile(byte[] bytes, Action<BrotliFileStructure> callback){
            InvalidatePanel();

            loadWorker.Start(sync => {
                BitStream bits = new BitStream(bytes);
                string bitsStr = bits.ToString();

                sync(() => UpdateTextBox(textBoxBitStream, bitsStr));
                
                BrotliFileStructure file = BrotliFileStructure.FromBytes(bytes);
                BrotliOutputStored output;

                try{
                    output = file.GetDecompressionState(bits, EnableBitMarkers);
                }catch(Exception ex){
                    sync(() => UpdateTextBox(textBoxOutput, ex));
                    return;
                }

                string outputStr = output.AsUTF8;
                MarkerNode[] markerSequence = output.BitMarkerRoot.ToArray();

                int totalBits = markerSequence.LastOrDefault()?.Marker?.IndexEnd ?? bits.Length; // use markers to account for padding

                sync(() => {
                    textBoxBitStream.UpdateMarkers(markerSequence);
                    UpdateTextBox(textBoxOutput, outputStr);
                    UpdateLabels(totalBits, output.OutputSize);
                    callback(file);
                });
            });
        }

        public void LoadBrotliFile(Func<BrotliFileStructure> fileLoader, Action<BrotliFileStructure> onLoaded, Action<Stopwatch> onSerialized, Action<Stopwatch> onDecompressed){
            InvalidatePanel();

            loadWorker.Start(sync => {
                BrotliFileStructure file = fileLoader();

                if (file == null){
                    sync(() => {
                        onSerialized(null);
                        onDecompressed(null);
                    });

                    return;
                }

                sync(() => onLoaded(file));

                BitStream bits;
                BrotliOutputStored output;

                Stopwatch stopwatchSerialization = Stopwatch.StartNew();

                try{
                    bits = file.Serialize();
                }catch(Exception ex){
                    sync(() => {
                        UpdateTextBox(textBoxBitStream, ex);
                        onSerialized(null);
                        onDecompressed(null);
                    });

                    return;
                }finally{
                    stopwatchSerialization.Stop();
                }

                string bitsStr = bits.ToString();

                sync(() => {
                    UpdateTextBox(textBoxBitStream, bitsStr);
                    onSerialized(stopwatchSerialization);
                });
                
                Stopwatch stopwatchDecompression = Stopwatch.StartNew();
                
                try{
                    output = file.GetDecompressionState(bits, EnableBitMarkers);
                }catch(Exception ex){
                    sync(() => {
                        UpdateTextBox(textBoxOutput, ex);
                        onDecompressed(null);
                    });

                    return;
                }finally{
                    stopwatchDecompression.Stop();
                }

                string outputStr = output.AsUTF8;
                MarkerNode[] markerSequence = output.BitMarkerRoot.ToArray();

                sync(() => {
                    textBoxBitStream.UpdateMarkers(markerSequence);
                    UpdateTextBox(textBoxOutput, outputStr);
                    UpdateLabels(bits.Length, output.OutputSize);
                    onDecompressed(stopwatchDecompression);
                });
            });
        }

        public void InvalidatePanel(){
            loadWorker.Abort();

            textBoxBitStream.ForeColor = SystemColors.ControlDark;
            textBoxOutput.ForeColor = SystemColors.ControlDark;

            textBoxBitStream.RemoveMarkers();
            ResetLabels();
        }

        private void UpdateTextBox(FastColoredTextBox tb, string text, Color color){
            tb.ForeColor = color;
            tb.Text = text;
            tb.Navigate(0);
        }
        
        private void UpdateTextBox(FastColoredTextBox tb, string text){
            UpdateTextBox(tb, text, SystemColors.WindowText);
        }

        private void UpdateTextBox(FastColoredTextBox tb, Exception ex){
            UpdateTextBox(tb, Regex.Replace(ex.ToString(), " in (.*):", " : "), Color.Red);
        }

        private void UpdateLabels(int bitStreamLength, int outputLength){
            labelBitStream.Text = $"{labelPrefix} Bit Stream ({bitStreamLength} bits)";
            labelOutput.Text = $"{labelPrefix} Output ({outputLength} bytes)";
        }

        private void ResetLabels(){
            labelBitStream.Text = $"{labelPrefix} Bit Stream";
            labelOutput.Text = $"{labelPrefix} Output";
        }
    }
}
