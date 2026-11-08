using System;
using System.IO;

namespace NovaBrowser {
    public class ProfileManager {
        public static string AppRoot {
            get {
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        public static string ProfilesDirectory {
            get {
                string dir = Path.Combine(AppRoot, "profiles");
                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

        public string CurrentProfile { get; set; }
        public string ProxyServer { get; set; }
        public bool DisableWebSecurity { get; set; }
        public bool UseDoH { get; set; } // DNS over HTTPS

        public ProfileManager(string profileName = "Default") {
            this.CurrentProfile = string.IsNullOrWhiteSpace(profileName) ? "Default" : profileName;
            this.ProxyServer = string.Empty;
            this.DisableWebSecurity = false;
            this.UseDoH = false;
        }

        public string GetUserDataFolder() {
            string profilePath = Path.Combine(ProfilesDirectory, this.CurrentProfile);
            if (!Directory.Exists(profilePath)) {
                Directory.CreateDirectory(profilePath);
            }
            return profilePath;
        }

        public string BuildCommandLineArguments() {
            var args = new System.Text.StringBuilder();

            // Disable WebRTC leak if proxy is set or pentest mode
            if (!string.IsNullOrEmpty(this.ProxyServer)) {
                args.Append(" --proxy-server=\"" + this.ProxyServer + "\"");
            }

            // Disable web security (CORS/CSP bypass for API testing)
            if (this.DisableWebSecurity) {
                args.Append(" --disable-web-security --allow-running-insecure-content --ignore-certificate-errors");
            }

            // DNS Over HTTPS (Cloudflare 1.1.1.1 Anti-Blokir)
            if (this.UseDoH) {
                args.Append(" --enable-features=DnsOverHttps --dns-over-https-templates=\"https://cloudflare-dns.com/dns-query{?dns}\"");
            }

            // Opsi Hemat Memori yang 100% Stabil (Aman dari GPU crash & restart loop)
            args.Append(" --disable-background-networking");
            args.Append(" --disable-background-timer-throttling");
            args.Append(" --disable-backgrounding-occluded-windows");
            args.Append(" --disable-component-update");
            args.Append(" --disable-default-apps");
            args.Append(" --disable-domain-reliability");
            args.Append(" --disable-sync");
            args.Append(" --disable-speech-api");
            args.Append(" --disable-features=Translate,OptimizationHints,MediaRouter");
            args.Append(" --renderer-process-limit=2");

            return args.ToString().Trim();
        }

        public static string[] GetAvailableProfiles() {
            try {
                var dirs = Directory.GetDirectories(ProfilesDirectory);
                var list = new string[dirs.Length];
                for (int i = 0; i < dirs.Length; i++) {
                    list[i] = Path.GetFileName(dirs[i]);
                }
                if (list.Length == 0) {
                    return new string[] { "Default" };
                }
                return list;
            } catch {
                return new string[] { "Default" };
            }
        }
    }
}

