using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NovaBrowser {
    public class MainForm : Form {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr hWnd, int msg, int wParam, [MarshalAs(UnmanagedType.LPWStr)] string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        [DllImport("kernel32.dll", EntryPoint = "SetProcessWorkingSetSize")]
        private static extern bool SetProcessWorkingSetSize(IntPtr proc, int min, int max);

        public static void FlushMemory() {
            try {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
                GC.WaitForPendingFinalizers();
                if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
                    SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
                }
            } catch { }
        }

        public static void SetCueBanner(TextBox txt, string placeholder) {
            try {
                if (txt != null && txt.IsHandleCreated) {
                    SendMessage(txt.Handle, EM_SETCUEBANNER, 1, placeholder);
                } else if (txt != null) {
                    txt.HandleCreated += (s, e) => SendMessage(txt.Handle, EM_SETCUEBANNER, 1, placeholder);
                }
            } catch { }
        }
        // State
        public ProfileManager CurrentProfileManager { get; private set; }
        public BookmarkHistoryManager BookmarkHistory { get; private set; }
        private List<BrowserTab> tabs = new List<BrowserTab>();
        private int activeTabIndex = -1;
        private Size normalSize;

        // UI Top Controls
        private Panel pnlHeader;
        private FlowLayoutPanel pnlTabs;
        private Button btnNewTab;
        private ComboBox cmbProfile;

        // Nav Bar
        private Panel pnlNav;
        private Button btnBack;
        private Button btnForward;
        private Button btnReload;
        private Button btnHome;
        private Label lblSecurity;
        private TextBox txtOmnibox;
        private Button btnClearUrl;
        private Button btnGo;
        private Button btnBookmarks;
        private Button btnHistory;
        private Button btnInstallApp;
        private Button btnToggleDev;
        private Button btnTogglePentest;
        private Button btnOpenDevTools;
        public bool IsAppMode { get; private set; }

        // Toolbars
        private DevToolbar devToolbar;
        private PentestToolbar pentestToolbar;

        // Content & Status
        private Panel pnlBrowserHost;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel statusLabel;
        private ToolStripStatusLabel statusRam;
        private ToolStripStatusLabel statusUpdate;

        public BrowserTab ActiveTab {
            get {
                if (activeTabIndex >= 0 && activeTabIndex < tabs.Count) {
                    return tabs[activeTabIndex];
                }
                return null;
            }
        }

        public string ActiveTabCurrentUrl {
            get {
                return ActiveTab != null ? ActiveTab.CurrentUrl : "";
            }
        }

        public const string APP_VERSION = "v1.5.1";

        public MainForm(string initialUrl = null, bool appMode = false) {
            this.IsAppMode = appMode;
            this.CurrentProfileManager = new ProfileManager(appMode ? "WebApps" : "Default");
            this.BookmarkHistory = new BookmarkHistoryManager(CurrentProfileManager.GetUserDataFolder());

            this.Text = "NovaBrowser " + APP_VERSION + " - Ultra Lightweight & Developer Edition";
            this.Size = new Size(1220, 780);
            this.MinimumSize = new Size(400, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9f);
            this.KeyPreview = true;
            this.normalSize = this.Size;

            Homepage.SaveHomepageFile();

            try {
                string icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.ico");
                if (File.Exists(icoPath)) {
                    this.Icon = new Icon(icoPath);
                } else {
                    this.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                }
            } catch { }

            BuildUI();
            HookShortcuts();

            if (this.IsAppMode) {
                pnlTabs.Visible = false;
                cmbProfile.Visible = false;
            }

            // Buka tab pertama
            string startUrl = string.IsNullOrWhiteSpace(initialUrl) ? "novalite://newtab" : initialUrl;
            AddNewTab(startUrl, true);
            UpdateMemoryStatus(false);

            // Pemeriksaan update otomatis di latar belakang setelah startup
            Task.Delay(3500).ContinueWith(_ => CheckForUpdatesSilent());
        }

        private void BuildUI() {
            // Container Header
            pnlHeader = new Panel() {
                Dock = DockStyle.Top,
                AutoSize = true,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            // 1. Tab Bar
            pnlTabs = new FlowLayoutPanel() {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(15, 23, 42),
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(6, 4, 6, 0)
            };

            btnNewTab = new Button() {
                Text = "+",
                Width = 32,
                Height = 28,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(2, 0, 8, 0)
            };
            btnNewTab.FlatAppearance.BorderSize = 0;
            btnNewTab.Click += (s, e) => AddNewTab("novalite://newtab", true);

            cmbProfile = new ComboBox() {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Width = 140,
                Font = new Font("Segoe UI", 8.5f),
                Margin = new Padding(4, 3, 4, 0)
            };
            cmbProfile.Items.AddRange(new object[] { "Profil: Default", "Profil: Work", "Profil: Pentest", "Profil: Incognito" });
            cmbProfile.SelectedIndex = 0;
            cmbProfile.SelectedIndexChanged += (s, e) => SwitchProfile();

            // 2. Navigation Bar
            pnlNav = new Panel() {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(8, 6, 8, 6)
            };

            btnBack = CreateNavButton("◀", (s, e) => { if (ActiveTab != null) ActiveTab.GoBack(); });
            btnForward = CreateNavButton("▶", (s, e) => { if (ActiveTab != null) ActiveTab.GoForward(); });
            btnReload = CreateNavButton("↻", (s, e) => { if (ActiveTab != null) ActiveTab.Reload(); });
            btnHome = CreateNavButton("⌂", (s, e) => { if (ActiveTab != null) ActiveTab.NavigateToHomepage(); });

            lblSecurity = new Label() {
                Text = "⚡ Ready",
                AutoSize = true,
                ForeColor = Color.FromArgb(56, 189, 248),
                Padding = new Padding(6, 6, 4, 0),
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold)
            };

            txtOmnibox = new TextBox() {
                Font = new Font("Segoe UI", 10.5f),
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoCompleteMode = AutoCompleteMode.SuggestAppend,
                AutoCompleteSource = AutoCompleteSource.CustomSource
            };
            SetCueBanner(txtOmnibox, "Ketik URL (misal: localhost:3000) atau cari di sini...");
            txtOmnibox.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.Enter) {
                    NavigateToOmnibox();
                    e.SuppressKeyPress = true;
                }
            };
            txtOmnibox.GotFocus += (s, e) => {
                txtOmnibox.SelectAll();
                UpdateClearButtonPosition();
            };
            txtOmnibox.TextChanged += (s, e) => UpdateClearButtonPosition();

            btnClearUrl = new Button() {
                Text = "✕",
                Size = new Size(20, 20),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(30, 41, 59),
                ForeColor = Color.FromArgb(148, 163, 184),
                Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                Visible = false
            };
            btnClearUrl.FlatAppearance.BorderSize = 0;
            btnClearUrl.FlatAppearance.MouseOverBackColor = Color.FromArgb(239, 68, 68);
            btnClearUrl.FlatAppearance.MouseDownBackColor = Color.FromArgb(185, 28, 28);
            btnClearUrl.Click += (s, e) => {
                txtOmnibox.Clear();
                txtOmnibox.Focus();
                UpdateClearButtonPosition();
            };

            pnlNav.Controls.Add(lblSecurity);
            pnlNav.Controls.Add(txtOmnibox);
            pnlNav.Controls.Add(btnClearUrl);
            btnClearUrl.BringToFront();

            btnGo = CreateNavButton("➔", (s, e) => NavigateToOmnibox());

            btnBookmarks = CreateNavButton("⭐", (s, e) => ShowBookmarkMenu());
            btnBookmarks.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnHistory = CreateNavButton("🕒", (s, e) => OpenHistoryDialog(0));
            btnHistory.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);

            btnInstallApp = CreateNavButton("📦 Install", (s, e) => ShowInstallAppDialog());
            btnInstallApp.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnInstallApp.BackColor = Color.FromArgb(5, 150, 105);

            btnToggleDev = CreateNavButton("🛠️ Dev", (s, e) => ToggleDevToolbar());
            btnToggleDev.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            btnTogglePentest = CreateNavButton("🛡️ Pentest", (s, e) => TogglePentestToolbar());
            btnTogglePentest.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnTogglePentest.ForeColor = Color.FromArgb(251, 113, 133);

            btnOpenDevTools = CreateNavButton("F12 DevTools", (s, e) => { if (ActiveTab != null) ActiveTab.OpenDevTools(); });
            btnOpenDevTools.BackColor = Color.FromArgb(14, 116, 144);
            btnOpenDevTools.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            // Susun Nav Bar secara absolut/docked
            pnlNav.Resize += (s, e) => LayoutNavBar();

            // 3. Toolbars
            devToolbar = new DevToolbar(this) { Visible = false };
            pentestToolbar = new PentestToolbar(this) { Visible = false };

            pnlHeader.Controls.Add(pentestToolbar);
            pnlHeader.Controls.Add(devToolbar);
            pnlHeader.Controls.Add(pnlNav);
            pnlHeader.Controls.Add(pnlTabs);

            // 4. Browser Host Container
            pnlBrowserHost = new Panel() {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(15, 23, 42)
            };

            // 5. Status Strip
            statusStrip = new StatusStrip() {
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.FromArgb(148, 163, 184),
                SizingGrip = false
            };
            statusLabel = new ToolStripStatusLabel("Siap") { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
            statusRam = new ToolStripStatusLabel("RAM: Mengkalkulasi...") {
                TextAlign = ContentAlignment.MiddleRight,
                IsLink = true,
                LinkColor = Color.FromArgb(56, 189, 248),
                ActiveLinkColor = Color.FromArgb(129, 230, 217)
            };
            statusRam.Click += (s, e) => {
                UpdateMemoryStatus(true);
                MessageBox.Show(this, "Pembersihan memori berhasil dilakukan!\nMemori yang tidak aktif telah dilepaskan kembali ke Windows.", "Optimasi RAM", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };

            statusUpdate = new ToolStripStatusLabel("🔄 Cek Update") {
                TextAlign = ContentAlignment.MiddleRight,
                IsLink = true,
                LinkColor = Color.FromArgb(16, 185, 129),
                ActiveLinkColor = Color.FromArgb(52, 211, 153),
                Margin = new Padding(8, 0, 8, 0)
            };
            statusUpdate.Click += (s, e) => CheckForUpdatesManual();

            statusStrip.Items.Add(statusLabel);
            statusStrip.Items.Add(statusRam);
            statusStrip.Items.Add(statusUpdate);

            this.Controls.Add(pnlBrowserHost);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(statusStrip);

            LayoutNavBar();
            UpdateOmniboxAutoComplete();
        }

        private void LayoutNavBar() {
            int left = 8;
            btnBack.Location = new Point(left, 7); left += btnBack.Width + 4;
            btnForward.Location = new Point(left, 7); left += btnForward.Width + 4;
            btnReload.Location = new Point(left, 7); left += btnReload.Width + 4;
            btnHome.Location = new Point(left, 7); left += btnHome.Width + 8;

            lblSecurity.Location = new Point(left, 7); left += lblSecurity.Width + 6;

            int right = pnlNav.Width - 8;
            btnOpenDevTools.Location = new Point(right - btnOpenDevTools.Width, 7); right -= btnOpenDevTools.Width + 5;
            btnTogglePentest.Location = new Point(right - btnTogglePentest.Width, 7); right -= btnTogglePentest.Width + 5;
            btnToggleDev.Location = new Point(right - btnToggleDev.Width, 7); right -= btnToggleDev.Width + 5;
            btnInstallApp.Location = new Point(right - btnInstallApp.Width, 7); right -= btnInstallApp.Width + 5;
            btnHistory.Location = new Point(right - btnHistory.Width, 7); right -= btnHistory.Width + 5;
            btnBookmarks.Location = new Point(right - btnBookmarks.Width, 7); right -= btnBookmarks.Width + 5;
            btnGo.Location = new Point(right - btnGo.Width, 7); right -= btnGo.Width + 5;

            txtOmnibox.Location = new Point(left, 7);
            txtOmnibox.Size = new Size(Math.Max(100, right - left), 28);
            UpdateClearButtonPosition();
        }

        private void UpdateClearButtonPosition() {
            if (btnClearUrl == null || txtOmnibox == null) return;
            btnClearUrl.Location = new Point(txtOmnibox.Right - btnClearUrl.Width - 4, txtOmnibox.Top + (txtOmnibox.Height - btnClearUrl.Height) / 2);
            btnClearUrl.BringToFront();
            btnClearUrl.Visible = !string.IsNullOrEmpty(txtOmnibox.Text);
        }

        private Button CreateNavButton(string text, EventHandler onClick) {
            Button btn = new Button() {
                Text = text,
                AutoSize = true,
                Height = 28,
                BackColor = Color.FromArgb(51, 65, 85),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            pnlNav.Controls.Add(btn);
            return btn;
        }

        public void AddNewTab(string url = "novalite://newtab", bool switchToNew = true) {
            var tab = new BrowserTab(this, CurrentProfileManager, url);
            tabs.Add(tab);

            tab.TitleChanged += (sender, title) => {
                this.BeginInvoke(new Action(() => {
                    RebuildTabBar();
                    if (sender == ActiveTab) {
                        this.Text = title + " - NovaLite Browser " + APP_VERSION;
                    }
                }));
            };

            tab.UrlChanged += (sender, newUrl) => {
                this.BeginInvoke(new Action(() => {
                    if (sender == ActiveTab) {
                        txtOmnibox.Text = newUrl.Equals("novalite://newtab", StringComparison.OrdinalIgnoreCase) ? "" : newUrl;
                        lblSecurity.Text = OmniboxHelper.GetSecurityBadge(newUrl);
                    }
                }));
            };

            tab.LoadingStateChanged += (sender, loading) => {
                this.BeginInvoke(new Action(() => {
                    if (sender == ActiveTab) {
                        statusLabel.Text = loading ? "Memuat halaman..." : "Selesai";
                        btnReload.Text = loading ? "✕" : "↻";
                        UpdateMemoryStatus();
                    }
                    if (!loading && sender != null) {
                        string curUrl = sender.CurrentUrl;
                        string curTitle = sender.PageTitle;
                        BookmarkHistory.AddHistory(curTitle, curUrl);
                        UpdateOmniboxAutoComplete();
                    }
                }));
            };

            if (switchToNew) {
                SwitchToTab(tabs.Count - 1);
            } else {
                RebuildTabBar();
            }
        }

        public void CloseTab(int index) {
            if (index < 0 || index >= tabs.Count) return;

            var tabToClose = tabs[index];
            pnlBrowserHost.Controls.Remove(tabToClose);
            tabs.RemoveAt(index);
            tabToClose.Dispose();

            if (tabs.Count == 0) {
                this.Close();
                return;
            }

            if (activeTabIndex >= tabs.Count) {
                activeTabIndex = tabs.Count - 1;
            }
            SwitchToTab(activeTabIndex);
        }

        public void SwitchToTab(int index) {
            if (index < 0 || index >= tabs.Count) return;

            activeTabIndex = index;
            pnlBrowserHost.Controls.Clear();
            var tab = tabs[activeTabIndex];
            pnlBrowserHost.Controls.Add(tab);

            txtOmnibox.Text = tab.CurrentUrl.Equals("novalite://newtab", StringComparison.OrdinalIgnoreCase) ? "" : tab.CurrentUrl;
            lblSecurity.Text = OmniboxHelper.GetSecurityBadge(tab.CurrentUrl);
            this.Text = (string.IsNullOrEmpty(tab.PageTitle) ? "NovaLite Browser " + APP_VERSION : tab.PageTitle + " - NovaLite Browser " + APP_VERSION);

            RebuildTabBar();
        }

        private void RebuildTabBar() {
            pnlTabs.SuspendLayout();
            pnlTabs.Controls.Clear();

            for (int i = 0; i < tabs.Count; i++) {
                int tabIdx = i;
                var tab = tabs[i];
                bool isActive = (i == activeTabIndex);

                Panel tabBtn = new Panel() {
                    Height = 28,
                    Width = 160,
                    BackColor = isActive ? Color.FromArgb(30, 41, 59) : Color.FromArgb(15, 23, 42),
                    Margin = new Padding(0, 0, 3, 0),
                    Padding = new Padding(8, 5, 4, 0)
                };

                string title = string.IsNullOrEmpty(tab.PageTitle) ? "Tab Baru" : tab.PageTitle;
                if (title.Length > 16) title = title.Substring(0, 14) + "..";

                Label lblTab = new Label() {
                    Text = title,
                    AutoSize = false,
                    Width = 120,
                    Height = 20,
                    ForeColor = isActive ? Color.White : Color.FromArgb(148, 163, 184),
                    Font = new Font("Segoe UI", 8.5f, isActive ? FontStyle.Bold : FontStyle.Regular),
                    Cursor = Cursors.Hand
                };
                lblTab.Click += (s, e) => SwitchToTab(tabIdx);
                tabBtn.Click += (s, e) => SwitchToTab(tabIdx);

                Button btnClose = new Button() {
                    Text = "✕",
                    Width = 18,
                    Height = 18,
                    Location = new Point(136, 5),
                    BackColor = Color.Transparent,
                    ForeColor = Color.FromArgb(148, 163, 184),
                    FlatStyle = FlatStyle.Flat,
                    Font = new Font("Segoe UI", 7f)
                };
                btnClose.FlatAppearance.BorderSize = 0;
                btnClose.Click += (s, e) => CloseTab(tabIdx);

                tabBtn.Controls.Add(lblTab);
                tabBtn.Controls.Add(btnClose);
                pnlTabs.Controls.Add(tabBtn);
            }

            pnlTabs.Controls.Add(btnNewTab);
            pnlTabs.Controls.Add(cmbProfile);
            pnlTabs.ResumeLayout();
        }

        private void NavigateToOmnibox() {
            if (ActiveTab != null) {
                ActiveTab.Navigate(txtOmnibox.Text);
            }
        }

        private void ToggleDevToolbar() {
            devToolbar.Visible = !devToolbar.Visible;
            btnToggleDev.BackColor = devToolbar.Visible ? Color.FromArgb(14, 116, 144) : Color.FromArgb(51, 65, 85);
        }

        private void TogglePentestToolbar() {
            pentestToolbar.Visible = !pentestToolbar.Visible;
            btnTogglePentest.BackColor = pentestToolbar.Visible ? Color.FromArgb(159, 18, 57) : Color.FromArgb(51, 65, 85);
        }

        public void ActiveTabHardReload() {
            if (ActiveTab != null) ActiveTab.HardReload();
        }

        public void ActiveTabClearCookies() {
            if (ActiveTab != null) ActiveTab.ClearCookies();
        }

        public void ActiveTabClearStorage() {
            if (ActiveTab != null) ActiveTab.ClearStorage();
        }

        public async void ActiveTabViewSource() {
            if (ActiveTab != null) {
                string html = await ActiveTab.GetPageHtmlAsync();
                Form frmSource = new Form() {
                    Text = "📄 Source Code: " + ActiveTabCurrentUrl,
                    Size = new Size(800, 600),
                    StartPosition = FormStartPosition.CenterParent,
                    BackColor = Color.FromArgb(15, 23, 42)
                };
                TextBox txtSrc = new TextBox() {
                    Multiline = true,
                    ScrollBars = ScrollBars.Both,
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(15, 23, 42),
                    ForeColor = Color.FromArgb(226, 232, 240),
                    Font = new Font("Consolas", 9.5f),
                    Text = html,
                    ReadOnly = true
                };
                frmSource.Controls.Add(txtSrc);
                frmSource.Show(this);
            }
        }

        public async Task<string> ActiveTabExecuteScriptAsync(string script) {
            if (ActiveTab != null) {
                return await ActiveTab.ExecuteScriptAsync(script);
            }
            return "";
        }

        public void ActiveTabSetUserAgent(string ua) {
            if (ActiveTab != null) {
                ActiveTab.SetUserAgent(ua);
            }
        }

        public void SetBrowserViewport(int width, int height) {
            this.WindowState = FormWindowState.Normal;
            this.Size = new Size(width + 16, height + pnlHeader.Height + statusStrip.Height + 40);
        }

        public void ResetBrowserViewport() {
            this.WindowState = FormWindowState.Normal;
            this.Size = normalSize;
        }

        public void ToggleViewportOrientation() {
            int w = this.Width;
            int h = this.Height;
            this.Size = new Size(h, w);
        }

        public void SetProxy(string proxyServer) {
            CurrentProfileManager.ProxyServer = proxyServer;
            MessageBox.Show(this, "Setelan proxy diperbarui: " + (string.IsNullOrEmpty(proxyServer) ? "Direct (OFF)" : proxyServer) + "\n\nTab baru akan menggunakan koneksi proxy ini.", "Info Proxy", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void ToggleDisableWebSecurity() {
            CurrentProfileManager.DisableWebSecurity = !CurrentProfileManager.DisableWebSecurity;
        }

        private bool isKioskFullScreen = false;
        private FormBorderStyle previousBorderStyle = FormBorderStyle.Sizable;

        public void ToggleFullScreen() {
            if (!isKioskFullScreen) {
                previousBorderStyle = this.FormBorderStyle;
                this.FormBorderStyle = FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
                isKioskFullScreen = true;
            } else {
                this.FormBorderStyle = previousBorderStyle;
                this.WindowState = FormWindowState.Maximized;
                isKioskFullScreen = false;
            }
        }

        public void ShowInstallAppDialog() {
            if (ActiveTab != null) {
                string url = ActiveTab.CurrentUrl;
                string title = ActiveTab.PageTitle;
                if (string.IsNullOrEmpty(url) || url.StartsWith("about:") || url.StartsWith("novalite://")) {
                    MessageBox.Show(this, "Buka website terlebih dahulu sebelum memasangnya sebagai aplikasi.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var dlg = new InstallAppDialog(title, url);
                dlg.ShowDialog(this);
            }
        }

        public async void CheckForUpdatesManual() {
            statusUpdate.Text = "Memeriksa...";
            var info = await UpdateManager.CheckForUpdatesAsync(APP_VERSION);
            if (info != null && info.HasUpdate) {
                statusUpdate.Text = "🎉 Update v" + info.Version;
                PromptUserUpdate(info);
            } else if (info != null) {
                statusUpdate.Text = "✅ v" + APP_VERSION + " Terbaru";
                MessageBox.Show(this, "NovaLite Browser sudah dalam versi paling baru (" + APP_VERSION + ").", "Cek Pembaruan", MessageBoxButtons.OK, MessageBoxIcon.Information);
            } else {
                statusUpdate.Text = "🔄 Cek Update";
                MessageBox.Show(this, "Tidak dapat terhubung ke server pembaruan. Periksa koneksi internet Anda.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        public async void CheckForUpdatesSilent() {
            var info = await UpdateManager.CheckForUpdatesAsync(APP_VERSION);
            if (info != null && info.HasUpdate) {
                this.BeginInvoke(new Action(() => {
                    statusUpdate.Text = "🎉 Update v" + info.Version;
                    PromptUserUpdate(info);
                }));
            }
        }

        private void PromptUserUpdate(UpdateInfo info) {
            var dlg = new UpdateDialog(APP_VERSION, info.Version, info.Changelog);
            dlg.ShowDialog(this);
            if (dlg.ShouldUpdate) {
                UpdateManager.ApplyUpdate(this, info.DownloadUrl, info.Version);
            }
        }

        public void BookmarkCurrentTab() {
            if (ActiveTab != null) {
                string url = ActiveTab.CurrentUrl;
                string title = ActiveTab.PageTitle;
                if (!string.IsNullOrEmpty(url) && !url.StartsWith("about:") && !url.StartsWith("novalite://")) {
                    BookmarkHistory.AddBookmark(title, url);
                    UpdateOmniboxAutoComplete();
                    MessageBox.Show(this, "Halaman berhasil ditambahkan ke Bookmark:\n" + title, "Bookmark Disimpan", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        public void UpdateOmniboxAutoComplete() {
            try {
                var source = new AutoCompleteStringCollection();
                var added = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                string[] defaults = new string[] {
                    "http://localhost:80",
                    "http://localhost:3000",
                    "http://localhost:5173",
                    "http://localhost:8000",
                    "http://localhost:8080",
                    "localhost:80",
                    "localhost:3000",
                    "localhost:5173",
                    "localhost:8000",
                    "localhost:8080",
                    "127.0.0.1:8000",
                    "127.0.0.1:3000",
                    "https://google.com",
                    "https://github.com",
                    "https://developer.mozilla.org",
                    "https://stackoverflow.com"
                };
                foreach (string d in defaults) {
                    if (added.Add(d)) source.Add(d);
                }

                if (BookmarkHistory != null) {
                    foreach (var b in BookmarkHistory.Bookmarks) {
                        if (!string.IsNullOrEmpty(b.Url) && added.Add(b.Url)) {
                            source.Add(b.Url);
                        }
                    }
                    foreach (var h in BookmarkHistory.History) {
                        if (!string.IsNullOrEmpty(h.Url) && added.Add(h.Url)) {
                            source.Add(h.Url);
                        }
                    }
                }

                if (txtOmnibox.InvokeRequired) {
                    txtOmnibox.BeginInvoke(new Action(() => txtOmnibox.AutoCompleteCustomSource = source));
                } else {
                    txtOmnibox.AutoCompleteCustomSource = source;
                }
            } catch { }
        }

        public void OpenHistoryDialog(int initialTab = 0) {
            var form = new HistoryForm(this, BookmarkHistory, initialTab);
            form.Show(this);
        }

        private void ShowBookmarkMenu() {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.BackColor = Color.FromArgb(30, 41, 59);
            menu.ForeColor = Color.White;
            menu.Font = new Font("Segoe UI", 9f);

            var itemAdd = new ToolStripMenuItem("⭐ Tambahkan Halaman Ini ke Bookmark (Ctrl+D)");
            itemAdd.Click += (s, e) => BookmarkCurrentTab();
            menu.Items.Add(itemAdd);

            var itemManage = new ToolStripMenuItem("📂 Buka Pengelola Bookmark...");
            itemManage.Click += (s, e) => OpenHistoryDialog(1);
            menu.Items.Add(itemManage);

            menu.Items.Add(new ToolStripSeparator());

            var bookmarks = BookmarkHistory.Bookmarks;
            int count = Math.Min(15, bookmarks.Count);
            if (count == 0) {
                var empty = new ToolStripMenuItem("(Belum ada bookmark tersimpan)") { Enabled = false };
                menu.Items.Add(empty);
            } else {
                for (int i = 0; i < count; i++) {
                    var b = bookmarks[i];
                    string title = b.Title;
                    if (title.Length > 28) title = title.Substring(0, 25) + "...";
                    var itemBm = new ToolStripMenuItem("🔖 " + title);
                    string targetUrl = b.Url;
                    itemBm.ToolTipText = targetUrl;
                    itemBm.Click += (s, e) => {
                        if (ActiveTab != null) ActiveTab.Navigate(targetUrl);
                    };
                    menu.Items.Add(itemBm);
                }
            }

            menu.Show(btnBookmarks, new Point(0, btnBookmarks.Height));
        }

        private void SwitchProfile() {
            string sel = cmbProfile.SelectedItem.ToString();
            string profileName = "Default";
            if (sel.Contains("Work")) profileName = "Work";
            else if (sel.Contains("Pentest")) profileName = "Pentest";
            else if (sel.Contains("Incognito")) profileName = "Incognito";

            if (profileName == CurrentProfileManager.CurrentProfile) return;

            CurrentProfileManager = new ProfileManager(profileName);
            BookmarkHistory = new BookmarkHistoryManager(CurrentProfileManager.GetUserDataFolder());
            UpdateOmniboxAutoComplete();
            MessageBox.Show(this, "Beralih ke " + sel + ".\nTab baru akan menggunakan direktori profil & cookies terpisah di folder profiles/" + profileName, "Ganti Profil", MessageBoxButtons.OK, MessageBoxIcon.Information);
            AddNewTab("novalite://newtab", true);
        }

        private void UpdateMemoryStatus(bool forceTrim = false) {
            if (forceTrim) {
                FlushMemory();
            }
            try {
                using (Process proc = Process.GetCurrentProcess()) {
                    long ramBytes = proc.WorkingSet64;
                    double ramMb = ramBytes / (1024.0 * 1024.0);
                    statusRam.Text = string.Format("Profil: {0} | ⚡ RAM: {1:0.0} MB (Klik utk Pangkas)", CurrentProfileManager.CurrentProfile, ramMb);
                }
            } catch {
                statusRam.Text = "Profil: " + CurrentProfileManager.CurrentProfile;
            }
        }

        private void HookShortcuts() {
            this.KeyDown += (s, e) => {
                if (e.KeyCode == Keys.F12) {
                    if (ActiveTab != null) ActiveTab.OpenDevTools();
                    e.Handled = true;
                } else if (e.KeyCode == Keys.F11) {
                    ToggleFullScreen();
                    e.Handled = true;
                } else if (e.Control && e.KeyCode == Keys.D) {
                    BookmarkCurrentTab();
                    e.Handled = true;
                } else if (e.Control && e.KeyCode == Keys.H) {
                    OpenHistoryDialog(0);
                    e.Handled = true;
                } else if (e.Control && e.KeyCode == Keys.T) {
                    AddNewTab("novalite://newtab", true);
                    e.Handled = true;
                } else if (e.Control && e.KeyCode == Keys.W) {
                    CloseTab(activeTabIndex);
                    e.Handled = true;
                } else if (e.Control && e.Shift && e.KeyCode == Keys.R) {
                    ActiveTabHardReload();
                    e.Handled = true;
                } else if (e.Control && e.KeyCode == Keys.R || e.KeyCode == Keys.F5) {
                    if (ActiveTab != null) ActiveTab.Reload();
                    e.Handled = true;
                } else if (e.Control && e.KeyCode == Keys.L || (e.Alt && e.KeyCode == Keys.D)) {
                    txtOmnibox.Focus();
                    txtOmnibox.SelectAll();
                    e.Handled = true;
                } else if (e.Alt && e.KeyCode == Keys.Left) {
                    if (ActiveTab != null) ActiveTab.GoBack();
                    e.Handled = true;
                } else if (e.Alt && e.KeyCode == Keys.Right) {
                    if (ActiveTab != null) ActiveTab.GoForward();
                    e.Handled = true;
                } else if (e.Alt && e.KeyCode == Keys.Home) {
                    if (ActiveTab != null) ActiveTab.NavigateToHomepage();
                    e.Handled = true;
                }
            };
        }
    }
}

