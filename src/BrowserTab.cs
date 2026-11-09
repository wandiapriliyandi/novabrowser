using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;

namespace NovaBrowser {
    public class BrowserTab : UserControl {
        private static System.Collections.Generic.Dictionary<string, CoreWebView2Environment> envCache = new System.Collections.Generic.Dictionary<string, CoreWebView2Environment>();

        private WebView2 webView;
        private MainForm mainForm;
        private ProfileManager profileManager;
        private bool isInitialized = false;
        private string pendingUrl = "";

        public string CurrentUrl {
            get {
                if (webView != null && webView.CoreWebView2 != null) {
                    return webView.CoreWebView2.Source ?? "";
                }
                return pendingUrl;
            }
        }

        public string PageTitle { get; private set; }
        public bool IsLoading { get; private set; }

        public event Action<BrowserTab, string> TitleChanged;
        public event Action<BrowserTab, string> UrlChanged;
        public event Action<BrowserTab, bool> LoadingStateChanged;

        public BrowserTab(MainForm form, ProfileManager profMgr, string initialUrl = "") {
            this.mainForm = form;
            this.profileManager = profMgr;
            this.PageTitle = "Tab Baru";
            this.pendingUrl = string.IsNullOrEmpty(initialUrl) ? "about:blank" : initialUrl;
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.FromArgb(15, 23, 42);

            InitializeWebView();
        }

        private async void InitializeWebView() {
            try {
                webView = new WebView2() {
                    Dock = DockStyle.Fill
                };
                this.Controls.Add(webView);

                string userDataFolder = profileManager.GetUserDataFolder();
                string args = profileManager.BuildCommandLineArguments();
                string envKey = profileManager.CurrentProfile + "_" + args;

                CoreWebView2Environment environment;
                lock (envCache) {
                    if (!envCache.TryGetValue(envKey, out environment)) {
                        environment = null;
                    }
                }

                if (environment == null) {
                    bool failed = false;
                    try {
                        var envOptions = new CoreWebView2EnvironmentOptions(args);
                        environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, envOptions);
                    } catch {
                        failed = true;
                    }

                    if (failed || environment == null) {
                        environment = await CoreWebView2Environment.CreateAsync(null, userDataFolder, null);
                    }

                    lock (envCache) {
                        envCache[envKey] = environment;
                    }
                }

                await webView.EnsureCoreWebView2Async(environment);

                ConfigureSettings();
                HookEvents();

                isInitialized = true;

                if (!string.IsNullOrEmpty(pendingUrl) && pendingUrl != "about:blank") {
                    webView.CoreWebView2.Navigate(pendingUrl);
                } else {
                    NavigateToHomepage();
                }
            } catch (Exception ex) {
                ShowInitError(ex.Message);
            }
        }

        private void ShowInitError(string error) {
            Label lbl = new Label() {
                Text = "Gagal memuat engine WebView2:\n" + error + "\n\nPastikan Microsoft Edge WebView2 Runtime terpasang di sistem.",
                ForeColor = Color.FromArgb(248, 113, 113),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11)
            };
            this.Controls.Add(lbl);
        }

        private void ConfigureSettings() {
            if (webView == null || webView.CoreWebView2 == null) return;

            var s = webView.CoreWebView2.Settings;
            // Developer Mode Penuh
            s.AreDevToolsEnabled = true;
            s.AreDefaultContextMenusEnabled = true;
            s.IsScriptEnabled = true;
            s.IsWebMessageEnabled = true;

            // Simpan Sandi & Autofill
            s.IsPasswordAutosaveEnabled = true;
            s.IsGeneralAutofillEnabled = true;

            s.IsStatusBarEnabled = false;
            s.AreDefaultScriptDialogsEnabled = true;
        }

        private void HookEvents() {
            if (webView == null || webView.CoreWebView2 == null) return;

            webView.CoreWebView2.NavigationStarting += (s, e) => {
                IsLoading = true;
                if (LoadingStateChanged != null) LoadingStateChanged(this, true);
            };

            webView.CoreWebView2.NavigationCompleted += (s, e) => {
                IsLoading = false;
                if (LoadingStateChanged != null) LoadingStateChanged(this, false);
            };

            webView.CoreWebView2.SourceChanged += (s, e) => {
                string url = webView.CoreWebView2.Source;
                if (UrlChanged != null) UrlChanged(this, url);
            };

            webView.CoreWebView2.DocumentTitleChanged += (s, e) => {
                string title = webView.CoreWebView2.DocumentTitle;
                if (string.IsNullOrWhiteSpace(title)) title = "Tab Baru";
                this.PageTitle = title;
                if (TitleChanged != null) TitleChanged(this, title);
            };

            webView.CoreWebView2.NewWindowRequested += (s, e) => {
                e.Handled = true;
                mainForm.AddNewTab(e.Uri, true);
            };

            webView.CoreWebView2.ProcessFailed += (s, e) => {
                try {
                    // Pulihkan WebView hanya jika renderer crash ringan tanpa mematikan browser jika proses utama sudah exit
                    if (e.ProcessFailedKind != CoreWebView2ProcessFailedKind.BrowserProcessExited) {
                        webView.Reload();
                    }
                } catch { }
            };
        }

        public void Navigate(string url) {
            if (string.IsNullOrWhiteSpace(url)) return;

            if (url.Equals("novalite://newtab", StringComparison.OrdinalIgnoreCase) ||
                url.Equals("about:blank", StringComparison.OrdinalIgnoreCase) ||
                url.Equals("home", StringComparison.OrdinalIgnoreCase)) {
                NavigateToHomepage();
                return;
            }

            url = OmniboxHelper.ResolveUrl(url);

            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.Navigate(url);
            } else {
                pendingUrl = url;
            }
        }

        public void NavigateToHomepage() {
            string html = Homepage.GetHtml();
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.NavigateToString(html);
            }
        }

        public void GoBack() {
            if (isInitialized && webView != null && webView.CanGoBack) {
                webView.GoBack();
            }
        }

        public void GoForward() {
            if (isInitialized && webView != null && webView.CanGoForward) {
                webView.GoForward();
            }
        }

        public void Reload() {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.Reload();
            }
        }

        public void HardReload() {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                // Hard reload bypass cache via JS location.reload(true) or navigation
                webView.CoreWebView2.ExecuteScriptAsync("location.reload(true);");
            }
        }

        public void OpenDevTools() {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.OpenDevToolsWindow();
            }
        }

        public async Task<string> ExecuteScriptAsync(string script) {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                return await webView.CoreWebView2.ExecuteScriptAsync(script);
            }
            return "Engine belum siap";
        }

        public void ClearCookies() {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.CookieManager.DeleteAllCookies();
                MessageBox.Show(this, "Semua cookies untuk profil ini berhasil dibersihkan!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public async void ClearStorage() {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                await webView.CoreWebView2.ExecuteScriptAsync("localStorage.clear(); sessionStorage.clear();");
                MessageBox.Show(this, "LocalStorage & SessionStorage berhasil dibersihkan!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void SetUserAgent(string userAgent) {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                webView.CoreWebView2.Settings.UserAgent = string.IsNullOrEmpty(userAgent) ? null : userAgent;
                webView.CoreWebView2.Reload();
            }
        }

        public async Task<string> GetPageHtmlAsync() {
            if (isInitialized && webView != null && webView.CoreWebView2 != null) {
                string html = await webView.CoreWebView2.ExecuteScriptAsync("document.documentElement.outerHTML");
                return System.Text.RegularExpressions.Regex.Unescape(html ?? "");
            }
            return "";
        }
    }
}
