package com.novalite.browser;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Message;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.KeyEvent;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.FrameLayout;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.net.URLEncoder;
import java.util.ArrayList;
import java.util.List;
import java.util.UUID;

public class MainActivity extends Activity {

    public static class TabModel {
        public String id;
        public String title;
        public String url;
        public WebView webView;

        public TabModel(String id, String title, String url, WebView webView) {
            this.id = id;
            this.title = title;
            this.url = url;
            this.webView = webView;
        }
    }

    private FrameLayout webViewContainer;
    private EditText txtUrl;
    private ProgressBar progressBar;
    private Button btnBack;
    private Button btnClear;
    private Button btnNewTab;
    private Button btnTabs;
    private Button btnReload;
    private Button btnUpdate;
    private Button btnDesktop;

    private final List<TabModel> tabs = new ArrayList<>();
    private int activeTabIndex = -1;

    private boolean isDesktopMode = false;
    private String defaultUserAgent = "";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        initViews();
        setupListeners();

        // Buka tab pertama
        createNewTab("novalite://newtab", true);

        // Cek pembaruan APK otomatis di latar belakang setelah 3.5 detik
        webViewContainer.postDelayed(() -> checkForUpdates(false), 3500);
    }

    private void initViews() {
        webViewContainer = findViewById(R.id.webViewContainer);
        txtUrl = findViewById(R.id.txtUrl);
        progressBar = findViewById(R.id.progressBar);
        btnBack = findViewById(R.id.btnBack);
        btnClear = findViewById(R.id.btnClear);
        btnNewTab = findViewById(R.id.btnNewTab);
        btnTabs = findViewById(R.id.btnTabs);
        btnReload = findViewById(R.id.btnReload);
        btnUpdate = findViewById(R.id.btnUpdate);
        btnDesktop = findViewById(R.id.btnDesktop);
    }

    private WebView getActiveWebView() {
        if (activeTabIndex >= 0 && activeTabIndex < tabs.size()) {
            return tabs.get(activeTabIndex).webView;
        }
        return null;
    }

    private TabModel getActiveTab() {
        if (activeTabIndex >= 0 && activeTabIndex < tabs.size()) {
            return tabs.get(activeTabIndex);
        }
        return null;
    }

    public TabModel createNewTab(String url, boolean switchToIt) {
        WebView wv = new WebView(this);
        wv.setLayoutParams(new FrameLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT,
                ViewGroup.LayoutParams.MATCH_PARENT
        ));

        WebSettings settings = wv.getSettings();
        settings.setJavaScriptEnabled(true);
        settings.setDomStorageEnabled(true);
        settings.setDatabaseEnabled(true);
        settings.setAllowFileAccess(true);
        settings.setAllowContentAccess(true);
        settings.setUseWideViewPort(true);
        settings.setLoadWithOverviewMode(true);
        settings.setBuiltInZoomControls(true);
        settings.setDisplayZoomControls(false);
        settings.setSupportMultipleWindows(true);
        settings.setJavaScriptCanOpenWindowsAutomatically(true);

        settings.setSavePassword(true);
        settings.setSaveFormData(true);
        wv.setLayerType(View.LAYER_TYPE_HARDWARE, null);

        if (defaultUserAgent.isEmpty()) {
            defaultUserAgent = settings.getUserAgentString();
        }

        if (isDesktopMode) {
            String desktopUa = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
            settings.setUserAgentString(desktopUa);
        } else {
            settings.setUserAgentString(defaultUserAgent);
        }

        String tabId = UUID.randomUUID().toString();
        TabModel tab = new TabModel(tabId, "Tab Baru", url, wv);
        tabs.add(tab);
        webViewContainer.addView(wv);

        wv.setWebViewClient(new WebViewClient() {
            @Override
            public void onPageStarted(WebView view, String pageUrl, Bitmap favicon) {
                super.onPageStarted(view, pageUrl, favicon);
                tab.url = pageUrl;
                if (getActiveTab() == tab) {
                    progressBar.setVisibility(View.VISIBLE);
                    txtUrl.setText(pageUrl.equals("novalite://newtab") ? "" : pageUrl);
                }
            }

            @Override
            public void onPageFinished(WebView view, String pageUrl) {
                super.onPageFinished(view, pageUrl);
                tab.url = pageUrl;
                if (getActiveTab() == tab) {
                    progressBar.setVisibility(View.GONE);
                    txtUrl.setText(pageUrl.equals("novalite://newtab") ? "" : pageUrl);
                }
            }
        });

        wv.setWebChromeClient(new WebChromeClient() {
            @Override
            public void onProgressChanged(WebView view, int newProgress) {
                super.onProgressChanged(view, newProgress);
                if (getActiveTab() == tab) {
                    progressBar.setProgress(newProgress);
                }
            }

            @Override
            public void onReceivedTitle(WebView view, String title) {
                super.onReceivedTitle(view, title);
                if (title != null && !title.isEmpty()) {
                    tab.title = title;
                }
            }

            @Override
            public boolean onCreateWindow(WebView view, boolean isDialog, boolean isUserGesture, Message resultMsg) {
                TabModel popupTab = createNewTab("about:blank", true);
                WebView.WebViewTransport transport = (WebView.WebViewTransport) resultMsg.obj;
                transport.setWebView(popupTab.webView);
                resultMsg.sendToTarget();
                return true;
            }
        });

        if (url == null || url.isEmpty() || url.equals("novalite://newtab")) {
            loadHomepage(wv);
        } else {
            wv.loadUrl(url);
        }

        if (switchToIt) {
            switchToTab(tabs.size() - 1);
        } else {
            wv.setVisibility(View.GONE);
            updateTabsButtonCount();
        }

        return tab;
    }

    public void switchToTab(int index) {
        if (index < 0 || index >= tabs.size()) return;

        for (int i = 0; i < tabs.size(); i++) {
            if (i == index) {
                tabs.get(i).webView.setVisibility(View.VISIBLE);
            } else {
                tabs.get(i).webView.setVisibility(View.GONE);
            }
        }

        activeTabIndex = index;
        TabModel current = tabs.get(index);
        String currentUrl = current.url;
        txtUrl.setText(currentUrl.equals("novalite://newtab") ? "" : currentUrl);
        updateTabsButtonCount();
    }

    public void closeTab(int index) {
        if (index < 0 || index >= tabs.size()) return;

        TabModel toClose = tabs.get(index);
        webViewContainer.removeView(toClose.webView);
        toClose.webView.destroy();
        tabs.remove(index);

        if (tabs.isEmpty()) {
            createNewTab("novalite://newtab", true);
        } else if (activeTabIndex >= tabs.size()) {
            switchToTab(tabs.size() - 1);
        } else if (activeTabIndex == index) {
            switchToTab(Math.max(0, index - 1));
        } else {
            if (activeTabIndex > index) {
                activeTabIndex--;
            }
            updateTabsButtonCount();
        }
    }

    private void updateTabsButtonCount() {
        btnTabs.setText(String.valueOf(tabs.size()));
    }

    private void showTabSwitcher() {
        if (tabs.isEmpty()) {
            createNewTab("novalite://newtab", true);
            return;
        }

        View dialogView = getLayoutInflater().inflate(R.layout.dialog_tab_switcher, null);
        AlertDialog dialog = new AlertDialog.Builder(this)
                .setView(dialogView)
                .create();

        TextView txtSwitcherTitle = dialogView.findViewById(R.id.txtSwitcherTitle);
        txtSwitcherTitle.setText("📑 Multi-Tab (" + tabs.size() + ")");

        Button btnCloseSwitcher = dialogView.findViewById(R.id.btnCloseSwitcher);
        btnCloseSwitcher.setOnClickListener(v -> dialog.dismiss());

        Button btnAddNewTab = dialogView.findViewById(R.id.btnAddNewTabFromSwitcher);
        btnAddNewTab.setOnClickListener(v -> {
            createNewTab("novalite://newtab", true);
            dialog.dismiss();
        });

        ViewGroup container = dialogView.findViewById(R.id.tabCardsContainer);
        container.removeAllViews();

        for (int i = 0; i < tabs.size(); i++) {
            final int tabIndex = i;
            TabModel t = tabs.get(i);

            View card = getLayoutInflater().inflate(R.layout.item_tab_card, container, false);

            TextView txtTabBadge = card.findViewById(R.id.txtTabBadge);
            TextView txtTabTitle = card.findViewById(R.id.txtTabTitle);
            TextView txtActiveIndicator = card.findViewById(R.id.txtActiveIndicator);
            TextView txtTabUrl = card.findViewById(R.id.txtTabUrl);
            Button btnCloseThisTab = card.findViewById(R.id.btnCloseThisTab);

            txtTabBadge.setText(String.valueOf(i + 1));
            String title = (t.title != null && !t.title.isEmpty()) ? t.title : "Tab " + (i + 1);
            txtTabTitle.setText(title);
            txtTabUrl.setText(t.url != null ? t.url : "about:blank");

            if (i == activeTabIndex) {
                txtActiveIndicator.setVisibility(View.VISIBLE);
                card.setBackgroundColor(0xFF334155); // Highlight tab aktif
            }

            // 1-Klik Langsung Pindah Tab & Tutup Dialog (Tanpa Konfirmasi)
            card.setOnClickListener(v -> {
                switchToTab(tabIndex);
                dialog.dismiss();
            });

            // 1-Klik Langsung Tutup Tab Ini
            btnCloseThisTab.setOnClickListener(v -> {
                closeTab(tabIndex);
                dialog.dismiss();
                if (!tabs.isEmpty()) {
                    showTabSwitcher(); // Segarkan tampilan daftar tab
                }
            });

            container.addView(card);
        }

        dialog.show();
    }

    private void setupListeners() {
        btnBack.setOnClickListener(v -> {
            WebView wv = getActiveWebView();
            if (wv != null && wv.canGoBack()) {
                wv.goBack();
            } else if (tabs.size() > 1) {
                closeTab(activeTabIndex);
            } else {
                Toast.makeText(this, "Sudah di halaman awal", Toast.LENGTH_SHORT).show();
            }
        });

        btnNewTab.setOnClickListener(v -> {
            createNewTab("novalite://newtab", true);
            Toast.makeText(this, "Tab baru dibuka", Toast.LENGTH_SHORT).show();
        });

        btnTabs.setOnClickListener(v -> showTabSwitcher());

        btnReload.setOnClickListener(v -> {
            WebView wv = getActiveWebView();
            if (wv != null) wv.reload();
        });

        btnUpdate.setOnClickListener(v -> {
            Toast.makeText(this, "Memeriksa pembaruan APK...", Toast.LENGTH_SHORT).show();
            checkForUpdates(true);
        });

        btnDesktop.setOnClickListener(v -> toggleDesktopMode());

        btnClear.setOnClickListener(v -> {
            txtUrl.setText("");
            txtUrl.requestFocus();
            btnClear.setVisibility(View.GONE);
        });

        txtUrl.addTextChangedListener(new TextWatcher() {
            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {}

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {
                btnClear.setVisibility(s != null && s.length() > 0 ? View.VISIBLE : View.GONE);
            }

            @Override
            public void afterTextChanged(Editable s) {}
        });

        txtUrl.setOnEditorActionListener((v, actionId, event) -> {
            if (actionId == EditorInfo.IME_ACTION_GO ||
                (event != null && event.getKeyCode() == KeyEvent.KEYCODE_ENTER && event.getAction() == KeyEvent.ACTION_DOWN)) {
                navigateTo(txtUrl.getText().toString().trim());
                return true;
            }
            return false;
        });
    }

    private void navigateTo(String input) {
        WebView wv = getActiveWebView();
        if (wv == null || input == null || input.isEmpty()) return;

        input = input.trim();
        String url;

        if (input.startsWith("http://") || input.startsWith("https://") || input.startsWith("file:///")) {
            url = input;
        } else if (input.matches("^(localhost|127\\.0\\.0\\.1|192\\.168\\.\\d+\\.\\d+|10\\.\\d+\\.\\d+\\.\\d+)(:\\d+)?(/.*)?$")) {
            url = "http://" + input;
        } else if (input.matches("^[a-zA-Z0-9\\-\\.]+\\.[a-zA-Z]{2,}(:\\d+)?(/.*)?$")) {
            url = "https://" + input;
        } else {
            try {
                url = "https://www.google.com/search?q=" + URLEncoder.encode(input, "UTF-8");
            } catch (Exception e) {
                url = "https://www.google.com/search?q=" + input;
            }
        }

        wv.loadUrl(url);
    }

    private void toggleDesktopMode() {
        isDesktopMode = !isDesktopMode;
        WebView wv = getActiveWebView();
        if (wv != null) {
            WebSettings settings = wv.getSettings();
            if (isDesktopMode) {
                String desktopUa = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/123.0.0.0 Safari/537.36";
                settings.setUserAgentString(desktopUa);
                btnDesktop.setText("📱");
                Toast.makeText(this, "Mode Desktop Aktif (User-Agent PC)", Toast.LENGTH_SHORT).show();
            } else {
                settings.setUserAgentString(defaultUserAgent);
                btnDesktop.setText("💻");
                Toast.makeText(this, "Mode Mobile Aktif", Toast.LENGTH_SHORT).show();
            }
            wv.reload();
        }
    }

    private void loadHomepage(WebView wv) {
        String html = "<!DOCTYPE html><html><head><meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                "<style>body{background:#0F172A;color:#fff;font-family:sans-serif;text-align:center;padding:40px 20px;}" +
                "h1{color:#38BDF8;font-size:28px;margin-bottom:6px;}" +
                "p{color:#94A3B8;font-size:14px;margin-bottom:24px;}" +
                ".btn{display:inline-block;background:#1E293B;color:#fff;padding:10px 18px;border-radius:10px;text-decoration:none;margin:6px;border:1px solid #334155;}" +
                "</style></head><body>" +
                "<h1>NovaBrowser</h1>" +
                "<p>Edisi Android &bull; Multi-Tab &bull; Cepat &bull; Hemat Baterai</p>" +
                "<div>" +
                "<a href='https://google.com' class='btn'>🔍 Google</a>" +
                "<a href='https://github.com' class='btn'>🐙 GitHub</a>" +
                "<a href='https://news.google.com' class='btn'>📰 Berita</a>" +
                "</div>" +
                "</body></html>";
        wv.loadDataWithBaseURL("novalite://newtab", html, "text/html", "UTF-8", null);
    }

    private void checkForUpdates(boolean isManual) {
        new Thread(() -> {
            String latestVersion = "";
            String changelog = "";
            String apkUrl = "https://github.com/wandiapriliyandi/novabrowser/releases/latest";

            // 1. Coba periksa via GitHub Releases API Resmi
            try {
                URL url = new URL("https://api.github.com/repos/wandiapriliyandi/novabrowser/releases/latest");
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setConnectTimeout(6000);
                conn.setReadTimeout(6000);
                conn.setRequestMethod("GET");
                conn.setRequestProperty("User-Agent", "NovaBrowser-Android");
                conn.setRequestProperty("Accept", "application/vnd.github.v3+json");

                if (conn.getResponseCode() == 200) {
                    BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));
                    StringBuilder sb = new StringBuilder();
                    String line;
                    while ((line = reader.readLine()) != null) {
                        sb.append(line);
                    }
                    reader.close();

                    JSONObject json = new JSONObject(sb.toString());
                    latestVersion = json.optString("tag_name", "").replace("v", "");
                    changelog = json.optString("body", "Pembaruan GitHub Release terbaru.");

                    // Cari asset file APK
                    JSONArray assets = json.optJSONArray("assets");
                    if (assets != null) {
                        for (int i = 0; i < assets.length(); i++) {
                            JSONObject asset = assets.getJSONObject(i);
                            String assetName = asset.optString("name", "");
                            if (assetName.endsWith(".apk")) {
                                apkUrl = asset.optString("browser_download_url", apkUrl);
                                break;
                            }
                        }
                    }
                }
            } catch (Exception ignored) { }

            // 2. Fallback jika GitHub API rate-limited
            if (latestVersion == null || latestVersion.isEmpty()) {
                try {
                    URL url = new URL("https://raw.githubusercontent.com/wandiapriliyandi/novabrowser/main/version.json?t=" + System.currentTimeMillis());
                    HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                    conn.setConnectTimeout(6000);
                    conn.setReadTimeout(6000);
                    conn.setRequestMethod("GET");
                    conn.setRequestProperty("User-Agent", "NovaBrowser-Android");

                    if (conn.getResponseCode() == 200) {
                        BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));
                        StringBuilder sb = new StringBuilder();
                        String line;
                        while ((line = reader.readLine()) != null) {
                            sb.append(line);
                        }
                        reader.close();

                        JSONObject json = new JSONObject(sb.toString());
                        latestVersion = json.optString("version", "");
                        changelog = json.optString("changelog", "Pembaruan stabilitas dan performa.");
                        apkUrl = json.optString("apkDownloadUrl", apkUrl);
                    }
                } catch (Exception ex) {
                    if (isManual) {
                        runOnUiThread(() -> Toast.makeText(this, "Koneksi update gagal: " + ex.getMessage(), Toast.LENGTH_SHORT).show());
                    }
                    return;
                }
            }

            try {
                String currentVersion = getPackageManager().getPackageInfo(getPackageName(), 0).versionName;
                final String finalVersion = latestVersion;
                final String finalChangelog = changelog;
                final String finalApkUrl = apkUrl;

                if (isNewerVersion(finalVersion, currentVersion)) {
                    runOnUiThread(() -> showUpdateDialog(finalVersion, finalChangelog, finalApkUrl));
                } else if (isManual) {
                    runOnUiThread(() -> Toast.makeText(this, "NovaBrowser sudah versi terbaru (v" + currentVersion + ")", Toast.LENGTH_LONG).show());
                }
            } catch (Exception e) {
                if (isManual) {
                    runOnUiThread(() -> Toast.makeText(this, "Gagal memproses versi: " + e.getMessage(), Toast.LENGTH_SHORT).show());
                }
            }
        }).start();
    }

    private boolean isNewerVersion(String latest, String current) {
        if (latest == null || current == null || latest.isEmpty()) return false;
        try {
            String[] v1 = latest.replace("v", "").split("\\.");
            String[] v2 = current.replace("v", "").split("\\.");
            int len = Math.max(v1.length, v2.length);
            for (int i = 0; i < len; i++) {
                int p1 = i < v1.length ? Integer.parseInt(v1[i]) : 0;
                int p2 = i < v2.length ? Integer.parseInt(v2[i]) : 0;
                if (p1 > p2) return true;
                if (p1 < p2) return false;
            }
        } catch (Exception ignored) {}
        return false;
    }

    private void showUpdateDialog(String newVersion, String changelog, String apkUrl) {
        new AlertDialog.Builder(this)
                .setTitle("🚀 Pembaruan APK Tersedia (v" + newVersion + ")")
                .setMessage("Versi terbaru NovaBrowser telah dirilis!\n\nCatatan Rilis:\n" + changelog + "\n\nUnduh file APK terbaru sekarang?")
                .setPositiveButton("Unduh APK", (dialog, which) -> {
                    try {
                        Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(apkUrl));
                        startActivity(intent);
                    } catch (Exception ex) {
                        Toast.makeText(this, "Gagal membuka tautan unduhan: " + ex.getMessage(), Toast.LENGTH_SHORT).show();
                    }
                })
                .setNegativeButton("Nanti", null)
                .show();
    }

    @Override
    protected void onPause() {
        super.onPause();
        WebView wv = getActiveWebView();
        if (wv != null) {
            wv.onPause();
            wv.pauseTimers();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        WebView wv = getActiveWebView();
        if (wv != null) {
            wv.onResume();
            wv.resumeTimers();
        }
    }

    @Override
    protected void onDestroy() {
        for (TabModel t : tabs) {
            if (t.webView != null) {
                t.webView.destroy();
            }
        }
        tabs.clear();
        super.onDestroy();
    }

    @Override
    public void onBackPressed() {
        WebView wv = getActiveWebView();
        if (wv != null && wv.canGoBack()) {
            wv.goBack();
        } else if (tabs.size() > 1) {
            closeTab(activeTabIndex);
            Toast.makeText(this, "Tab ditutup", Toast.LENGTH_SHORT).show();
        } else {
            super.onBackPressed();
        }
    }
}
