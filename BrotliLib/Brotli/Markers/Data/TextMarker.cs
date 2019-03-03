using System;
using BrotliLib.Markers;

namespace BrotliLib.Brotli.Markers.Data{
    class TextMarker : IMarkerInfo{
        public bool IsBold => false;

        private readonly string text;

        public TextMarker(string text){
            this.text = text;
        }
        
        public TextMarker(string name, object value){
            TypeCode type = Type.GetTypeCode(value.GetType());

            if (type == TypeCode.Object){
                this.text = name + " = { " + value + " }";
            }
            else if (type == TypeCode.Boolean){
                this.text = name + ((bool)value ? " = TRUE" : " = FALSE");
            }
            else{
                this.text = name + " = " + value;
            }
        }
        
        public override string ToString(){
            return text;
        }
    }
}
