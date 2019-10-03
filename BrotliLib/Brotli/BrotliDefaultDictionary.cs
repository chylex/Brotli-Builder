using System.Collections.Generic;
using System.Linq;
using System.Text;
using BrotliLib.Brotli.Dictionary;
using BrotliLib.Brotli.Dictionary.Format;
using BrotliLib.Brotli.Dictionary.Source;
using BrotliLib.Brotli.Dictionary.Transform;
using BrotliLib.Resources;

namespace BrotliLib.Brotli{
    /// <summary>
    /// Defines the format and transformations used for the default Brotli dictionary.
    /// https://tools.ietf.org/html/rfc7932#section-8
    /// https://tools.ietf.org/html/rfc7932#appendix-B
    /// </summary>
    public sealed class BrotliDefaultDictionary : BrotliDictionary{
        public static BrotliDefaultDictionary Embedded { get; }

        private static readonly IDictionaryFormat DefaultFormat = new BitDepthFormat(new int[]{
            0, 0, 0, 0, 10, 10, 11, 11, 10, 10, 10, 10, 10, 9, 9, 8, 7, 7, 8, 7, 7, 6, 6, 5, 5
        });

        private static readonly WordTransform[] DefaultTransforms;

        static BrotliDefaultDictionary(){
            DefaultTransforms = Enumerable.Range(0, 121).Select(index => new WordTransform(PrefixMap[index], GetTransformType(index), SuffixMap[index])).ToArray();
            Embedded = new BrotliDefaultDictionary(new StreamSource(Resource.Get("dict"))); 
        }
        
        public BrotliDefaultDictionary(IDictionarySource source) : base(DefaultFormat, DefaultTransforms, source){}

        // Transforms

        private static readonly List<byte[]> PrefixMap = "~~ ~~~~ ~s ~~~~~~, ~~ ~~~e ~~~~~~~~~~~~ ~~.~ ~~~~~~~~ the ~~~~~~~~~~~ ~~~~~~~~~~ the ~~~ ~~.~~~~ ~.com/~ the ~~~~.~~~~ ~~ ~~ ~~~~ ~~ ~~~~~ ~~ ~~~~\xc2\xa0~ ~~~~~~ ~ ~ ~~~~ ~~ ~ ~ ~ "
                                                         .Split('~').Select(Encoding.GetEncoding(28591).GetBytes).ToList(); // Latin1

        private static readonly List<byte[]> SuffixMap = "~ ~ ~~ ~ the ~~ ~ of ~~ and ~~~ ~, ~ ~ in ~ to ~ ~\"~.~\">~\n~~]~ for ~~~ a ~ that ~~. ~~, ~~ with ~\'~ from ~ by ~~~~~. The ~~ on ~ as ~ is ~~ing ~\n\t~:~. ~ed ~~~~(~, ~~ at ~ly ~ of ~~~, ~\"~(~ ~\">~=\"~.~~ of the ~\'~. This ~,~ ~(~.~ not ~=\"~er ~ ~al ~~=\'~\"~. ~(~ful ~. ~ive ~less ~\'~est ~.~\">~=\'~,~ize ~.~~,~=\"~=\"~ous ~, ~=\'~,~=\"~, ~,~(~. ~.~=\'~. ~=\"~=\'~=\'"
                                                         .Split('~').Select(Encoding.GetEncoding(28591).GetBytes).ToList(); // Latin1

        private static readonly HashSet<int> IndFermentFirst = new HashSet<int>{
            4, 9, 15, 30, 58, 65, 66, 69, 74, 78, 79, 88, 91, 96, 99, 104, 108, 109, 118, 120
        };

        private static readonly HashSet<int> IndFermentAll = new HashSet<int>{
            44, 68, 83, 85, 87, 94, 97, 101, 105, 107, 110, 111, 112, 113, 114, 115, 116, 117, 119
        };
        
        private static TransformType GetTransformType(int index){
            if (IndFermentFirst.Contains(index)){
                return TransformType.FermentFirst;
            }

            if (IndFermentAll.Contains(index)){
                return TransformType.FermentAll;
            }

            switch(index){
                case  3: return TransformType.OmitFirstN + 1;
                case 11: return TransformType.OmitFirstN + 2;
                case 12: return TransformType.OmitLastN + 1;
                case 23: return TransformType.OmitLastN + 3;
                case 26: return TransformType.OmitFirstN + 3;
                case 27: return TransformType.OmitLastN + 2;
                case 34: return TransformType.OmitFirstN + 4;
                case 39: return TransformType.OmitFirstN + 5;
                case 40: return TransformType.OmitFirstN + 6;
                case 42: return TransformType.OmitLastN + 4;
                case 48: return TransformType.OmitLastN + 7;
                case 49: return TransformType.OmitLastN + 1;
                case 54: return TransformType.OmitFirstN + 9;
                case 55: return TransformType.OmitFirstN + 7;
                case 56: return TransformType.OmitLastN + 6;
                case 59: return TransformType.OmitLastN + 8;
                case 63: return TransformType.OmitLastN + 5;
                case 64: return TransformType.OmitLastN + 9;
                // all combinations except for (OmitFirstN + 8) are used
            }

            return TransformType.Identity;
        }
    }
}
