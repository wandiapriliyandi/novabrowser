using System;
using System.IO;

namespace NovaBrowser {
    public static class Homepage {
        public static string GetHtml() {
            return @"<!DOCTYPE html>
<html lang=""id"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>NovaLite - Developer Browser</title>
    <style>
        :root {
            --bg-color: #0f172a;
            --surface-color: #1e293b;
            --surface-hover: #334155;
            --border-color: #334155;
            --accent-color: #38bdf8;
            --accent-gradient: linear-gradient(135deg, #38bdf8, #818cf8);
            --text-main: #f8fafc;
            --text-muted: #94a3b8;
            --card-shadow: 0 10px 25px -5px rgba(0, 0, 0, 0.4);
        }
        * { box-sizing: border-box; margin: 0; padding: 0; }
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
            background: radial-gradient(circle at 50% 0%, #1e1b4b 0%, #0f172a 75%);
            color: var(--text-main);
            min-height: 100vh;
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            padding: 30px 20px;
        }
        .container {
            max-width: 860px;
            width: 100%;
            text-align: center;
        }
        .badge {
            display: inline-block;
            background: rgba(56, 189, 248, 0.15);
            color: #38bdf8;
            padding: 6px 14px;
            border-radius: 9999px;
            font-size: 13px;
            font-weight: 600;
            letter-spacing: 0.5px;
            margin-bottom: 18px;
            border: 1px solid rgba(56, 189, 248, 0.3);
        }
        h1 {
            font-size: 44px;
            font-weight: 800;
            letter-spacing: -0.5px;
            background: var(--accent-gradient);
            -webkit-background-clip: text;
            -webkit-text-fill-color: transparent;
            margin-bottom: 8px;
        }
        p.subtitle {
            font-size: 16px;
            color: var(--text-muted);
            margin-bottom: 32px;
        }
        .search-box {
            position: relative;
            max-width: 650px;
            margin: 0 auto 36px auto;
        }
        .search-box input {
            width: 100%;
            padding: 16px 22px 16px 52px;
            font-size: 16px;
            border-radius: 14px;
            border: 1px solid var(--border-color);
            background: rgba(30, 41, 59, 0.85);
            color: #fff;
            outline: none;
            box-shadow: var(--card-shadow);
            backdrop-filter: blur(8px);
            transition: all 0.2s ease;
        }
        .search-box input:focus {
            border-color: #38bdf8;
            box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.25);
            background: #1e293b;
        }
        .search-icon {
            position: absolute;
            left: 20px;
            top: 50%;
            transform: translateY(-50%);
            color: var(--text-muted);
            font-size: 18px;
        }
        .section-title {
            font-size: 13px;
            text-transform: uppercase;
            letter-spacing: 1px;
            color: var(--text-muted);
            margin-bottom: 16px;
            text-align: left;
        }
        .grid {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(130px, 1fr));
            gap: 12px;
            margin-bottom: 32px;
        }
        .card {
            background: var(--surface-color);
            border: 1px solid var(--border-color);
            border-radius: 12px;
            padding: 14px 10px;
            text-decoration: none;
            color: var(--text-main);
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            gap: 8px;
            transition: all 0.2s ease;
        }
        .card:hover {
            transform: translateY(-2px);
            background: var(--surface-hover);
            border-color: #38bdf8;
            box-shadow: 0 6px 15px rgba(0,0,0,0.3);
        }
        .card .icon {
            font-size: 24px;
        }
        .card .name {
            font-size: 13px;
            font-weight: 600;
        }
        .card .port {
            font-size: 11px;
            color: var(--accent-color);
            font-family: Consolas, monospace;
        }
        .footer-tools {
            display: flex;
            flex-wrap: wrap;
            justify-content: center;
            gap: 16px;
            font-size: 13px;
            color: var(--text-muted);
            padding-top: 16px;
            border-top: 1px solid var(--border-color);
        }
        .kbd {
            background: #334155;
            color: #cbd5e1;
            padding: 2px 7px;
            border-radius: 5px;
            font-family: Consolas, monospace;
            font-size: 12px;
            border-bottom: 2px solid #1e293b;
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""badge"">⚡ ULTRA LIGHTWEIGHT &bull; PORTABLE &bull; DEV EDITION</div>
        <h1>NovaLite Browser</h1>
        <p class=""subtitle"">Peramban Cepat Khusus Pengembang Web, Ethical Hacking &amp; Multi-Profil</p>

        <form class=""search-box"" action=""https://www.google.com/search"" method=""GET"">
            <span class=""search-icon"">&#128269;</span>
            <input type=""text"" name=""q"" placeholder=""Ketik URL (misal: localhost:3000) atau cari di Google..."" autofocus>
        </form>

        <div class=""section-title"">Localhost &amp; Port Developer</div>
        <div class=""grid"">
            <a href=""http://localhost:80"" class=""card"">
                <span class=""icon"">&#128421;</span>
                <span class=""name"">Apache / XAMPP</span>
                <span class=""port"">:80</span>
            </a>
            <a href=""http://localhost:3000"" class=""card"">
                <span class=""icon"">&#9883;</span>
                <span class=""name"">React / Next.js</span>
                <span class=""port"">:3000</span>
            </a>
            <a href=""http://localhost:5173"" class=""card"">
                <span class=""icon"">&#9889;</span>
                <span class=""name"">Vite Server</span>
                <span class=""port"">:5173</span>
            </a>
            <a href=""http://localhost:8000"" class=""card"">
                <span class=""icon"">&#128736;</span>
                <span class=""name"">Laravel / FastAPI</span>
                <span class=""port"">:8000</span>
            </a>
            <a href=""http://localhost:8080"" class=""card"">
                <span class=""icon"">&#9749;</span>
                <span class=""name"">Vue / Spring</span>
                <span class=""port"">:8080</span>
            </a>
            <a href=""https://github.com"" class=""card"">
                <span class=""icon"">&#128025;</span>
                <span class=""name"">GitHub</span>
                <span class=""port"">github.com</span>
            </a>
        </div>

        <div class=""footer-tools"">
            <span><span class=""kbd"">F12</span> DevTools</span>
            <span><span class=""kbd"">Ctrl + T</span> Tab Baru</span>
            <span><span class=""kbd"">Ctrl + W</span> Tutup Tab</span>
            <span><span class=""kbd"">Ctrl + Shift + R</span> Hard Refresh</span>
            <span><span class=""kbd"">🛡️ Pentest</span> Burp Suite &amp; Security Tools</span>
        </div>
    </div>
</body>
</html>";
        }

        public static string SaveHomepageFile() {
            try {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "homepage.html");
                File.WriteAllText(path, GetHtml(), System.Text.Encoding.UTF8);
                return path;
            } catch {
                return string.Empty;
            }
        }
    }
}

