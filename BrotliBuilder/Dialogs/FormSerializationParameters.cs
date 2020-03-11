using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using BrotliLib.Brotli.Parameters;
using BrotliLib.Brotli.Parameters.Heuristics;
using static BrotliLib.Brotli.Parameters.BrotliSerializationParameters;

namespace BrotliBuilder.Dialogs{
    public partial class FormSerializationParameters : Form{
        public event EventHandler<BrotliSerializationParameters>? Updated;
        public event EventHandler? Reserialize;

        private readonly Builder parameters;
        private readonly Dictionary<RadioButton, ContextMapHeuristics.DecideRuns> optionsContextMapsRLE;

        public FormSerializationParameters(BrotliSerializationParameters parameters){
            InitializeComponent();

            this.parameters = new Builder(parameters);
            this.buttonReserialize.Click += buttonReserialize_Click;
            this.Disposed += FormParameters_Disposed;

            this.optionsContextMapsRLE = new Dictionary<RadioButton, ContextMapHeuristics.DecideRuns>{
                { radioContextMapsRleDisable,  ContextMapHeuristics.RLE.Disable },
                { radioContextMapsRleKeepAll,  ContextMapHeuristics.RLE.KeepAll },
                { radioContextMapsRleSplit1AB, ContextMapHeuristics.RLE.SplitOneAboveBoundary },
            };

            LoadOptions();
            SetupOptionEvents();
        }

        private void FormParameters_Disposed(object? sender, EventArgs e){
            Reserialize = null;
        }

        private void buttonReserialize_Click(object? sender, EventArgs e){
            Reserialize?.Invoke(this, e);
        }

        // Option handling

        private void SetupOptionEvents(){

            this.checkBoxContextMapMTF.CheckedChanged += OnOptionChanged;
            this.radioContextMapsRleDisable.CheckedChanged += OnOptionChanged;
            this.radioContextMapsRleKeepAll.CheckedChanged += OnOptionChanged;
            this.radioContextMapsRleSplit1AB.CheckedChanged += OnOptionChanged;
        }

        private void ProcessRadio<T>(Dictionary<RadioButton, T> dictionary, Func<KeyValuePair<RadioButton, T>, bool> predicate, Action<KeyValuePair<RadioButton, T>> action){
            foreach(var kvp in dictionary.Where(predicate)){
                action(kvp);
                break;
            }
        }

        private void LoadOptions(){
            ProcessRadio(optionsContextMapsRLE,  kvp => ReferenceEquals(parameters.ContextMapRLE, kvp.Value),  kvp => kvp.Key.Checked = true);

            checkBoxContextMapMTF.Checked = !ReferenceEquals(parameters.ContextMapMTF, ContextMapHeuristics.MTF.Disable);
        }

        private void OnOptionChanged(object? sender, EventArgs e){
            ProcessRadio(optionsContextMapsRLE,  kvp => kvp.Key.Checked, kvp => parameters.ContextMapRLE = kvp.Value);

            parameters.ContextMapMTF = checkBoxContextMapMTF.Checked ? ContextMapHeuristics.MTF.Enable : ContextMapHeuristics.MTF.Disable;

            Updated?.Invoke(this, parameters.Build());
        }
    }
}
