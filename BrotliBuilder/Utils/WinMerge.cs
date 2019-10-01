using System;
using System.Diagnostics;
using System.IO;

namespace BrotliBuilder.Utils{
    static class WinMerge{
        public static void CompareText(string originalText, string generatedText){
            string folder = Path.Combine(Path.GetTempPath(), "BrotliBuilder_Markers_" + Path.GetRandomFileName());
            string originalFile = Path.Combine(folder, "original.txt");
            string generatedFile = Path.Combine(folder, "generated.txt");
                
            Directory.CreateDirectory(folder);
            File.WriteAllText(originalFile, originalText);
            File.WriteAllText(generatedFile, generatedText);

            string[] winMergePaths = {
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WinMerge", "WinMergeU.exe"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "WinMerge", "WinMergeU.exe"),
                "WinMergeU.exe"
            };

            string winMergeArgs = "/E /U /DL Generated /DR Original \"" + generatedFile + "\" \"" + originalFile + "\"";

            bool anySuccess = false;

            foreach(string path in winMergePaths){
                try{
                    using(Process.Start(path, winMergeArgs)){}
                    anySuccess = true;
                    break;
                }catch(Exception){
                    // ignore
                }
            }

            if (!anySuccess){
                using(Process.Start("explorer.exe", folder)){}
            }
        }
    }
}
