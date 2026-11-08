using System;
using System.Drawing;
using System.Windows.Forms;

namespace NovaBrowser {
    public class HistoryForm : Form {
        private BookmarkHistoryManager manager;
        private MainForm mainForm;

        private TabControl tabControl;

        // History Controls
        private TextBox txtSearchHistory;
        private ListView lvHistory;
        private Button btnOpenHistory;
        private Button btnDeleteHistory;
        private Button btnClearAllHistory;

        // Bookmark Controls
        private ListView lvBookmarks;
        private Button btnOpenBookmark;
        private Button btnDeleteBookmark;
        private TextBox txtNewBmTitle;
        private TextBox txtNewBmUrl;
        private Button btnAddBookmark;

        public HistoryForm(MainForm form, BookmarkHistoryManager mgr, int initialTab = 0) {
            this.mainForm = form;
            this.manager = mgr;

            this.Text = "🕒 Riwayat & ⭐ Bookmark - NovaLite";
            this.Size = new Size(820, 560);
            this.MinimumSize = new Size(600, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(15, 23, 42);
            this.ForeColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            BuildUI();
            if (initialTab == 1 && tabControl.TabPages.Count > 1) {
                tabControl.SelectedIndex = 1;
            }
        }

        private void BuildUI() {
            tabControl = new TabControl() {
                Dock = DockStyle.Fill,
                Padding = new Point(14, 6)
            };

            // TAB 1: HISTORY
            TabPage tabHist = new TabPage("🕒 Riwayat Penjelajahan") {
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10)
            };
            BuildHistoryTab(tabHist);
            tabControl.TabPages.Add(tabHist);

            // TAB 2: BOOKMARKS
            TabPage tabBm = new TabPage("⭐ Kelola Bookmark") {
                BackColor = Color.FromArgb(30, 41, 59),
                Padding = new Padding(10)
            };
            BuildBookmarkTab(tabBm);
            tabControl.TabPages.Add(tabBm);

            this.Controls.Add(tabControl);
        }

        private void BuildHistoryTab(TabPage page) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Top Search Bar
            FlowLayoutPanel top = new FlowLayoutPanel() { Dock = DockStyle.Fill, WrapContents = false };
            Label lblSearch = new Label() { Text = "🔍 Cari Riwayat:", AutoSize = true, Padding = new Padding(0, 6, 4, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            txtSearchHistory = new TextBox() { Width = 350, Font = new Font("Segoe UI", 10), BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White };
            txtSearchHistory.TextChanged += (s, e) => RefreshHistoryList();
            top.Controls.Add(lblSearch);
            top.Controls.Add(txtSearchHistory);

            // ListView
            lvHistory = new ListView() {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f)
            };
            lvHistory.Columns.Add("Waktu Kunjungan", 150);
            lvHistory.Columns.Add("Judul Halaman", 280);
            lvHistory.Columns.Add("Alamat URL", 340);
            lvHistory.DoubleClick += (s, e) => OpenSelectedHistory();

            // Bottom Actions
            FlowLayoutPanel bottom = new FlowLayoutPanel() { Dock = DockStyle.Fill, WrapContents = false };
            btnOpenHistory = CreateButton("🚀 Buka Halaman", Color.FromArgb(14, 116, 144));
            btnOpenHistory.Click += (s, e) => OpenSelectedHistory();

            btnDeleteHistory = CreateButton("🗑️ Hapus Pilihan", Color.FromArgb(71, 85, 105));
            btnDeleteHistory.Click += (s, e) => DeleteSelectedHistory();

            btnClearAllHistory = CreateButton("⚠️ Bersihkan Semua Riwayat", Color.FromArgb(185, 28, 28));
            btnClearAllHistory.Click += (s, e) => ClearAllHistory();

            bottom.Controls.Add(btnOpenHistory);
            bottom.Controls.Add(btnDeleteHistory);
            bottom.Controls.Add(btnClearAllHistory);

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(lvHistory, 0, 1);
            layout.Controls.Add(bottom, 0, 2);

            page.Controls.Add(layout);
            RefreshHistoryList();
        }

        private void RefreshHistoryList() {
            lvHistory.Items.Clear();
            string query = txtSearchHistory.Text.Trim().ToLower();

            for (int i = 0; i < manager.History.Count; i++) {
                var h = manager.History[i];
                if (string.IsNullOrEmpty(query) ||
                    h.Title.ToLower().Contains(query) ||
                    h.Url.ToLower().Contains(query)) {
                    var lvi = new ListViewItem(h.VisitedAt);
                    lvi.SubItems.Add(h.Title);
                    lvi.SubItems.Add(h.Url);
                    lvi.Tag = i;
                    lvHistory.Items.Add(lvi);
                }
            }
        }

