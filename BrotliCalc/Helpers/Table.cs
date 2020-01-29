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

        protected IEnumerable<string> ReadRowAsStrings(object?[] values){
            if (values.Length != columns.Count){
                throw new ArgumentException($"Amount of entries must match the amount of columns ({values.Length} != {columns.Count}).", nameof(values));
            }

            return values.Select(RowValueToString);
        }

        public abstract void AddRow(params object?[] values);
        protected abstract string RowValueToString(object? value);

        internal class CSV : Table, IDisposable{
            private static readonly HashSet<TypeCode> OmitQuotes = new HashSet<TypeCode>{
                TypeCode.SByte, TypeCode.Byte,
                TypeCode.Int16, TypeCode.UInt16,
                TypeCode.Int32, TypeCode.UInt32,
                TypeCode.Int64, TypeCode.UInt64,
                TypeCode.Single,
                TypeCode.Double,
                TypeCode.Decimal
            };

            private static bool CanOmitQuotes(object value){
                return OmitQuotes.Contains(Type.GetTypeCode(value.GetType()));
            }

            private readonly StreamWriter writer;

            public CSV(string path, string[] columns) : base(columns){
                this.writer = new StreamWriter(path);
                this.writer.WriteLine(string.Join(',', this.columns));
                this.writer.Flush();
            }

            protected override string RowValueToString(object? value){
                return value == null ? "?" : CanOmitQuotes(value) ? value.ToString()! : $"\"{value}\"";
            }

            public override void AddRow(params object?[] values){
                writer.Write(string.Join(",", ReadRowAsStrings(values)));
                writer.WriteLine();
                writer.Flush();
            }

            public void Dispose(){
                writer.Dispose();
            }
        }
    }
}
