package com.novalite.browser;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Message;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.KeyEvent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.view.inputmethod.InputMethodManager;
import android.webkit.WebChromeClient;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.AutoCompleteTextView;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.Filter;
import android.widget.Filterable;
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
import java.util.Arrays;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
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

    public static class SuggestionItem {
        public final String text;
        public final boolean isUrl;

        public SuggestionItem(String text, boolean isUrl) {
            this.text = text;
            this.isUrl = isUrl;
        }

        @Override
        public String toString() {
            return text;
        }
    }

    public static class OmniboxSuggestAdapter extends BaseAdapter implements Filterable {
        private final Context context;
        private final List<SuggestionItem> items = new ArrayList<>();
        private final List<String> historyList;
        private final List<String> defaultUrls;

        public OmniboxSuggestAdapter(Context context, List<String> historyList, List<String> defaultUrls) {
            this.context = context;
            this.historyList = historyList;
            this.defaultUrls = defaultUrls;
        }

        @Override
        public int getCount() {
            return items.size();
        }

        @Override
        public SuggestionItem getItem(int position) {
            return items.get(position);
        }

        @Override
        public long getItemId(int position) {
            return position;
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            if (convertView == null) {
                convertView = LayoutInflater.from(context).inflate(R.layout.item_autocomplete_suggestion, parent, false);
            }
            SuggestionItem item = getItem(position);
            TextView txtIcon = convertView.findViewById(R.id.txtSuggestionIcon);
            TextView txtText = convertView.findViewById(R.id.txtSuggestionText);

            if (txtIcon != null) {
                txtIcon.setText(item.isUrl ? "🌐" : "🔍");
            }
            if (txtText != null) {
                txtText.setText(item.text);
            }

            return convertView;
        }

        @Override
        public Filter getFilter() {
            return new Filter() {
                @Override
                protected FilterResults performFiltering(CharSequence constraint) {
                    FilterResults results = new FilterResults();
                    if (constraint == null || constraint.toString().trim().isEmpty()) {
                        results.values = new ArrayList<SuggestionItem>();
                        results.count = 0;
                        return results;
                    }

                    String query = constraint.toString().trim().toLowerCase();
                    List<SuggestionItem> filtered = new ArrayList<>();
                    Set<String> added = new HashSet<>();

                    // 1. Cek riwayat lokal pengguna (prioritas tertinggi)
                    synchronized (historyList) {
                        for (String h : historyList) {
                            if (h.toLowerCase().contains(query)) {
                                if (added.add(h)) {
                                    filtered.add(new SuggestionItem(h, true));
                                    if (filtered.size() >= 6) break;
                                }
                            }
                        }
                    }

                    // 2. Cek situs populer default & localhost
                    for (String d : defaultUrls) {
                        if (d.toLowerCase().contains(query)) {
                            if (added.add(d)) {
                                filtered.add(new SuggestionItem(d, true));
                                if (filtered.size() >= 10) break;
                            }
                        }
                    }

                    // 3. Jika bukan URL langsung, ambil saran Google Suggest real-time
                    if (!query.contains(".") && !query.startsWith("http") && !query.startsWith("localhost")) {
                        List<String> googleList = fetchGoogleSuggestions(query);
                        for (String s : googleList) {
                            if (added.add(s)) {
                                filtered.add(new SuggestionItem(s, false));
                                if (filtered.size() >= 12) break;
                            }
                        }
                    }

                    results.values = filtered;
                    results.count = filtered.size();
                    return results;
                }

                @SuppressWarnings("unchecked")
                @Override
                protected void publishResults(CharSequence constraint, FilterResults results) {
                    items.clear();
                    if (results != null && results.values != null) {
                        items.addAll((List<SuggestionItem>) results.values);
                    }
                    notifyDataSetChanged();
                }

                @Override
                public CharSequence convertResultToString(Object resultValue) {
                    if (resultValue instanceof SuggestionItem) {
                        return ((SuggestionItem) resultValue).text;
                    }
                    return super.convertResultToString(resultValue);
                }
            };
        }

        private List<String> fetchGoogleSuggestions(String query) {
            List<String> list = new ArrayList<>();
            try {
                String encoded = URLEncoder.encode(query, "UTF-8");
                URL url = new URL("https://suggestqueries.google.com/complete/search?client=chrome&q=" + encoded);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setConnectTimeout(1200);
                conn.setReadTimeout(1200);
                conn.setRequestProperty("User-Agent", "NovaBrowser-Android");
                if (conn.getResponseCode() == 200) {
                    BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));
                    StringBuilder sb = new StringBuilder();
                    String line;
                    while ((line = reader.readLine()) != null) {
                        sb.append(line);
                    }
                    reader.close();
                    JSONArray root = new JSONArray(sb.toString());
                    if (root.length() > 1) {
                        JSONArray suggestions = root.getJSONArray(1);
                        for (int i = 0; i < suggestions.length() && i < 5; i++) {
                            list.add(suggestions.getString(i));
                        }
                    }
                }
            } catch (Exception ignored) {}
            return list;
        }
    }

    private static final String PREFS_NAME = "novalite_prefs";
    private static final String KEY_HISTORY = "url_history";
    private final List<String> historyList = new ArrayList<>();

    private final List<String> defaultUrls = Arrays.asList(
            "google.com",
            "https://google.com",
            "youtube.com",
            "https://youtube.com",
            "github.com",
            "https://github.com",
            "chatgpt.com",
            "https://chatgpt.com",
            "wikipedia.org",
            "https://wikipedia.org",
            "stackoverflow.com",
            "https://stackoverflow.com",
            "reddit.com",
            "https://reddit.com",
            "twitter.com",
            "https://x.com",
            "instagram.com",
            "https://instagram.com",
            "facebook.com",
            "https://facebook.com",
            "detik.com",
            "https://detik.com",
            "kompas.com",
            "https://kompas.com",
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
            "127.0.0.1:3000"
    );

    private FrameLayout webViewContainer;
    private AutoCompleteTextView txtUrl;
    private OmniboxSuggestAdapter suggestAdapter;
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

        loadHistory();
        initViews();
        setupOmnibox();
        setupListeners();

        // Buka tab pertama
        createNewTab("novalite://newtab", true);

        // Cek pembaruan APK otomatis di latar belakang setelah 3.5 detik
        webViewContainer.postDelayed(() -> checkForUpdates(false), 3500);
    }

    private void loadHistory() {
        try {
            SharedPreferences sp = getSharedPreferences(PREFS_NAME, MODE_PRIVATE);
            String jsonStr = sp.getString(KEY_HISTORY, "[]");
            JSONArray arr = new JSONArray(jsonStr);
            synchronized (historyList) {
                historyList.clear();
                for (int i = 0; i < arr.length(); i++) {
                    historyList.add(arr.getString(i));
                }
            }
        } catch (Exception ignored) {}
    }

    private void saveToHistory(String url) {
        if (url == null || url.trim().isEmpty() ||
                url.equals("novalite://newtab") ||
                url.equals("about:blank") ||
                url.startsWith("data:")) return;

        url = url.trim();
        synchronized (historyList) {
            historyList.remove(url);
            historyList.add(0, url);
            if (historyList.size() > 100) {
                historyList.remove(historyList.size() - 1);
            }
            try {
                JSONArray arr = new JSONArray();
                for (String u : historyList) {
                    arr.put(u);
                }
                getSharedPreferences(PREFS_NAME, MODE_PRIVATE)
                        .edit()
                        .putString(KEY_HISTORY, arr.toString())
                        .apply();
            } catch (Exception ignored) {}
        }
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

    private void setupOmnibox() {
        suggestAdapter = new OmniboxSuggestAdapter(this, historyList, defaultUrls);
        txtUrl.setAdapter(suggestAdapter);
        txtUrl.setThreshold(1);
        try {
            txtUrl.setDropDownBackgroundResource(R.drawable.bg_autocomplete_dropdown);
        } catch (Exception ignored) {}
    }

    private void hideSoftKeyboard() {
        try {
            InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
            if (imm != null) {
                View focus = getCurrentFocus();
                if (focus != null) {
                    imm.hideSoftInputFromWindow(focus.getWindowToken(), 0);
                } else if (txtUrl != null) {
                    imm.hideSoftInputFromWindow(txtUrl.getWindowToken(), 0);
                }
            }
        } catch (Exception ignored) {}
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

        try {
            settings.setSaveFormData(true);
        } catch (Exception ignored) { }

        try {
            wv.setLayerType(View.LAYER_TYPE_HARDWARE, null);
        } catch (Exception ignored) { }

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
                    txtUrl.setText(pageUrl.equals("novalite://newtab") ? "" : pageUrl, false);
                }
            }

            @Override
            public void onPageFinished(WebView view, String pageUrl) {
                super.onPageFinished(view, pageUrl);
                tab.url = pageUrl;
                saveToHistory(pageUrl);
                if (getActiveTab() == tab) {
                    progressBar.setVisibility(View.GONE);
                    txtUrl.setText(pageUrl.equals("novalite://newtab") ? "" : pageUrl, false);
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
        txtUrl.setText(currentUrl.equals("novalite://newtab") ? "" : currentUrl, false);
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

        txtUrl.setOnItemClickListener((parent, view, position, id) -> {
            SuggestionItem item = (SuggestionItem) parent.getItemAtPosition(position);
            if (item != null) {
                txtUrl.setText(item.text, false);
                txtUrl.setSelection(item.text.length());
                navigateTo(item.text);
                hideSoftKeyboard();
                txtUrl.dismissDropDown();
            }
        });

        txtUrl.setOnEditorActionListener((v, actionId, event) -> {
            if (actionId == EditorInfo.IME_ACTION_GO ||
                (event != null && event.getKeyCode() == KeyEvent.KEYCODE_ENTER && event.getAction() == KeyEvent.ACTION_DOWN)) {
                String input = txtUrl.getText().toString().trim();
                navigateTo(input);
                hideSoftKeyboard();
                txtUrl.dismissDropDown();
                return true;
            }
            return false;
        });
    }

    private void navigateTo(String input) {
        WebView wv = getActiveWebView();
        if (wv == null || input == null || input.isEmpty()) return;

        input = input.trim();
        saveToHistory(input);
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
        wv.loadDataWithBaseURL(null, html, "text/html", "UTF-8", null);
    }

    private String cleanChangelog(String raw) {
        if (raw == null || raw.trim().isEmpty()) {
            return "Pembaruan performa dan perbaikan bug.";
        }

        String text = raw;
        // Jika ada section "Catatan Rilis" atau "Yang Baru", ambil bagian tersebut
        if (text.contains("Catatan Rilis") || text.contains("Yang Baru")) {
            int startIdx = text.indexOf("Catatan Rilis");
            if (startIdx == -1) startIdx = text.indexOf("Yang Baru");

            int lineEnd = text.indexOf("\n", startIdx);
            if (lineEnd != -1) {
                text = text.substring(lineEnd + 1);
            } else {
                text = text.substring(startIdx);
            }

            int nextSection = text.indexOf("### ");
            if (nextSection != -1) {
                text = text.substring(0, nextSection);
            }
        }

        text = text.replace("##", "")
                   .replace("**", "")
                   .replace("__", "")
                   .trim();

        return text.isEmpty() ? raw.trim() : text;
    }

    private void checkForUpdates(boolean isManual) {
        new Thread(() -> {
            String latestVersion = "";
            String changelog = "";
            String apkUrl = "https://github.com/wandiapriliyandi/novabrowser/releases/latest";

            // 1. Ambil data versi & changelog spesifik langsung dari version.json (anti rate-limit dan selalu spesifik)
            try {
                URL vUrl = new URL("https://raw.githubusercontent.com/wandiapriliyandi/novabrowser/main/version.json?t=" + System.currentTimeMillis());
                HttpURLConnection vConn = (HttpURLConnection) vUrl.openConnection();
                vConn.setConnectTimeout(5000);
                vConn.setReadTimeout(5000);
                vConn.setRequestProperty("User-Agent", "NovaBrowser-Android");

                if (vConn.getResponseCode() == 200) {
                    BufferedReader reader = new BufferedReader(new InputStreamReader(vConn.getInputStream()));
                    StringBuilder sb = new StringBuilder();
                    String line;
                    while ((line = reader.readLine()) != null) {
                        sb.append(line).append("\n");
                    }
                    reader.close();

                    JSONObject vJson = new JSONObject(sb.toString());
                    latestVersion = vJson.optString("version", "").replace("v", "");
                    changelog = vJson.optString("changelog", "");
                    apkUrl = vJson.optString("apkDownloadUrl", apkUrl);
                }
            } catch (Exception ignored) { }

            // 2. Cek GitHub Releases API resmi untuk memastikan rilis APK terbaru dan download URL langsung
            try {
                URL url = new URL("https://api.github.com/repos/wandiapriliyandi/novabrowser/releases/latest");
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                conn.setConnectTimeout(5000);
                conn.setReadTimeout(5000);
                conn.setRequestProperty("User-Agent", "NovaBrowser-Android");
                conn.setRequestProperty("Accept", "application/vnd.github.v3+json");

                if (conn.getResponseCode() == 200) {
                    BufferedReader reader = new BufferedReader(new InputStreamReader(conn.getInputStream()));
                    StringBuilder sb = new StringBuilder();
                    String line;
                    while ((line = reader.readLine()) != null) {
                        sb.append(line).append("\n");
                    }
                    reader.close();

                    JSONObject json = new JSONObject(sb.toString());
                    String ghVersion = json.optString("tag_name", "").replace("v", "");
                    if (!ghVersion.isEmpty()) {
                        latestVersion = ghVersion;
                    }

                    // Jika version.json belum terisi changelog, ambil dari body GitHub Release yang dibersihkan
                    if (changelog == null || changelog.trim().isEmpty()) {
                        String ghBody = json.optString("body", "");
                        changelog = cleanChangelog(ghBody);
                    }

                    // Cari direct asset file APK
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

            if (latestVersion == null || latestVersion.isEmpty()) {
                if (isManual) {
                    runOnUiThread(() -> Toast.makeText(this, "Tidak dapat memeriksa pembaruan: server tidak merespons.", Toast.LENGTH_SHORT).show());
                }
                return;
            }

            try {
                String currentVersion = getPackageManager().getPackageInfo(getPackageName(), 0).versionName;
                final String finalVersion = latestVersion;
                final String finalChangelog = (changelog != null && !changelog.trim().isEmpty())
                        ? changelog.trim()
                        : "Pembaruan performa dan peningkatan stabilitas.";
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
        if (isFinishing() || (Build.VERSION.SDK_INT >= Build.VERSION_CODES.JELLY_BEAN_MR1 && isDestroyed())) {
            return;
        }

        try {
            new AlertDialog.Builder(this)
                    .setTitle("🚀 Pembaruan Tersedia (v" + newVersion + ")")
                    .setMessage("Versi terbaru NovaBrowser telah dirilis!\n\n📢 Apa yang Baru:\n" + changelog + "\n\nUnduh dan pasang file APK terbaru sekarang?")
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
        } catch (Exception ignored) { }
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
