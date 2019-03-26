using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrotliCalc.Helpers{
    sealed class Table{
        private readonly string[] columns;
        private readonly List<string[]> rows = new List<string[]>();

        public Table(string[] columns){
            this.columns = columns;
        }

        public void AddRow(params object[] values){
            if (values.Length != columns.Length){
                throw new ArgumentException("Amount of entries must match the amount of columns (" + values.Length + " != " + columns.Length + ").", nameof(values));
            }

            rows.Add(values.Select(value => value.ToString()).ToArray());
        }

        public void WriteCSV(string path){
            using(StreamWriter writer = new StreamWriter(path)){
                writer.WriteLine(string.Join(',', columns));

                foreach(string[] row in rows){
                    writer.Write('"');
                    writer.WriteLine(string.Join("\",\"", row));
                    writer.Write('"');
                }
            }
        }
    }
}
