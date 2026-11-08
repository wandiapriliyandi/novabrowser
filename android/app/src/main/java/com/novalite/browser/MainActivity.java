package com.novalite.browser;

import android.app.Activity;
import android.graphics.Bitmap;
import android.os.Build;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.KeyEvent;
import android.view.View;
import android.view.inputmethod.EditorInfo;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

public class MainActivity extends Activity {

    private WebView webView;
    private EditText txtUrl;
    private ProgressBar progressBar;
    private Button btnBack;
    private Button btnClear;
    private Button btnReload;
    private Button btnUpdate;
    private Button btnDesktop;

    private boolean isDesktopMode = false;
    private String defaultUserAgent = "";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        initViews();
        configureWebView();
        setupListeners();

        // Buka halaman beranda awal
        loadHomepage();

        // Cek pembaruan APK otomatis di latar belakang setelah 3.5 detik
        webView.postDelayed(() -> checkForUpdates(false), 3500);
    }

    private void initViews() {
        webView = findViewById(R.id.webView);
        txtUrl = findViewById(R.id.txtUrl);
        progressBar = findViewById(R.id.progressBar);
        btnBack = findViewById(R.id.btnBack);
        btnClear = findViewById(R.id.btnClear);
        btnReload = findViewById(R.id.btnReload);
        btnUpdate = findViewById(R.id.btnUpdate);
        btnDesktop = findViewById(R.id.btnDesktop);
    }

    private void configureWebView() {
        WebSettings settings = webView.getSettings();
        settings.setJavaScriptEnabled(true);
        settings.setDomStorageEnabled(true);
        settings.setDatabaseEnabled(true);
        settings.setAllowFileAccess(true);
        settings.setAllowContentAccess(true);
        settings.setUseWideViewPort(true);
        settings.setLoadWithOverviewMode(true);
        settings.setBuiltInZoomControls(true);
        settings.setDisplayZoomControls(false);

        // Simpan Sandi & Autofill
        settings.setSavePassword(true);
        settings.setSaveFormData(true);

        // Hardware Acceleration
        webView.setLayerType(View.LAYER_TYPE_HARDWARE, null);

        // Aktifkan Remote Chrome DevTools Debugging (Bisa di-inspect via PC di chrome://inspect)
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.KITKAT) {
            WebView.setWebContentsDebuggingEnabled(true);
        }

        defaultUserAgent = settings.getUserAgentString();

        webView.setWebViewClient(new WebViewClient() {
            @Override
            public void onPageStarted(WebView view, String url, Bitmap favicon) {
                super.onPageStarted(view, url, favicon);
                progressBar.setVisibility(View.VISIBLE);
                txtUrl.setText(url);
            }

            @Override
            public void onPageFinished(WebView view, String url) {
                super.onPageFinished(view, url);
                progressBar.setVisibility(View.GONE);
                txtUrl.setText(url);
            }
        });

        webView.setWebChromeClient(new WebChromeClient() {
            @Override
            public void onProgressChanged(WebView view, int newProgress) {
                super.onProgressChanged(view, newProgress);
                progressBar.setProgress(newProgress);
            }
        });
    }

    private void setupListeners() {
        btnBack.setOnClickListener(v -> {
            if (webView.canGoBack()) {
                webView.goBack();
            } else {
                Toast.makeText(this, "Sudah di halaman awal", Toast.LENGTH_SHORT).show();
            }
        });

        btnReload.setOnClickListener(v -> webView.reload());

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
        if (input == null || input.isEmpty()) return;

        input = input.trim();
        String url;

        if (input.startsWith("http://") || input.startsWith("https://") || input.startsWith("file:///")) {
            url = input;
        } else if (input.matches("^(localhost|127\\.0\\.0\\.1|192\\.168\\.\\d+\\.\\d+|10\\.\\d+\\.\\d+\\.\\d+)(:\\d+)?(/.*)?$")) {
            url = "http://" + input;
        } else if (input.matches("^[a-zA-Z0-9\\-\\.]+\\.[a-zA-Z]{2,}(:\\d+)?(/.*)?$")) {
            url = "https://" + input;
        } else {
            url = "https://www.google.com/search?q=" + java.net.URLEncoder.encode(input);
        }

        webView.loadUrl(url);
    }

    private void toggleDesktopMode() {
        isDesktopMode = !isDesktopMode;
        WebSettings settings = webView.getSettings();
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
        webView.reload();
    }

    private void loadHomepage() {
        String html = "<!DOCTYPE html><html><head><meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                "<style>body{background:#0F172A;color:#fff;font-family:sans-serif;text-align:center;padding:40px 20px;}" +
                "h1{color:#38BDF8;font-size:28px;margin-bottom:6px;}" +
                "p{color:#94A3B8;font-size:14px;margin-bottom:24px;}" +
                ".btn{display:inline-block;background:#1E293B;color:#fff;padding:10px 18px;border-radius:10px;text-decoration:none;margin:6px;border:1px solid #334155;}" +
                "</style></head><body>" +
                "<h1>NovaBrowser</h1>" +
                "<p>Edisi Android &bull; Cepat &bull; Hemat Baterai &bull; Bersih</p>" +
                "<div>" +
                "<a href='https://google.com' class='btn'>🔍 Google</a>" +
                "<a href='https://github.com' class='btn'>🐙 GitHub</a>" +
                "<a href='https://news.google.com' class='btn'>📰 Berita</a>" +
                "</div>" +
                "</body></html>";
        webView.loadDataWithBaseURL("novalite://newtab", html, "text/html", "UTF-8", null);
    }

    private void checkForUpdates(boolean isManual) {
        new Thread(() -> {
            String latestVersion = "";
            String changelog = "";
            String apkUrl = "https://github.com/wandiapriliyandi/novabrowser/releases/latest";

            // 1. Coba periksa via GitHub Releases API Resmi
            try {
                java.net.URL url = new java.net.URL("https://api.github.com/repos/wandiapriliyandi/novabrowser/releases/latest");
                java.net.HttpURLConnection conn = (java.net.HttpURLConnection) url.openConnection();
                conn.setConnectTimeout(6000);
                conn.setReadTimeout(6000);
                conn.setRequestMethod("GET");
                conn.setRequestProperty("User-Agent", "NovaBrowser-Android");
                conn.setRequestProperty("Accept", "application/vnd.github.v3+json");

                if (conn.getResponseCode() == 200) {
                    java.io.BufferedReader reader = new java.io.BufferedReader(new java.io.InputStreamReader(conn.getInputStream()));
                    StringBuilder sb = new StringBuilder();
                    String line;
                    while ((line = reader.readLine()) != null) {
                        sb.append(line);
                    }
                    reader.close();

                    org.json.JSONObject json = new org.json.JSONObject(sb.toString());
                    latestVersion = json.optString("tag_name", "").replace("v", "");
                    changelog = json.optString("body", "Pembaruan GitHub Release terbaru.");

                    // Cari asset file APK
                    org.json.JSONArray assets = json.optJSONArray("assets");
                    if (assets != null) {
                        for (int i = 0; i < assets.length(); i++) {
                            org.json.JSONObject asset = assets.getJSONObject(i);
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
                    java.net.URL url = new java.net.URL("https://raw.githubusercontent.com/wandiapriliyandi/novabrowser/main/version.json?t=" + System.currentTimeMillis());
                    java.net.HttpURLConnection conn = (java.net.HttpURLConnection) url.openConnection();
                    conn.setConnectTimeout(6000);
                    conn.setReadTimeout(6000);
                    conn.setRequestMethod("GET");
                    conn.setRequestProperty("User-Agent", "NovaBrowser-Android");

                    if (conn.getResponseCode() == 200) {
                        java.io.BufferedReader reader = new java.io.BufferedReader(new java.io.InputStreamReader(conn.getInputStream()));
                        StringBuilder sb = new StringBuilder();
                        String line;
                        while ((line = reader.readLine()) != null) {
                            sb.append(line);
                        }
                        reader.close();

                        org.json.JSONObject json = new org.json.JSONObject(sb.toString());
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
        new android.app.AlertDialog.Builder(this)
                .setTitle("🚀 Pembaruan APK Tersedia (v" + newVersion + ")")
                .setMessage("Versi terbaru NovaBrowser telah dirilis!\n\nCatatan Rilis:\n" + changelog + "\n\nUnduh file APK terbaru sekarang?")
                .setPositiveButton("Unduh APK", (dialog, which) -> {
                    try {
                        android.content.Intent intent = new android.content.Intent(android.content.Intent.ACTION_VIEW, android.net.Uri.parse(apkUrl));
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
        if (webView != null) {
            webView.onPause();
            webView.pauseTimers(); // Membekukan animasi dan timer JS di latar belakang untuk menghemat baterai
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        if (webView != null) {
            webView.onResume();
            webView.resumeTimers();
        }
    }

    @Override
    protected void onDestroy() {
        if (webView != null) {
            webView.destroy();
        }
        super.onDestroy();
    }

    @Override
    public void onBackPressed() {
        if (webView.canGoBack()) {
            webView.goBack();
        } else {
            super.onBackPressed();
        }
    }
}
