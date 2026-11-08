using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace NovaBrowser {
    static class Program {
        [STAThread]
        static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Handler cerdas untuk menemukan DLL WebView2 di folder lib/ meskipun tanpa file .config
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveArgs) => {
                try {
                    string assemblyName = new AssemblyName(resolveArgs.Name).Name;
                    string appDir = AppDomain.CurrentDomain.BaseDirectory;

                    // 1. Cek di subfolder lib/
                    string pathInLib = Path.Combine(appDir, "lib", assemblyName + ".dll");
                    if (File.Exists(pathInLib)) {
                        return Assembly.LoadFrom(pathInLib);
                    }

                    // 2. Cek di root direktori aplikasi
                    string pathInRoot = Path.Combine(appDir, assemblyName + ".dll");
                    if (File.Exists(pathInRoot)) {
                        return Assembly.LoadFrom(pathInRoot);
                    }

                    // 3. Fallback jika dibuka dari shortcut atau path dev
                    string pathInDev = Path.Combine(@"c:\xampp\htdocs\browser\lib", assemblyName + ".dll");
                    if (File.Exists(pathInDev)) {
                        return Assembly.LoadFrom(pathInDev);
                    }
                } catch { }

                return null;
            };

            Application.ThreadException += (s, e) => {
                MessageBox.Show("Terjadi kesalahan:\n" + e.Exception.Message, "NovaLite Browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) => {
                MessageBox.Show("Kesalahan fatal sistem:\n" + (e.ExceptionObject != null ? e.ExceptionObject.ToString() : "Unknown"), "NovaLite Browser", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            // Pemeriksaan awal ketersediaan pustaka WebView2 sebelum form di-load
            if (!CheckPrerequisites()) {
                return;
            }

            RunApplication(args);
        }

        private static bool CheckPrerequisites() {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string dllInLib = Path.Combine(appDir, "lib", "Microsoft.Web.WebView2.WinForms.dll");
            string dllInRoot = Path.Combine(appDir, "Microsoft.Web.WebView2.WinForms.dll");
            string dllInDev = @"c:\xampp\htdocs\browser\lib\Microsoft.Web.WebView2.WinForms.dll";

            if (!File.Exists(dllInLib) && !File.Exists(dllInRoot) && !File.Exists(dllInDev)) {
                MessageBox.Show(
                    "⚠️ Pustaka WebView2 Tidak Ditemukan!\n\n" +
                    "Aplikasi 'NovaBrowser.exe' memerlukan folder 'lib' untuk dapat berjalan.\n\n" +
                    "Kemungkinan penyebab:\n" +
                    "• Anda memindahkan file 'NovaBrowser.exe' sendirian tanpa menyertakan folder 'lib'.\n\n" +
                    "Solusi:\n" +
                    "1. Pastikan folder 'lib' berada di samping 'NovaBrowser.exe', ATAU\n" +
                    "2. Gunakan paket lengkap 'NovaBrowser-Windows-Portable.zip'.",
                    "NovaBrowser - Folder 'lib' Kurang",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return false;
            }
            return true;
        }

        // Dipisahkan ke method terpisah agar JIT compiler tidak mencoba me-resolve WebView2 sebelum AssemblyResolve terpasang
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
        private static void RunApplication(string[] args) {
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

