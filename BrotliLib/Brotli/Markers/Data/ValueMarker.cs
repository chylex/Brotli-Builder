using System;
using System.Text;
using BrotliLib.Brotli.Components.Data;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Data{
    sealed class ValueMarker : IMarkerInfo{
        public bool IsBold => false;

        private readonly string name, value;
        
        public ValueMarker(string name, object value){
            this.name = string.Intern(name);

            TypeCode type = Type.GetTypeCode(value.GetType());

            if (type == TypeCode.Object && !(value is Literal)){
                this.value = string.Intern("{ " + value + " }");
            }
            else if (type == TypeCode.Boolean){
                this.value = string.Intern(((bool)value ? "TRUE" : "FALSE"));
            }
            else{
                this.value = string.Intern(value.ToString());
            }
        }

        public void ToString(StringBuilder build){
            build.Append(name).Append(" = ").Append(value);
        }
        
        public override string ToString(){
            return name + " = " + value;
        }
    }
}
