using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;

namespace BrotliBuilder{
    static class Program{
        public static CultureInfo Culture { get; }

        static Program(){
            Culture = Thread.CurrentThread.CurrentCulture;

            Thread.CurrentThread.CurrentCulture = CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        }

        [STAThread]
        private static void Main(){
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
