using System;
using System.Windows.Forms;

namespace NovaBrowser {
    static class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (s, e) => {
                MessageBox.Show("Terjadi kesalahan:\n" + e.Exception.Message, "NovaLite Browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                MessageBox.Show("Kesalahan fatal sistem:\n" + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown"), "NovaLite Browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            string appUrl = null;
            bool isAppMode = false;

            if (args != null && args.Length > 0) {
                foreach (string arg in args) {
                    if (arg.StartsWith("--app=", StringComparison.OrdinalIgnoreCase)) {
                        appUrl = arg.Substring(6).Trim('"', '\'');
                        isAppMode = true;
                    } else if (!arg.StartsWith("-") && !arg.StartsWith("/")) {
                        appUrl = arg;
                    }
                }
            }

            Application.Run(new MainForm(appUrl, isAppMode));
        }
    }
}

