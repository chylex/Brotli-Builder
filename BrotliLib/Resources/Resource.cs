using System.IO;
using System.Reflection;

namespace BrotliLib.Resources{
    static class Resource{
        public static Stream Get(string name){
            return typeof(Resource).GetTypeInfo().Assembly.GetManifestResourceStream("BrotliLib.Resources." + name);
        }
    }
}
