package com.novalite.browser;

import android.app.Activity;
import android.graphics.Bitmap;
import android.os.Build;
import android.os.Bundle;
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
    private Button btnDev;
    private Button btnDesktop;
    private Button btnReload;

    private boolean isDesktopMode = false;
    private boolean isDevToolsInjected = false;
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
    }

    private void initViews() {
        webView = findViewById(R.id.webView);
        txtUrl = findViewById(R.id.txtUrl);
        progressBar = findViewById(R.id.progressBar);
        btnBack = findViewById(R.id.btnBack);
        btnDev = findViewById(R.id.btnDev);
        btnDesktop = findViewById(R.id.btnDesktop);
        btnReload = findViewById(R.id.btnReload);
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
                isDevToolsInjected = false;
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

        btnDesktop.setOnClickListener(v -> toggleDesktopMode());

        btnDev.setOnClickListener(v -> toggleMobileDevTools());

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

    private void toggleMobileDevTools() {
        if (!isDevToolsInjected) {
            // Injeksi Eruda Mobile DevTools (Console, Elements, Network, Storage) langsung ke halaman web
            String jsCode = "javascript:(function () { " +
                    "if (window.eruda) { window.eruda.show(); return; } " +
                    "var script = document.createElement('script'); " +
                    "script.src='https://cdn.jsdelivr.net/npm/eruda'; " +
                    "script.onload = function () { eruda.init(); eruda.show(); }; " +
                    "document.body.appendChild(script); " +
                    "})();";
            webView.evaluateJavascript(jsCode, null);
            isDevToolsInjected = true;
            Toast.makeText(this, "🛠️ DevTools Mobile (Eruda) Berhasil Dimuat!", Toast.LENGTH_SHORT).show();
        } else {
            webView.evaluateJavascript("javascript:if(window.eruda){ eruda.hide(); }", null);
            isDevToolsInjected = false;
        }
    }

    private void loadHomepage() {
        String html = "<!DOCTYPE html><html><head><meta name='viewport' content='width=device-width, initial-scale=1.0'>" +
                "<style>body{background:#0F172A;color:#fff;font-family:sans-serif;text-align:center;padding:40px 20px;}" +
                "h1{color:#38BDF8;font-size:28px;margin-bottom:6px;}" +
                "p{color:#94A3B8;font-size:14px;margin-bottom:24px;}" +
                ".btn{display:inline-block;background:#1E293B;color:#fff;padding:10px 18px;border-radius:10px;text-decoration:none;margin:6px;border:1px solid #334155;}" +
                "</style></head><body>" +
                "<h1>NovaLite Browser</h1>" +
                "<p>Edisi Android &bull; Sangat Ringan &bull; Mode Developer</p>" +
                "<div>" +
                "<a href='https://google.com' class='btn'>🔍 Google</a>" +
                "<a href='https://github.com' class='btn'>🐙 GitHub</a>" +
                "<a href='http://10.0.2.2:8000' class='btn'>💻 Localhost PC</a>" +
                "</div>" +
                "<div style='margin-top:30px;color:#64748B;font-size:12px;'>" +
                "Klik tombol 🛠️ di atas untuk membuka DevTools Console di HP!" +
                "</div>" +
                "</body></html>";
        webView.loadDataWithBaseURL("novalite://newtab", html, "text/html", "UTF-8", null);
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
