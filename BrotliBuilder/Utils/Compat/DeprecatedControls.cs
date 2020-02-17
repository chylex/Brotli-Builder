using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using BrotliBuilder.Utils.Compat.Legacy;
using BrotliBuilder.Utils.Compat.Strip;

namespace BrotliBuilder.Utils.Compat{
    static class DeprecatedControls{
        public static MainMenuBase CreateMainMenu(Form form, IContainer container){
            var winForms = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(assembly => assembly?.FullName?.StartsWith("System.Windows.Forms,") == true);

            if (winForms?.GetType("System.Windows.Forms.MainMenu") == null){
                return new StripMainMenu(form);
            }
            else{
                return new LegacyMainMenu(form, container);
            }
        }
    }
}
