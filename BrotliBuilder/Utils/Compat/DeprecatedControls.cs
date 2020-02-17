using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BrotliBuilder.Utils.Compat.Legacy;
using BrotliBuilder.Utils.Compat.Strip;

namespace BrotliBuilder.Utils.Compat{
    static class DeprecatedControls{
        private static readonly Assembly WinForms = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly?.FullName?.StartsWith("System.Windows.Forms,") == true);

        public static MainMenuBase CreateMainMenu(Form form, IContainer container){
            if (WinForms?.GetType("System.Windows.Forms.MainMenu") == null){
                return new StripMainMenu(form);
            }
            else{
                return new LegacyMainMenu(form, container);
            }
        }

        public static StatusBarBase CreateStatusBar(Form form){
            if (WinForms?.GetType("System.Windows.Forms.StatusBar") == null){
                return new StripStatusBar(form);
            }
            else{
                return new LegacyStatusBar(form);
            }
        }
    }
}
