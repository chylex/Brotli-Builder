﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using BrotliBuilder.State;
using BrotliLib.Markers;
using FastColoredTextBoxNS;

namespace BrotliBuilder.Components{
    partial class BrotliFilePanel : UserControl{
        public string Title{
            get{
                return title;
            }

            set{
                title = value;
                ResetLabels();
            }
        }

        public bool WordWrapOutput{
            set => textBoxOutput.WordWrap = value;
        }

        public IList<MarkerNode> MarkerSequence => textBoxBitStream.MarkerSequence;

        public event EventHandler<MarkedTextBox.MarkerUpdateEventArgs> MarkersUpdated{
            add => textBoxBitStream.MarkersUpdated += value;
            remove => textBoxBitStream.MarkersUpdated -= value;
        }

        private string title = null;

        public BrotliFilePanel(){
            InitializeComponent();
        }

        public void ResetPanel(){
            UpdateTextBox(textBoxBitStream, "");
            UpdateTextBox(textBoxOutput, "");
            ResetLabels();
        }

        public void InvalidatePanel(){
            textBoxBitStream.ForeColor = SystemColors.ControlDark;
            textBoxOutput.ForeColor = SystemColors.ControlDark;

            textBoxBitStream.RemoveMarkers();
            ResetLabels();
        }

        public void UpdateBits(Exception ex){
            UpdateTextBox(textBoxOutput, ex);
        }

        public void UpdateBits(BrotliFileState.HasBits state){
            UpdateTextBox(textBoxBitStream, state.Bits);
        }

        public void UpdateOutput(Exception ex){
            UpdateTextBox(textBoxOutput, ex);
        }

        public void UpdateOutput(BrotliFileState.HasOutput state){
            UpdateTextBox(textBoxOutput, Encoding.UTF8.GetString(state.OutputBytes));
        }

        public void FinalizeOutput(BrotliFileState.Loaded state){
            UpdateLabels(state.TotalCompressedBits, state.TotalOutputBytes);
            textBoxBitStream.UpdateMarkers(state.Markers);
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
            labelBitStream.Text = $"{title} Bit Stream ({bitStreamLength} bits)";
            labelOutput.Text = $"{title} Output ({outputLength} bytes)";
        }

        private void ResetLabels(){
            labelBitStream.Text = $"{title} Bit Stream";
            labelOutput.Text = $"{title} Output";
        }
    }
}
