using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BrotliCalc.Helpers{
    abstract class Table{
        protected readonly IList<string> columns;

        protected Table(string[] columns){
            this.columns = columns;
        }

        protected IEnumerable<string> ReadRowAsString(object?[] values){
            if (values.Length != columns.Count){
                throw new ArgumentException("Amount of entries must match the amount of columns (" + values.Length + " != " + columns.Count + ").", nameof(values));
            }

            return values.Select(value => value?.ToString() ?? "?");
        }

        public abstract void AddRow(params object?[] values);

        internal class CSV : Table, IDisposable{
            private readonly StreamWriter writer;

            public CSV(string path, string[] columns) : base(columns){
                this.writer = new StreamWriter(path);
                this.writer.WriteLine(string.Join(',', this.columns));
                this.writer.Flush();
            }

            public override void AddRow(params object?[] values){
                writer.Write('"');
                writer.Write(string.Join("\",\"", ReadRowAsString(values)));
                writer.Write('"');
                writer.WriteLine();
                writer.Flush();
            }

            public void Dispose(){
                writer.Dispose();
            }
        }
    }
}
