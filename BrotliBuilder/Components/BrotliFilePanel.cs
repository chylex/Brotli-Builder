﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;
using BrotliLib.IO;
using BrotliLib.Markers;
using FastColoredTextBoxNS;

namespace BrotliBuilder.Components{
    public partial class BrotliFilePanel : UserControl{
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
                BrotliGlobalState state;

                try{
                    state = file.GetDecompressionState(bits);
                }catch(Exception ex){
                    sync(() => UpdateTextBox(textBoxOutput, ex));
                    return;
                }

                string outputStr = state.OutputAsUTF8;
                MarkerNode[] markerSequence = state.BitMarkerRoot.ToArray();

                int totalBits = (int)markerSequence.Last().Marker.IndexEnd; // use markers to account for padding

                sync(() => {
                    textBoxBitStream.UpdateMarkers(markerSequence);
                    UpdateTextBox(textBoxOutput, outputStr);
                    UpdateLabels(totalBits, state.OutputSize);
                    callback(file);
                });
            });
        }

        public void LoadBrotliFile(BrotliFileStructure file, Action<Stopwatch> onSerialized, Action<Stopwatch> onDecompressed){
            InvalidatePanel();

            loadWorker.Start(sync => {
                BitStream bits;
                BrotliGlobalState state;

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
                    state = file.GetDecompressionState(bits);
                }catch(Exception ex){
                    sync(() => {
                        UpdateTextBox(textBoxOutput, ex);
                        onDecompressed(null);
                    });

                    return;
                }finally{
                    stopwatchDecompression.Stop();
                }

                string outputStr = state.OutputAsUTF8;
                MarkerNode[] markerSequence = state.BitMarkerRoot.ToArray();

                sync(() => {
                    textBoxBitStream.UpdateMarkers(markerSequence);
                    UpdateTextBox(textBoxOutput, outputStr);
                    UpdateLabels(bits.Length, state.OutputSize);
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