        private void OpenSelectedHistory() {
            if (lvHistory.SelectedItems.Count > 0) {
                string url = lvHistory.SelectedItems[0].SubItems[2].Text;
                mainForm.AddNewTab(url, true);
                this.Close();
            }
        }

        private void DeleteSelectedHistory() {
            if (lvHistory.SelectedItems.Count > 0) {
                int index = (int)lvHistory.SelectedItems[0].Tag;
                manager.RemoveHistoryItem(index);
                RefreshHistoryList();
            }
        }

        private void ClearAllHistory() {
            var confirm = MessageBox.Show(this, "Yakin ingin menghapus seluruh riwayat penjelajahan?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes) {
                manager.ClearHistory();
                RefreshHistoryList();
            }
        }

        private void BuildBookmarkTab(TabPage page) {
            TableLayoutPanel layout = new TableLayoutPanel() {
                Dock = DockStyle.Fill,
                RowCount = 3,
                ColumnCount = 1
            };
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));

            // Top Add Bookmark Bar
            FlowLayoutPanel top = new FlowLayoutPanel() { Dock = DockStyle.Fill, WrapContents = false };
            Label lblT = new Label() { Text = "Judul:", AutoSize = true, Padding = new Padding(0, 6, 2, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            txtNewBmTitle = new TextBox() { Width = 180, Font = new Font("Segoe UI", 9.5f), BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White };
            Label lblU = new Label() { Text = "URL:", AutoSize = true, Padding = new Padding(8, 6, 2, 0), ForeColor = Color.FromArgb(148, 163, 184) };
            txtNewBmUrl = new TextBox() { Width = 280, Font = new Font("Segoe UI", 9.5f), BackColor = Color.FromArgb(15, 23, 42), ForeColor = Color.White };

            btnAddBookmark = CreateButton("➕ Simpan", Color.FromArgb(22, 163, 74));
            btnAddBookmark.Click += (s, e) => {
                string t = txtNewBmTitle.Text.Trim();
                string u = txtNewBmUrl.Text.Trim();
                if (!string.IsNullOrEmpty(u)) {
                    manager.AddBookmark(string.IsNullOrEmpty(t) ? u : t, u);
                    txtNewBmTitle.Clear();
                    txtNewBmUrl.Clear();
                    RefreshBookmarkList();
                }
            };

            top.Controls.Add(lblT);
            top.Controls.Add(txtNewBmTitle);
            top.Controls.Add(lblU);
            top.Controls.Add(txtNewBmUrl);
            top.Controls.Add(btnAddBookmark);

            // ListView
            lvBookmarks = new ListView() {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                BackColor = Color.FromArgb(15, 23, 42),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5f)
            };
            lvBookmarks.Columns.Add("Judul Bookmark", 260);
            lvBookmarks.Columns.Add("Alamat URL", 420);
            lvBookmarks.DoubleClick += (s, e) => OpenSelectedBookmark();

            // Bottom Actions
            FlowLayoutPanel bottom = new FlowLayoutPanel() { Dock = DockStyle.Fill, WrapContents = false };
            btnOpenBookmark = CreateButton("🚀 Buka Bookmark", Color.FromArgb(14, 116, 144));
            btnOpenBookmark.Click += (s, e) => OpenSelectedBookmark();

            btnDeleteBookmark = CreateButton("🗑️ Hapus Bookmark", Color.FromArgb(185, 28, 28));
            btnDeleteBookmark.Click += (s, e) => DeleteSelectedBookmark();

            bottom.Controls.Add(btnOpenBookmark);
            bottom.Controls.Add(btnDeleteBookmark);

            layout.Controls.Add(top, 0, 0);
            layout.Controls.Add(lvBookmarks, 0, 1);
            layout.Controls.Add(bottom, 0, 2);

            page.Controls.Add(layout);
            RefreshBookmarkList();
        }

        private void RefreshBookmarkList() {
            lvBookmarks.Items.Clear();
            foreach (var b in manager.Bookmarks) {
                var lvi = new ListViewItem(b.Title);
                lvi.SubItems.Add(b.Url);
                lvBookmarks.Items.Add(lvi);
            }
        }

        private void OpenSelectedBookmark() {
            if (lvBookmarks.SelectedItems.Count > 0) {
                string url = lvBookmarks.SelectedItems[0].SubItems[1].Text;
                mainForm.AddNewTab(url, true);
                this.Close();
            }
        }

        private void DeleteSelectedBookmark() {
            if (lvBookmarks.SelectedItems.Count > 0) {
                string url = lvBookmarks.SelectedItems[0].SubItems[1].Text;
                manager.RemoveBookmark(url);
                RefreshBookmarkList();
            }
        }

        private Button CreateButton(string text, Color bg) {
            Button btn = new Button() {
                Text = text,
                AutoSize = true,
                Height = 30,
                BackColor = bg,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9f),
                Margin = new Padding(3, 0, 4, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}

