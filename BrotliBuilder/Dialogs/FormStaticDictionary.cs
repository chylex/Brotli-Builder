using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BrotliBuilder.Utils;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Brotli.Dictionary.Transform;

namespace BrotliBuilder.Dialogs{
    public partial class FormStaticDictionary : Form{
        private static readonly char FormatSpace = Environment.OSVersion.Version >= new Version(6, 2) ? '⎵' : '␣';
        private const string FormatNewLine = "↵";

        private static readonly NumberFormatInfo FormatCounters = new NumberFormatInfo{
            NumberGroupSeparator = " ",
            NumberDecimalDigits = 0
        };
        
        private readonly DataColumn colLength = new DataColumn("Length", typeof(int));
        private readonly DataColumn colIndex = new DataColumn("Index", typeof(int));
        private readonly DataColumn colTransform = new DataColumn("Transform", typeof(int));
        private readonly DataColumn colText = new DataColumn("Text", typeof(string));
        
        public FormStaticDictionary(BrotliDictionary dict){
            InitializeComponent();
            dataGridViewWords.EnableDoubleBuffering();
            backgroundWorkerLoading.RunWorkerAsync(dict);
        }

        // Dictionary loading

        private void FormStaticDictionary_FormClosing(object sender, FormClosingEventArgs e){
            if (backgroundWorkerLoading.IsBusy){
                backgroundWorkerLoading.CancelAsync();
                e.Cancel = true;
            }
        }

        private void backgroundWorkerLoading_DoWork(object sender, DoWorkEventArgs e){
            BrotliDictionary dict = (BrotliDictionary)e.Argument;

            IDictionaryFormat format = dict.Format;
            IReadOnlyList<WordTransform> transforms = dict.Transforms;

            DataTable table = new DataTable();
            DataRowCollection rows = table.Rows;
            
            int totalProgressPoints = format.WordLengths.Sum(format.WordCount);
            int currentProgressPoints = 0;
            int lastPercentReported = -1;

            table.Columns.AddRange(new DataColumn[]{ colLength, colIndex, colTransform, colText });
            table.MinimumCapacity = totalProgressPoints * transforms.Count;
            table.BeginLoadData();

            foreach(int length in format.WordLengths){
                for(int word = 0, count = format.WordCount(length); word < count; word++){
                    if (backgroundWorkerLoading.CancellationPending){
                        e.Cancel = true;
                        return;
                    }
                    
                    byte[] bytes = dict.ReadRaw(length, word);

                    for(int transform = 0; transform < transforms.Count; transform++){
                        DataRow row = table.NewRow();

                        row[colLength] = length;
                        row[colIndex] = word;
                        row[colTransform] = transform;
                        row[colText] = Encoding.UTF8.GetString(transforms[transform].Process(bytes)).Replace(' ', FormatSpace).Replace("\r\n", FormatNewLine).Replace("\n", FormatNewLine).Replace("\r", "");

                        rows.Add(row);
                    }

                    ++currentProgressPoints;

                    int percentProgress = (int)Math.Floor(100.0 * currentProgressPoints / totalProgressPoints);

                    if (lastPercentReported != percentProgress){
                        lastPercentReported = percentProgress;
                        backgroundWorkerLoading.ReportProgress(percentProgress, currentProgressPoints * transforms.Count);
                    }
                }
            }
            
            table.EndLoadData();
            e.Result = table.DefaultView;
        }

        private void backgroundWorkerLoading_ProgressChanged(object sender, ProgressChangedEventArgs e){
            progressBarLoading.SetValueInstantly(e.ProgressPercentage);
            labelWordCountValue.Text = ((int)e.UserState).ToString("N0", FormatCounters);
        }

        private void backgroundWorkerLoading_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e){
            if (e.Cancelled){
                Close();
            }
            else if (e.Error != null){
                Close();
                throw e.Error;
            }
            else{
                DataView view = (DataView)e.Result;
                
                UpdateCounters(view);
                Refresh();

                Controls.Remove(progressBarLoading);

                for(int index = 0; index < dataGridViewWords.Columns.Count; index++){
                    var column = dataGridViewWords.Columns[index];

                    column.DataPropertyName = view.Table.Columns[index].ColumnName;
                    column.HeaderCell.Style.Alignment = column.DefaultCellStyle.Alignment;
                }
                
                dataGridViewWords.DataSource = view;
                dataGridViewWords.Enabled = true;
                textBoxFilter.Enabled = true;
                checkBoxShowTransforms.Enabled = true;
            }
        }

        // Data filtering

        private void textBoxFilter_TextChanged(object sender, EventArgs e){
            timerFilterUpdate.Start();
        }

        private void timerFilterUpdate_Tick(object sender, EventArgs e){
            timerFilterUpdate.Stop();
            UpdateFilter();
        }

        private void checkBoxShowTransforms_CheckedChanged(object sender, EventArgs e){
            UpdateFilter();
        }

        private void UpdateFilter(){
            string matchingText = textBoxFilter.Text.Replace(' ', FormatSpace);
            bool showTransforms = checkBoxShowTransforms.Checked;
            
            DataTable data = ((DataView)dataGridViewWords.DataSource).Table;
            DataView filtered = data.AsEnumerable().Where(row => ((int)row[colTransform] == 0 || showTransforms) && ((string)row[colText]).Contains(matchingText)).AsDataView();

            dataGridViewWords.DataSource = filtered;
            UpdateCounters(filtered);
        }

        private void UpdateCounters(DataView view){
            int totalWords = view.Count;
            int distinctWords = view.Cast<DataRowView>().Select(row => (string)row.Row[colText]).Distinct().Count();
            
            labelWordCountValue.Text = totalWords.ToString("N0", FormatCounters);
            labelDuplicateCountValue.Text = (totalWords - distinctWords).ToString("N0", FormatCounters);
        }
    }
}
