using System;
using System.Windows.Forms;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Parameters.Heuristics;
using static BrotliLib.Brotli.Parameters.BrotliSerializationParameters;

namespace BrotliBuilder.Dialogs{
    public partial class FormSerializationParameters : Form{
        public event EventHandler<BrotliSerializationParameters>? Updated;
        public event EventHandler? Reserialize;

        private readonly Builder parameters;

        public FormSerializationParameters(BrotliSerializationParameters parameters){
            InitializeComponent();

            this.parameters = new Builder(parameters);
            this.Disposed += FormParameters_Disposed;

            LoadOptions();
            SetupOptionEvents();
        }

        private void FormParameters_Disposed(object? sender, EventArgs e){
            Reserialize = null;
        }

        private void buttonReserialize_Click(object sender, EventArgs e){
            Reserialize?.Invoke(this, e);
        }

        // Option handling

        private static readonly DecideComplexTreeFeature ComplexTreeFeatureEnable  = _ => true;
        private static readonly DecideComplexTreeFeature ComplexTreeFeatureDisable = _ => false;

        private void SetupOptionEvents(){
            this.checkBoxComplexTreeSkipCode.CheckedChanged   += OnOptionChanged;
            this.checkBoxComplexTreeRepeatCode.CheckedChanged += OnOptionChanged;

            this.checkBoxContextMapMTF.CheckedChanged += OnOptionChanged;
            this.checkBoxContextMapRLE.CheckedChanged += OnOptionChanged;
        }

        private void LoadOptions(){
            checkBoxComplexTreeSkipCode.Checked   = !ReferenceEquals(parameters.UseComplexTreeSkipCode,   ComplexTreeFeatureDisable);
            checkBoxComplexTreeRepeatCode.Checked = !ReferenceEquals(parameters.UseComplexTreeRepeatCode, ComplexTreeFeatureDisable);

            checkBoxContextMapMTF.Checked = !ReferenceEquals(parameters.ContextMapMTF, ContextMapHeuristics.MTF.Disable);
            checkBoxContextMapRLE.Checked = !ReferenceEquals(parameters.ContextMapRLE, ContextMapHeuristics.RLE.Disable);
        }

        private void OnOptionChanged(object? sender, EventArgs e){
            parameters.UseComplexTreeSkipCode   = checkBoxComplexTreeSkipCode.Checked   ? ComplexTreeFeatureEnable : ComplexTreeFeatureDisable;
            parameters.UseComplexTreeRepeatCode = checkBoxComplexTreeRepeatCode.Checked ? ComplexTreeFeatureEnable : ComplexTreeFeatureDisable;

            parameters.ContextMapMTF = checkBoxContextMapMTF.Checked ? ContextMapHeuristics.MTF.Enable : ContextMapHeuristics.MTF.Disable;
            parameters.ContextMapRLE = checkBoxContextMapRLE.Checked ? ContextMapHeuristics.RLE.KeepAll : ContextMapHeuristics.RLE.Disable;

            Updated?.Invoke(this, parameters.Build());
        }
    }
}
