using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace NovaBrowser {
    public class BookmarkItem {
        public string Title { get; set; }
        public string Url { get; set; }
        public string DateAdded { get; set; }

        public BookmarkItem() { }
        public BookmarkItem(string title, string url) {
            this.Title = string.IsNullOrWhiteSpace(title) ? url : title;
            this.Url = url;
            this.DateAdded = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
    }

    public class HistoryItem {
        public string Title { get; set; }
        public string Url { get; set; }
        public string VisitedAt { get; set; }

        public HistoryItem() { }
        public HistoryItem(string title, string url) {
            this.Title = string.IsNullOrWhiteSpace(title) ? url : title;
            this.Url = url;
            this.VisitedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    public class BookmarkHistoryManager {
        private string profileDir;
        private string bookmarkFile;
        private string historyFile;

        public List<BookmarkItem> Bookmarks { get; private set; }
        public List<HistoryItem> History { get; private set; }

        public BookmarkHistoryManager(string profilePath) {
            this.profileDir = profilePath;
            this.bookmarkFile = Path.Combine(profileDir, "bookmarks.txt");
            this.historyFile = Path.Combine(profileDir, "history.txt");

            this.Bookmarks = new List<BookmarkItem>();
            this.History = new List<HistoryItem>();

            LoadBookmarks();
            LoadHistory();
        }

        public void AddBookmark(string title, string url) {
            if (string.IsNullOrWhiteSpace(url) || url.StartsWith("about:", StringComparison.OrdinalIgnoreCase)) return;

            // Jangan duplikasi URL yang persis sama
            Bookmarks.RemoveAll(b => b.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
            Bookmarks.Insert(0, new BookmarkItem(title, url));
            SaveBookmarks();
        }

        public void RemoveBookmark(string url) {
            Bookmarks.RemoveAll(b => b.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
            SaveBookmarks();
        }

        public bool IsBookmarked(string url) {
            return Bookmarks.Exists(b => b.Url.Equals(url, StringComparison.OrdinalIgnoreCase));
        }

        public void AddHistory(string title, string url) {
            if (string.IsNullOrWhiteSpace(url) ||
                url.StartsWith("about:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("novalite://", StringComparison.OrdinalIgnoreCase)) return;

            // Batasi riwayat terakhir (misal maks 500 entri agar tetap ultra-ringan)
            History.Insert(0, new HistoryItem(title, url));
            if (History.Count > 500) {
                History.RemoveRange(500, History.Count - 500);
            }
            SaveHistory();
        }

        public void ClearHistory() {
            History.Clear();
            SaveHistory();
        }

        public void RemoveHistoryItem(int index) {
            if (index >= 0 && index < History.Count) {
                History.RemoveAt(index);
                SaveHistory();
            }
        }

        private void LoadBookmarks() {
            Bookmarks.Clear();
            if (File.Exists(bookmarkFile)) {
                try {
                    string[] lines = File.ReadAllLines(bookmarkFile, Encoding.UTF8);
                    foreach (string line in lines) {
                        string[] parts = line.Split(new char[] { '|' }, 3);
                        if (parts.Length >= 2) {
                            Bookmarks.Add(new BookmarkItem(parts[0], parts[1]));
                        }
                    }
                } catch { }
            } else {
                // Default Developer Bookmarks
                Bookmarks.Add(new BookmarkItem("XAMPP Localhost", "http://localhost:80"));
                Bookmarks.Add(new BookmarkItem("React / Next.js", "http://localhost:3000"));
                Bookmarks.Add(new BookmarkItem("Vite Dev Server", "http://localhost:5173"));
                Bookmarks.Add(new BookmarkItem("Laravel / FastAPI", "http://localhost:8000"));
                Bookmarks.Add(new BookmarkItem("GitHub", "https://github.com"));
                Bookmarks.Add(new BookmarkItem("MDN Web Docs", "https://developer.mozilla.org"));
                SaveBookmarks();
            }
        }

        private void SaveBookmarks() {
            try {
                var sb = new StringBuilder();
                foreach (var b in Bookmarks) {
                    sb.AppendLine(string.Format("{0}|{1}|{2}", b.Title.Replace("|", " "), b.Url, b.DateAdded));
                }
                File.WriteAllText(bookmarkFile, sb.ToString(), Encoding.UTF8);
            } catch { }
        }

        private void LoadHistory() {
            History.Clear();
            if (File.Exists(historyFile)) {
                try {
                    string[] lines = File.ReadAllLines(historyFile, Encoding.UTF8);
                    foreach (string line in lines) {
                        string[] parts = line.Split(new char[] { '|' }, 3);
                        if (parts.Length >= 2) {
                            var item = new HistoryItem(parts[0], parts[1]);
                            if (parts.Length >= 3) item.VisitedAt = parts[2];
                            History.Add(item);
                        }
                    }
                } catch { }
            }
        }

        private void SaveHistory() {
            try {
                var sb = new StringBuilder();
                foreach (var h in History) {
                    sb.AppendLine(string.Format("{0}|{1}|{2}", h.Title.Replace("|", " "), h.Url, h.VisitedAt));
                }
                File.WriteAllText(historyFile, sb.ToString(), Encoding.UTF8);
            } catch { }
        }
    }
}

