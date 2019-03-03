﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli;
using BrotliLib.IO;

namespace BrotliBuilder.Components{
    public partial class BrotliFilePanel : UserControl{
        public string LabelPrefix{
            get{
                return labelPrefix;
            }

            set{
                labelPrefix = value;
                labelBitStream.Text = $"{value} Bit Stream";
                labelOutput.Text = $"{value} Output";
                loadWorker.Name = $"BrotliFilePanel ({value})";
            }
        }

        public bool WordWrapOutput{
            set => textBoxOutput.WordWrap = value;
        }
        
        public int MaxLength{
            set{
                textBoxBitStream.MaxDisplayedLength = value;
                textBoxOutput.MaxDisplayedLength = value;
            }
        }

        private readonly AsyncWorker loadWorker = new AsyncWorker();
        
        private string labelPrefix = null;

        public BrotliFilePanel(){
            InitializeComponent();
            textBoxOutput.SetPlainTextMode();
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

                sync(() => {
                    UpdateTextBox(textBoxOutput, outputStr);
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

                sync(() => {
                    UpdateTextBox(textBoxOutput, outputStr);
                    onDecompressed(stopwatchDecompression);
                });
            });
        }

        public void InvalidatePanel(){
            loadWorker.Abort();

            textBoxBitStream.ForeColor = SystemColors.ControlDark;
            textBoxOutput.ForeColor = SystemColors.ControlDark;
        }

        private void UpdateTextBox(RicherTextBox tb, string text, Color color){
            tb.ForeColor = color;
            tb.Text = text;
            tb.Select(0, 0);
        }
        
        private void UpdateTextBox(RicherTextBox tb, string text){
            UpdateTextBox(tb, text, SystemColors.WindowText);
        }

        private void UpdateTextBox(RicherTextBox tb, Exception ex){
            UpdateTextBox(tb, Regex.Replace(ex.ToString(), " in (.*):", " : "), Color.Red);
        }
    }
}