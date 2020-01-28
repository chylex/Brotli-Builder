using System;
using System.Windows.Forms;
using BrotliLib.Brotli.Parameters;
using DecideComplexTreeFeature = BrotliLib.Brotli.Parameters.BrotliSerializationParameters.DecideComplexTreeFeature;
using DecideContextMapFeature = BrotliLib.Brotli.Parameters.BrotliSerializationParameters.DecideContextMapFeature;

namespace BrotliBuilder.Dialogs{
    public partial class FormSerializationParameters : Form{
        public event EventHandler<BrotliSerializationParameters>? Updated;
        public event EventHandler? Reserialize;

        private readonly BrotliSerializationParameters.Builder parameters;

        public FormSerializationParameters(BrotliSerializationParameters parameters){
            InitializeComponent();

            this.parameters = new BrotliSerializationParameters.Builder(parameters);
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

        private static readonly DecideContextMapFeature ContextMapFeatureEnable  = _ => true;
        private static readonly DecideContextMapFeature ContextMapFeatureDisable = _ => false;

        private void SetupOptionEvents(){
            this.checkBoxComplexTreeSkipCode.CheckedChanged   += OnOptionChanged;
            this.checkBoxComplexTreeRepeatCode.CheckedChanged += OnOptionChanged;

            this.checkBoxContextMapIMTF.CheckedChanged += OnOptionChanged;
            this.checkBoxContextMapRLE.CheckedChanged  += OnOptionChanged;
        }

        private void LoadOptions(){
            checkBoxComplexTreeSkipCode.Checked   = !ReferenceEquals(parameters.UseComplexTreeSkipCode,   ComplexTreeFeatureDisable);
            checkBoxComplexTreeRepeatCode.Checked = !ReferenceEquals(parameters.UseComplexTreeRepeatCode, ComplexTreeFeatureDisable);

            checkBoxContextMapIMTF.Checked = !ReferenceEquals(parameters.UseContextMapIMTF, ContextMapFeatureDisable);
            checkBoxContextMapRLE.Checked  = !ReferenceEquals(parameters.UseContextMapRLE,  ContextMapFeatureDisable);
        }

        private void OnOptionChanged(object? sender, EventArgs e){
            parameters.UseComplexTreeSkipCode   = checkBoxComplexTreeSkipCode.Checked   ? ComplexTreeFeatureEnable : ComplexTreeFeatureDisable;
            parameters.UseComplexTreeRepeatCode = checkBoxComplexTreeRepeatCode.Checked ? ComplexTreeFeatureEnable : ComplexTreeFeatureDisable;

            parameters.UseContextMapIMTF = checkBoxContextMapIMTF.Checked ? ContextMapFeatureEnable : ContextMapFeatureDisable;
            parameters.UseContextMapRLE  = checkBoxContextMapRLE.Checked  ? ContextMapFeatureEnable : ContextMapFeatureDisable;

            Updated?.Invoke(this, parameters.Build());
        }
    }
}
