using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace NovaBrowser {
    public class InstallAppDialog : Form {
        private TextBox txtAppName;
        private TextBox txtAppUrl;
        private Button btnInstall;
        private Button btnCancel;

        public InstallAppDialog(string pageTitle, string pageUrl) {
            this.Text = "📦 Pasang Halaman Sebagai Aplikasi (PWA Desktop)";
            this.Size = new Size(520, 260);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUI(pageTitle, pageUrl);
        }

        private void BuildUI(string pageTitle, string pageUrl) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                Padding = new Padding(20),
                RowCount = 5,
                ColumnCount = 2
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            Label lblDesc = new Label() {
                Text = "Pasang website ini sebagai aplikasi desktop mandiri di Desktop Windows Anda.",
                ForeColor = Color.FromArgb(148, 163, 184),
                Dock = DockStyle.Fill,
                AutoSize = true
            };
            layout.SetColumnSpan(lblDesc, 2);

            Label lblName = new Label() { Text = "Nama Aplikasi:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
            txtAppName = new TextBox() {
                Text = string.IsNullOrWhiteSpace(pageTitle) ? "Aplikasi Web" : pageTitle,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10)
            };

            Label lblUrl = new Label() { Text = "Alamat URL:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
            txtAppUrl = new TextBox() {
                Text = pageUrl,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(148, 163, 184),
                Font = new Font("Segoe UI", 9.5f),
                ReadOnly = true
            };

            FlowLayoutPanel pnlButtons = new FlowLayoutPanel() {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            layout.SetColumnSpan(pnlButtons, 2);

            btnInstall = new Button() {
                Text = "📦 Pasang ke Desktop",
                AutoSize = true,
                Height = 32,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnInstall.Click += (s, e) => ExecuteInstall();

            btnCancel = new Button() {
                Text = "Batal",
                Width = 80,
                Height = 32,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnCancel.Click += (s, e) => this.Close();

            pnlButtons.Controls.Add(btnInstall);
            pnlButtons.Controls.Add(btnCancel);

            layout.Controls.Add(lblDesc, 0, 0);
            layout.Controls.Add(lblName, 0, 1);
            layout.Controls.Add(txtAppName, 1, 1);
            layout.Controls.Add(lblUrl, 0, 2);
            layout.Controls.Add(txtAppUrl, 1, 2);
            layout.Controls.Add(pnlButtons, 0, 3);

            this.Controls.Add(layout);
        }

        private void ExecuteInstall() {
            string title = txtAppName.Text.Trim();
            string url = txtAppUrl.Text.Trim();

            if (string.IsNullOrEmpty(title)) title = "Aplikasi Web";

            // Bersihkan karakter terlarang untuk nama file Windows
            char[] invalid = Path.GetInvalidFileNameChars();
            foreach (char c in invalid) {
                title = title.Replace(c, ' ');
            }
            title = title.Trim();
            if (string.IsNullOrEmpty(title)) title = "Aplikasi Web";

            try {
                Type shellType = Type.GetTypeFromProgID("WScript.Shell");
                if (shellType == null) {
                    MessageBox.Show(this, "WScript.Shell tidak tersedia di sistem ini.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                dynamic shell = Activator.CreateInstance(shellType);
                string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                string shortcutPath = Path.Combine(desktopDir, title + ".lnk");

                dynamic shortcut = shell.CreateShortcut(shortcutPath);
                shortcut.TargetPath = Application.ExecutablePath;
                shortcut.Arguments = string.Format("--app=\"{0}\"", url);
                shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;

                string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(icoPath)) {
                    shortcut.IconLocation = icoPath + ",0";
                }
                shortcut.Description = "Aplikasi Web Mandiri: " + title;
                shortcut.Save();

                MessageBox.Show(this, "Aplikasi web \"" + title + "\" berhasil dipasang di Desktop Windows Anda!\n\nAnda dapat membukanya langsung dari Desktop sebagai aplikasi mandiri.", "Sukses Memasang Aplikasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            } catch (Exception ex) {
                MessageBox.Show(this, "Gagal membuat shortcut aplikasi:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}

