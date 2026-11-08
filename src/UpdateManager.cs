using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NovaBrowser {
    public class UpdateInfo {
        public string Version { get; set; }
        public string DownloadUrl { get; set; }
        public string Changelog { get; set; }
        public bool HasUpdate { get; set; }
    }

    public static class UpdateManager {
        public const string VERSION_CHECK_URL = "https://raw.githubusercontent.com/wandiapriliyandi/novabrowser/main/version.json";

        public static async Task<UpdateInfo> CheckForUpdatesAsync(string currentVersion) {
            try {
                using (WebClient client = new WebClient()) {
                    client.Headers.Add("User-Agent", "NovaLite-Browser-Updater/" + currentVersion);
                    string json = await client.DownloadStringTaskAsync(new Uri(VERSION_CHECK_URL));

                    string remoteVer = ExtractJsonValue(json, "version");
                    string downloadUrl = ExtractJsonValue(json, "downloadUrl");
                    string changelog = ExtractJsonValue(json, "changelog");

                    if (string.IsNullOrEmpty(remoteVer)) return null;

                    Version cur = ParseVersion(currentVersion);
                    Version rem = ParseVersion(remoteVer);

                    bool hasUpdate = (rem > cur);

                    return new UpdateInfo() {
                        Version = remoteVer,
                        DownloadUrl = downloadUrl,
                        Changelog = changelog,
                        HasUpdate = hasUpdate
                    };
                }
            } catch {
                return null;
            }
        }

        private static string ExtractJsonValue(string json, string key) {
            var match = Regex.Match(json, "\"" + key + "\"\\s*:\\s*\"([^\"]+)\"");
            return match.Success ? match.Groups[1].Value : "";
        }

        private static Version ParseVersion(string ver) {
            ver = ver.TrimStart('v', 'V').Trim();
            string[] parts = ver.Split('.');
            int major = parts.Length > 0 ? int.Parse(parts[0]) : 1;
            int minor = parts.Length > 1 ? int.Parse(parts[1]) : 0;
            int build = parts.Length > 2 ? int.Parse(parts[2]) : 0;
            return new Version(major, minor, build);
        }

        public static void ApplyUpdate(Form parent, string downloadUrl, string newVersion) {
            Form dlg = new Form() {
                Text = "⚡ Memperbarui NovaLite Browser ke v" + newVersion,
                Size = new Size(460, 200),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White
            };

            Label lblStatus = new Label() {
                Text = "Mengunduh file pembaruan NovaBrowser.exe...",
                Dock = DockStyle.Top,
                Height = 45,
                Padding = new Padding(15, 15, 15, 0),
                Font = new Font("Segoe UI", 10)
            };

            ProgressBar pb = new ProgressBar() {
                Dock = DockStyle.Top,
                Height = 24,
                Style = ProgressBarStyle.Marquee,
                Margin = new Padding(20)
            };

            dlg.Controls.Add(pb);
            dlg.Controls.Add(lblStatus);

            dlg.Shown += async (s, e) => {
                string currentExe = Application.ExecutablePath;
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string tempExe = Path.Combine(appDir, "NovaBrowser.update");
                string updaterBat = Path.Combine(appDir, "apply_update.bat");

                try {
                    using (WebClient client = new WebClient()) {
                        client.Headers.Add("User-Agent", "NovaLite-Browser-Updater");
                        await client.DownloadFileTaskAsync(new Uri(downloadUrl), tempExe);
                    }

                    lblStatus.Text = "Pembaruan berhasil diunduh. Memulai restart...";
                    await Task.Delay(800);

                    // Buat skrip batch pengganti yang aman
                    string batchContent = string.Format(
@"@echo off
timeout /t 1 /nobreak >nul
del ""{0}""
move ""{1}"" ""{0}""
start """" ""{0}""
del ""%~f0""
", currentExe, tempExe);

                    File.WriteAllText(updaterBat, batchContent);

                    // Jalankan batch updater lalu tutup browser
                    ProcessStartInfo psi = new ProcessStartInfo() {
                        FileName = updaterBat,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };
                    Process.Start(psi);
                    Environment.Exit(0);
                } catch (Exception ex) {
                    MessageBox.Show(dlg, "Gagal mengunduh pembaruan:\n" + ex.Message, "Error Pembaruan", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    dlg.Close();
                }
            };

            dlg.ShowDialog(parent);
        }
    }
}
