using System;
using System.Diagnostics;
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

            // Tangani proses uninstall
            if (args != null && args.Length > 0 && Array.Exists(args, a => a.Equals("--uninstall", StringComparison.OrdinalIgnoreCase))) {
                PerformUninstall();
                return;
            }

            // Pemeriksaan awal ketersediaan pustaka WebView2 sebelum form di-load
            if (!CheckPrerequisites()) {
                return;
            }

            RunApplication(args);
        }

        private static void PerformUninstall() {
            var confirm = MessageBox.Show(
                "Apakah Anda yakin ingin menghapus NovaBrowser dari komputer Anda?",
                "Penghapusan NovaBrowser",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );
            if (confirm != DialogResult.Yes) return;

            try {
                // 1. Hapus Desktop Shortcut
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string lnkDesktop = Path.Combine(desktopPath, "NovaBrowser.lnk");
                if (File.Exists(lnkDesktop)) File.Delete(lnkDesktop);

                // 2. Hapus Start Menu Shortcut
                string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs", "NovaBrowser");
                if (Directory.Exists(startMenu)) Directory.Delete(startMenu, true);

                // 3. Hapus Registry Entry
                using (var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", true)) {
                    if (key != null) key.DeleteSubKeyTree("NovaBrowser", false);
                }

                // 4. Jadwalkan pembersihan folder instalasi setelah proses keluar
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string cleanupBat = Path.Combine(Path.GetTempPath(), "cleanup_novabrowser.bat");
                string batContent = string.Format(
@"@echo off
timeout /t 1 /nobreak >nul
rmdir /s /q ""{0}""
del ""%~f0""
", appDir);
                File.WriteAllText(cleanupBat, batContent);
                Process.Start(new ProcessStartInfo(cleanupBat) { CreateNoWindow = true, UseShellExecute = false });

                MessageBox.Show("NovaBrowser telah berhasil dihapus dari komputer Anda.", "Uninstall Selesai", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } catch (Exception ex) {
                MessageBox.Show("Gagal menghapus beberapa berkas:\n" + ex.Message, "Uninstall", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private static bool CheckPrerequisites() {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;
            string dllInLib = Path.Combine(appDir, "lib", "Microsoft.Web.WebView2.WinForms.dll");
            string dllInRoot = Path.Combine(appDir, "Microsoft.Web.WebView2.WinForms.dll");
            string dllInDev = @"c:\xampp\htdocs\browser\lib\Microsoft.Web.WebView2.WinForms.dll";

            if (!File.Exists(dllInLib) && !File.Exists(dllInRoot) && !File.Exists(dllInDev)) {
                MessageBox.Show(
                    "⚠️ Berkas Instalasi NovaBrowser Belum Lengkap!\n\n" +
                    "Aplikasi memerlukan pustaka komponen WebView2 untuk dapat berjalan.\n\n" +
                    "Solusi:\n" +
                    "Silakan pasang kembali menggunakan installer resmi 'NovaBrowserSetup.exe'.",
                    "NovaBrowser - Memerlukan Instalasi",
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

