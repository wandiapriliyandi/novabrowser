using System;
using System.Text.RegularExpressions;

namespace NovaBrowser {
    public static class OmniboxHelper {
        public static string ResolveUrl(string input) {
            if (string.IsNullOrWhiteSpace(input)) {
                return "about:blank";
            }

            input = input.Trim();

            // Custom local schemas
            if (input.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("file:///", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("edge://", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("chrome://", StringComparison.OrdinalIgnoreCase) ||
                input.StartsWith("about:", StringComparison.OrdinalIgnoreCase)) {
                return input;
            }

            // Localhost / IP address
            if (Regex.IsMatch(input, @"^(localhost|127\.0\.0\.1|0\.0\.0\.0)(:\d+)?(/.*)?$", RegexOptions.IgnoreCase)) {
                return "http://" + input;
            }

            // LAN IP addresses
            if (Regex.IsMatch(input, @"^(192\.168\.\d+\.\d+|10\.\d+\.\d+\.\d+|172\.(1[6-9]|2\d|3[0-1])\.\d+\.\d+)(:\d+)?(/.*)?$")) {
                return "http://" + input;
            }

            // Domain with port or valid TLD
            if (Regex.IsMatch(input, @"^[a-zA-Z0-9\-\.]+\.[a-zA-Z]{2,}(:\d+)?(/.*)?$")) {
                return "https://" + input;
            }

            // Otherwise, treat as search query
            return "https://www.google.com/search?q=" + Uri.EscapeDataString(input);
        }

        public static string GetSecurityBadge(string url) {
            if (string.IsNullOrEmpty(url)) return "⚪ Blank";
            if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) return "🔒 HTTPS";
            if (url.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("http://127.0.0.1", StringComparison.OrdinalIgnoreCase)) return "⚡ Localhost";
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)) return "⚠️ HTTP (Tidak Aman)";
            return "📄 Dokumen";
        }
    }
}

