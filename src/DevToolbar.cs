using System;
using System.Drawing;
using System.Windows.Forms;

namespace NovaBrowser {
    public class DevToolbar : Panel {
        private MainForm parentForm;

        // JS Console
        private TextBox txtJsInput;
        private Button btnJsRun;
        private Label lblJsResult;

        // Responsive Mode
        private ComboBox cmbViewport;
        private Button btnRotate;

        // User Agent
        private ComboBox cmbUserAgent;

        // Cache actions
        private Button btnHardReload;
        private Button btnClearCookies;
        private Button btnClearStorage;
        private Button btnViewSource;

        public DevToolbar(MainForm form) {
            this.parentForm = form;
            this.Dock = DockStyle.Top;
            this.Height = 74;
            this.BackColor = Color.FromArgb(24, 32, 47);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);
            this.Padding = new Padding(8, 4, 8, 4);

            BuildUI();
        }

        private void BuildUI() {
            // Baris 1: JS Quick Console & Storage Actions
            FlowLayoutPanel row1 = new FlowLayoutPanel() {
                Dock = DockStyle.Top,
                Height = 32,
                WrapContents = false,
                AutoScroll = false
            };

            Label lblJs = new Label() { Text = "⚡ Live JS:", AutoSize = true, Padding = new Padding(0, 5, 0, 0), ForeColor = Color.FromArgb(56, 189, 248), Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
            txtJsInput = new TextBox() { Width = 300, Font = new Font("Consolas", 9.5f), BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White };
            txtJsInput.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) {
                    ExecuteQuickJs();
                    e.SuppressKeyPress = true;
                }
            };

            btnJsRun = new Button() {
                Text = "Run",
                Width = 55,
                Height = 26,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };
            btnJsRun.Click += (s, e) => ExecuteQuickJs();

            lblJsResult = new Label() {
                Text = "",
                AutoSize = true,
                Padding = new Padding(6, 5, 6, 0),
                ForeColor = Color.FromArgb(129, 230, 217),
                Font = new Font("Consolas", 9f)
            };

            btnHardReload = CreateToolbarButton("🔁 Hard Reload", Color.FromArgb(51, 65, 85));
            btnHardReload.Click += (s, e) => parentForm.ActiveTabHardReload();

            btnClearCookies = CreateToolbarButton("🍪 Hapus Cookies", Color.FromArgb(51, 65, 85));
            btnClearCookies.Click += (s, e) => parentForm.ActiveTabClearCookies();

            btnClearStorage = CreateToolbarButton("🗑️ Hapus Storage", Color.FromArgb(51, 65, 85));
            btnClearStorage.Click += (s, e) => parentForm.ActiveTabClearStorage();

            btnViewSource = CreateToolbarButton("📄 View Source", Color.FromArgb(51, 65, 85));
            btnViewSource.Click += (s, e) => parentForm.ActiveTabViewSource();

            row1.Controls.Add(lblJs);
            row1.Controls.Add(txtJsInput);
            row1.Controls.Add(btnJsRun);
            row1.Controls.Add(lblJsResult);
            row1.Controls.Add(btnHardReload);
            row1.Controls.Add(btnClearCookies);
            row1.Controls.Add(btnClearStorage);
            row1.Controls.Add(btnViewSource);

            // Baris 2: Viewport Simulator & User Agent
            FlowLayoutPanel row2 = new FlowLayoutPanel() {
                Dock = DockStyle.Top,
                Height = 34,
                WrapContents = false,
                AutoScroll = false,
                Padding = new Padding(0, 3, 0, 0)
            };

            Label lblVp = new Label() { Text = "📱 Viewport:", AutoSize = true, Padding = new Padding(0, 5, 0, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            cmbViewport = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
            cmbViewport.Items.AddRange(new object[] {
                "Ukuran Jendela Asli",
                "Mobile: iPhone 14/15 (393 x 852)",
                "Mobile: Android Pixel (412 x 915)",
                "Tablet: iPad Mini/Air (768 x 1024)",
                "Laptop: HD (1366 x 768)",
                "Desktop: Full HD (1920 x 1080)"
            });
            cmbViewport.SelectedIndex = 0;
            cmbViewport.SelectedIndexChanged += (s, e) => ApplyViewport();

            btnRotate = CreateToolbarButton("🔄 Putar", Color.FromArgb(51, 65, 85));
            btnRotate.Click += (s, e) => RotateViewport();

            Label lblUa = new Label() { Text = "🌐 User-Agent:", AutoSize = true, Padding = new Padding(12, 5, 0, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            cmbUserAgent = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 210 };
            cmbUserAgent.Items.AddRange(new object[] {
                "Default (Chromium Desktop)",
                "iPhone Safari (iOS 17)",
                "Android Chrome (Pixel)",
                "Googlebot 2.1 (Crawler)"
            });
            cmbUserAgent.SelectedIndex = 0;
            cmbUserAgent.SelectedIndexChanged += (s, e) => ApplyUserAgent();

            row2.Controls.Add(lblVp);
            row2.Controls.Add(cmbViewport);
            row2.Controls.Add(btnRotate);
            row2.Controls.Add(lblUa);
            row2.Controls.Add(cmbUserAgent);

            this.Controls.Add(row2);
            this.Controls.Add(row1);
        }

        private Button CreateToolbarButton(string text, Color bg) {
            Button btn = new Button() {
                Text = text,
                AutoSize = true,
                Height = 26,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8.5f),
                Margin = new Padding(4, 1, 4, 1)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private async void ExecuteQuickJs() {
            string code = txtJsInput.Text.Trim();
            if (string.IsNullOrEmpty(code)) return;

            lblJsResult.Text = "Menjalankan...";
            try {
                string result = await parentForm.ActiveTabExecuteScriptAsync(code);
                if (result != null && result.Length > 80) {
                    result = result.Substring(0, 77) + "...";
                }
                lblJsResult.Text = "=> " + (result ?? "undefined");
            } catch (Exception ex) {
                lblJsResult.Text = "Error: " + ex.Message;
            }
        }

        private void ApplyViewport() {
            int sel = cmbViewport.SelectedIndex;
            switch (sel) {
                case 1: parentForm.SetBrowserViewport(393, 852); break;
                case 2: parentForm.SetBrowserViewport(412, 915); break;
                case 3: parentForm.SetBrowserViewport(768, 1024); break;
                case 4: parentForm.SetBrowserViewport(1366, 768); break;
                case 5: parentForm.SetBrowserViewport(1920, 1080); break;
                default: parentForm.ResetBrowserViewport(); break;
            }
        }

        private void RotateViewport() {
            parentForm.ToggleViewportOrientation();
        }

        private void ApplyUserAgent() {
            int sel = cmbUserAgent.SelectedIndex;
            string ua = "";
            switch (sel) {
                case 1:
                    ua = "Mozilla/5.0 (iPhone; CPU iPhone OS 17_4 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Mobile/15E148 Safari/604.1";
                    break;
                case 2:
                    ua = "Mozilla/5.0 (Linux; Android 14; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Mobile Safari/537.36";
                    break;
                case 3:
                    ua = "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)";
                    break;
                default:
                    ua = "";
                    break;
            }
            parentForm.ActiveTabSetUserAgent(ua);
        }
    }
}

