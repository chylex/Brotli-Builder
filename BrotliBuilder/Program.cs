using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace BrotliBuilder{
    static class Program{
        [STAThread]
        private static void Main(){
            Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
