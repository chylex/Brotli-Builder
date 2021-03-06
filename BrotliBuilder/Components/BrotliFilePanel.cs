﻿using System;
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
                return title!;
            }

            set{
                title = value;
                textBoxBitStream.MarkerTitle = value;
                ResetLabels();
            }
        }

        public bool WordWrapOutput{
            set => textBoxOutput.WordWrap = value;
        }

        public MarkerRoot? MarkerRoot => textBoxBitStream.MarkerRoot;

        public event EventHandler<MarkedTextBox.MarkerUpdateEventArgs> MarkersUpdated{
            add => textBoxBitStream.MarkersUpdated += value;
            remove => textBoxBitStream.MarkersUpdated -= value;
        }

        private string? title;

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

        public void UpdateMarkers(BrotliFileState.HasMarkers state){
            textBoxBitStream.UpdateMarkers(state.MarkerRoot, state.Markers);
        }

        public void UpdateOutput(Exception ex){
            UpdateTextBox(textBoxOutput, ex);
        }

        public void UpdateOutput(BrotliFileState.HasOutput state){
            UpdateTextBox(textBoxOutput, Encoding.UTF8.GetString(state.OutputBytes));
        }

        public void FinalizeOutput(BrotliFileState.Loaded state){
            UpdateLabels(state.TotalCompressedBits, state.TotalOutputBytes);
        }

        private void UpdateTextBox(FastColoredTextBox tb, string text, Color color){
            tb.ForeColor = color;
            tb.Text = text;
        }
        
        private void UpdateTextBox(FastColoredTextBox tb, string text){
            UpdateTextBox(tb, text, SystemColors.WindowText);
        }

        private void UpdateTextBox(FastColoredTextBox tb, Exception ex){
            UpdateTextBox(tb, Regex.Replace(ex.ToString(), " in (.*):", " : "), Color.Red);
        }

        private void UpdateLabels(int bitStreamLength, int outputLength){
            int bitStreamBytes = (7 + bitStreamLength) / 8;

            static string Number(int n){
                return n.ToString("N0", Program.Culture);
            }

            labelBitStream.Text = $"{Title} Bit Stream ({Number(bitStreamLength)} bit{(bitStreamLength == 1 ? "" : "s")} / {Number(bitStreamBytes)} byte{(bitStreamBytes == 1 ? "" : "s")})";
            labelOutput.Text = $"{Title} Output ({Number(outputLength)} bytes)";
        }

        private void ResetLabels(){
            labelBitStream.Text = $"{Title} Bit Stream";
            labelOutput.Text = $"{Title} Output";
        }
    }
}
