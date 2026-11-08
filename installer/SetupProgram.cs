using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace NovaBrowser.Installer {
    static class SetupProgram {
        public const string APP_VERSION = "v1.5.4";

        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }

    public class SetupForm : Form {
        private ProgressBar progressBar;
        private Label lblStatus;
        private Button btnInstall;
        private CheckBox chkDesktopShortcut;
        private CheckBox chkLaunchNow;
        private Label lblInstallPath;

        private string installDir;

        public SetupForm() {
            this.Text = "NovaBrowser " + SetupProgram.APP_VERSION + " Setup";
            this.Size = new Size(520, 360);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42); // Dark slate
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);

            try {
                string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(icoPath)) {
                    this.Icon = new Icon(icoPath);
                }
            } catch { }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            installDir = Path.Combine(localAppData, "Programs", "NovaBrowser");

            BuildUI();
        }

        private void BuildUI() {
            // Header Banner
            Panel pnlHeader = new Panel() {
                Dock = DockStyle.Top,
                Height = 85,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(20, 15, 20, 10)
            };

            Label lblTitle = new Label() {
                Text = "NovaBrowser " + SetupProgram.APP_VERSION,
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                ForeColor = Color.FromArgb(56, 189, 248),
                AutoSize = true,
                Location = new Point(20, 14)
            };

            Label lblSubtitle = new Label() {
                Text = "Peramban Super Cepat, Ringan & Terintegrasi WebView2 Chromium",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(22, 48)
            };

            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(lblSubtitle);

            // Content Panel
            Panel pnlContent = new Panel() {
                Dock = DockStyle.Fill,
                Padding = new Padding(25, 20, 25, 10)
            };

            Label lblPathTitle = new Label() {
                Text = "Lokasi Pemasangan:",
                ForeColor = Color.FromArgb(203, 213, 225),
                AutoSize = true,
                Location = new Point(25, 20)
            };

            lblInstallPath = new Label() {
                Text = installDir,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.FromArgb(241, 245, 249),
                AutoSize = true,
                Location = new Point(25, 42)
            };

            chkDesktopShortcut = new CheckBox() {
                Text = "Buat pintasan di Desktop (Desktop Shortcut)",
                Checked = true,
                AutoSize = true,
                Location = new Point(25, 75),
                ForeColor = Color.FromArgb(226, 232, 240)
            };

            chkLaunchNow = new CheckBox() {
                Text = "Jalankan NovaBrowser setelah pemasangan selesai",
                Checked = true,
                AutoSize = true,
                Location = new Point(25, 105),
                ForeColor = Color.FromArgb(226, 232, 240)
            };

            lblStatus = new Label() {
                Text = "Siap memasang NovaBrowser di komputer Anda.",
                ForeColor = Color.FromArgb(148, 163, 184),
                AutoSize = true,
                Location = new Point(25, 145)
            };

            progressBar = new ProgressBar() {
                Location = new Point(25, 170),
                Size = new Size(455, 20),
                Visible = false,
                Style = ProgressBarStyle.Marquee
            };

            btnInstall = new Button() {
                Text = "Pasang Sekarang (Install)",
                Size = new Size(180, 40),
                Location = new Point(300, 210),
                BackColor = Color.FromArgb(2, 132, 199),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += async (s, e) => await StartInstallation();

            pnlContent.Controls.Add(lblPathTitle);
            pnlContent.Controls.Add(lblInstallPath);
            pnlContent.Controls.Add(chkDesktopShortcut);
            pnlContent.Controls.Add(chkLaunchNow);
            pnlContent.Controls.Add(lblStatus);
            pnlContent.Controls.Add(progressBar);
            pnlContent.Controls.Add(btnInstall);

            this.Controls.Add(pnlContent);
            this.Controls.Add(pnlHeader);
        }

        private async System.Threading.Tasks.Task StartInstallation() {
            btnInstall.Enabled = false;
            chkDesktopShortcut.Enabled = false;
            chkLaunchNow.Enabled = false;
            progressBar.Visible = true;
            lblStatus.Text = "Menyiapkan direktori pemasangan...";

            try {
                // Tutup proses NovaBrowser lama jika sedang berjalan agar berkas tidak terkunci
                foreach (var proc in Process.GetProcessesByName("NovaBrowser")) {
                    try { proc.Kill(); proc.WaitForExit(1500); } catch { }
                }

                // 1. Buat direktori instalasi
                if (!Directory.Exists(installDir)) {
                    Directory.CreateDirectory(installDir);
                }

                lblStatus.Text = "Mengekstrak berkas aplikasi...";
                await System.Threading.Tasks.Task.Delay(300);

                // 2. Ekstrak payload dari Embedded Resource
                Assembly asm = Assembly.GetExecutingAssembly();
                string resourceName = "NovaBrowserPayload.zip";
                
                // Cari resource name
                foreach (string name in asm.GetManifestResourceNames()) {
                    if (name.EndsWith("NovaBrowserPayload.zip", StringComparison.OrdinalIgnoreCase)) {
                        resourceName = name;
                        break;
                    }
                }

                using (Stream stream = asm.GetManifestResourceStream(resourceName)) {
                    if (stream != null) {
                        using (ZipArchive archive = new ZipArchive(stream)) {
                            foreach (ZipArchiveEntry entry in archive.Entries) {
                                string destinationPath = Path.Combine(installDir, entry.FullName);
                                string destDir = Path.GetDirectoryName(destinationPath);
                                if (!string.IsNullOrEmpty(destDir) && !Directory.Exists(destDir)) {
                                    Directory.CreateDirectory(destDir);
                                }
                                if (!string.IsNullOrEmpty(entry.Name)) {
                                    entry.ExtractToFile(destinationPath, true);
                                }
                            }
                        }
                    } else {
                        // Fallback jika installer di folder development: copy langsung dari folder lokal
                        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                        CopyFolder(baseDir, installDir);
                    }
                }

                string mainExe = Path.Combine(installDir, "NovaBrowser.exe");
                string appIco = Path.Combine(installDir, "app.ico");

                lblStatus.Text = "Membuat pintasan desktop & start menu...";
                await System.Threading.Tasks.Task.Delay(200);

                // 3. Buat Desktop Shortcut
                if (chkDesktopShortcut.Checked) {
                    string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    CreateShortcut(Path.Combine(desktopPath, "NovaBrowser.lnk"), mainExe, appIco);
                }

                // 4. Buat Start Menu Shortcut
                string startMenuPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                    "Programs",
                    "NovaBrowser"
                );
                if (!Directory.Exists(startMenuPath)) {
                    Directory.CreateDirectory(startMenuPath);
                }
                CreateShortcut(Path.Combine(startMenuPath, "NovaBrowser.lnk"), mainExe, appIco);

                // 5. Daftarkan Uninstaller ke Windows Registry
                lblStatus.Text = "Mendaftarkan program ke sistem Windows...";
                RegisterUninstallEntry(installDir, mainExe);

                lblStatus.Text = "Pemasangan selesai dengan sukses!";
                progressBar.Visible = false;
                btnInstall.Text = "Selesai (Finish)";
                btnInstall.BackColor = Color.FromArgb(5, 150, 105);
                btnInstall.Enabled = true;

                btnInstall.Click += (s, e) => {
                    LaunchAppAndExit(mainExe, installDir);
                };

                if (chkLaunchNow.Checked && File.Exists(mainExe)) {
                    LaunchAppAndExit(mainExe, installDir);
                }
            } catch (Exception ex) {
                progressBar.Visible = false;
                btnInstall.Enabled = true;
                lblStatus.Text = "Pemasangan gagal: " + ex.Message;
                MessageBox.Show(this, "Terjadi kesalahan saat memasang NovaBrowser:\n" + ex.Message, "Error Pemasangan", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LaunchAppAndExit(string mainExe, string workingDir) {
            try {
                if (File.Exists(mainExe)) {
                    var psi = new ProcessStartInfo(mainExe) {
                        WorkingDirectory = workingDir,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
            } catch { }
            Application.Exit();
        }

        private void CopyFolder(string sourceDir, string targetDir) {
            string[] neededFiles = new string[] { "NovaBrowser.exe", "NovaBrowser.exe.config", "app.ico" };
            foreach (string file in neededFiles) {
                string src = Path.Combine(sourceDir, file);
                if (File.Exists(src)) {
                    File.Copy(src, Path.Combine(targetDir, file), true);
                }
            }

            string libSrc = Path.Combine(sourceDir, "lib");
            if (Directory.Exists(libSrc)) {
                CopyDirectoryRecursive(libSrc, Path.Combine(targetDir, "lib"));
            }
        }

        private void CopyDirectoryRecursive(string source, string target) {
            if (!Directory.Exists(target)) Directory.CreateDirectory(target);
            foreach (string file in Directory.GetFiles(source)) {
                File.Copy(file, Path.Combine(target, Path.GetFileName(file)), true);
            }
            foreach (string dir in Directory.GetDirectories(source)) {
                CopyDirectoryRecursive(dir, Path.Combine(target, Path.GetFileName(dir)));
            }
        }

        private void CreateShortcut(string shortcutPath, string targetPath, string iconPath) {
            try {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) return;
                dynamic shell = Activator.CreateInstance(shellType);
                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = targetPath;
                shortcut.WorkingDirectory = Path.GetDirectoryName(targetPath);
                shortcut.Description = "NovaBrowser " + SetupProgram.APP_VERSION;
                if (File.Exists(iconPath)) {
                    shortcut.IconLocation = iconPath + ",0";
                }
                shortcut.Save();
            } catch { }
        }

        private void RegisterUninstallEntry(string installFolder, string exePath) {
            try {
                string keyPath = @"Software\Microsoft\Windows\CurrentVersion\Uninstall\NovaBrowser";
                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(keyPath)) {
                    if (key != null) {
                        key.SetValue("DisplayName", "NovaBrowser " + SetupProgram.APP_VERSION);
                        key.SetValue("DisplayVersion", SetupProgram.APP_VERSION.TrimStart('v'));
                        key.SetValue("Publisher", "NovaBrowser Project");
                        key.SetValue("DisplayIcon", exePath);
                        key.SetValue("InstallLocation", installFolder);
                        // Jalankan uninstaller command
                        key.SetValue("UninstallString", "\"" + exePath + "\" --uninstall");
                        key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                        key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                    }
                }
            } catch { }
        }
    }
}

