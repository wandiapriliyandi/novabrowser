# ⚡ NovaLite Browser `v1.2.1` (Portable & Developer Edition)

> **Peramban Desktop Ultra-Ringan, 100% Portabel (Zero-Install), Berbasis Windows Native WebView2 (Chromium Core), dengan Dukungan Penuh Mode Developer, Alat Pentest/Keamanan, Simpan Sandi, dan Manajemen Multi-Profil.**

[![Version](https://img.shields.io/badge/version-1.2.1-blue.svg)](README.md)
[![Platform](https://img.shields.io/badge/platform-Windows%20%7C%20Android-green.svg)](README.md)
[![License](https://img.shields.io/badge/license-MIT-purple.svg)](README.md)

---

## 📝 Catatan Rilis (Changelog)

### Versi `v1.2.1` (Rilis Saat Ini)
* **Fitur Tombol Cepat Hapus URL (Clear Omnibox '✕')**: Saat form URL diklik atau terdapat teks, muncul tombol `✕` di ujung kanan kolom alamat untuk menghapus URL seketika dengan 1 klik tanpa perlu menekan tombol delete atau backspace.
* **Auto-Focus & Smart Visibility**: Tombol `✕` otomatis muncul saat ada teks URL dan otomatis sembunyi saat URL kosong, serta kursor langsung siap mengetik setelah diklik.

### Versi `v1.2.0`
* **Fitur Utama: Sistem Update Otomatis (In-App Auto-Updater)**: Browser dapat mendeteksi versi terbaru secara senyap saat startup dan memperbarui file executable portabel dalam 1 klik langsung dari dalam aplikasi tanpa perlu repot membuka GitHub atau download ulang manual.
* **Tombol Cek Pembaruan Manual**: Tersedia tombol `[🔄 Cek Update]` di bilah status bawah untuk memeriksa rilis baru kapan saja.

### Versi `v1.1.2`
* **Optimasi Efisiensi Baterai (Android Lifecycle)**: Menambahkan `pauseTimers()` dan `onPause()` otomatis saat layar mati atau aplikasi di-minimize untuk membekukan proses JS latar belakang (0% CPU background).
* **Update Versi SemVer**: Otomatis dinaikkan ke `v1.1.2` sesuai skill lokal `.agents/skills/app-versioning/`.

### Versi `v1.1.1`
* **Perbaikan Build Android CI**: Memperbaiki workflow GitHub Actions untuk build APK menggunakan Android SDK bawaan runner Ubuntu dan `gradle/actions/setup-gradle@v4` yang stabil.

### Versi `v1.1.0`
* **Skill Portabel Melekat**: Menyertakan `.agents/skills/app-versioning/` agar tata kelola versi tetap permanen saat proyek di-clone/pindah ke komputer lain.
* **Fitur Pasang Halaman Web (Install Page / PWA)**: Tombol `[📦 Install]` untuk memasang website apapun ke Desktop sebagai aplikasi mandiri.
* **Omnibox Autocomplete**: Saran otomatis instan dari riwayat penjelajahan, bookmark, dan port developer (`localhost:3000`, `vite:5173`, dll).
* **Bookmark & History Manager**: Pengelola bookmark dan riwayat penjelajahan portabel (`Ctrl+D`, `Ctrl+H`).
* **Mode Layar Penuh**: Otomatis maximized saat dibuka dan toggle Full Screen Kiosk via `F11`.
* **Desain Ikon Baru**: Ikon neon orbital futuristik tertanam langsung di executable `.exe` dan shortcut desktop.
* **Stabilitas Tinggi**: Menghilangkan siklus restart GPU watchdog saat startup awal dengan shared singleton environment.
* **Dukungan Android APK**: Proyek native Android lengkap dan otomatisasi build APK via GitHub Actions.

---

## 🚀 Keunggulan Utama

* **Ultra Ringan**: Ukuran file `NovaBrowser.exe` hanya **~71 KB** dengan konsumsi RAM idle hanya **~39 MB** (2000x lebih kecil dari Electron).
* **100% Portabel & Tanpa Install (Zero-Install)**: Tidak perlu installer (setup.exe/msi), tidak memerlukan hak Administrator, dan tidak mengotori registry Windows. Cukup simpan di folder manapun atau flashdisk dan klik ganda `NovaBrowser.exe`.
* **Engine Chromium Modern**: Menggunakan Microsoft Edge WebView2 Evergreen bawaan Windows 10 & 11 yang selalu terupdate, aman, dan mendukung akselerasi GPU.
* **Simpan Sandi Otomatis (Password Manager & Autofill)**: Menyimpan sandi login secara aman dan mengisi otomatis (autofill).
* **Multi-Profil Portabel**: Semua riwayat, cookies, sesi login, dan sandi tersimpan rapi di dalam subdirektori `./profiles/` di folder aplikasi.
* **Pasang Halaman Web sebagai Aplikasi (Install Page / PWA)**: Pasang website apapun (misal YouTube, WhatsApp Web, ChatGPT, atau Dashboard Localhost) langsung ke Desktop Windows sebagai aplikasi mandiri (*standalone web app window*).

---

## 🛠️ Fitur Mode Developer (Dev Mode)

1. **Chromium DevTools Lengkap (`F12` / Inspect Element)**:
   * Tekan tombol `F12` atau klik kanan halaman lalu pilih **Inspect / Periksa Elemen**.
   * Panel lengkap: Elements (HTML/CSS live editor), Console JS, Network Waterfall & HTTP Header Inspector, Sources Debugger, Application Storage (Cookies, LocalStorage, IndexedDB), Memory & Performance Profiler, dan Lighthouse Audit.
2. **Quick JS Console Runner**:
   * Jalankan ekspresi JavaScript langsung dari toolbar ke halaman aktif tanpa harus membuka jendela DevTools besar.
3. **Responsive Viewport Simulator**:
   * Uji tampilan responsif web dalam 1 klik:
     * Mobile: iPhone 14/15 (393 x 852)
     * Mobile: Android Pixel (412 x 915)
     * Tablet: iPad Mini/Air (768 x 1024)
     * Laptop: HD (1366 x 768)
     * Desktop: Full HD (1920 x 1080)
   * Tombol **Putar / Rotate** untuk simulasi orientasi Landscape vs Portrait.
4. **User-Agent Switcher**:
   * Ganti identitas peramban: Desktop Chrome, iPhone Safari iOS, Android Chrome, Googlebot Crawler.
5. **One-Click Cache & Storage Controller**:
   * **Hard Reload** (`Ctrl+Shift+R`): Muat ulang halaman dengan mengabaikan cache.
   * **Hapus Cookies**: Hapus semua cookie sesi domain aktif.
   * **Hapus Storage**: Bersihkan LocalStorage dan SessionStorage dalam 1 klik.
   * **View Source**: Buka kode sumber mentah HTML halaman aktif.

---

## 🛡️ Fitur Security & Ethical Hacking (Pentest Mode)

Klik tombol **`[🛡️ Pentest]`** di bar navigasi untuk mengaktifkan:
1. **1-Klik Burp Suite / OWASP ZAP Proxy**:
   * Mengarahkan koneksi browser langsung ke proxy lokal `127.0.0.1:8080` hanya untuk browser ini saja, tanpa mengubah pengaturan proxy sistem Windows.
2. **Tor / SOCKS5 Proxy Mode**:
   * Mengarahkan lalu lintas ke port Tor `127.0.0.1:9050` untuk pengujian anonimitas.
3. **Bypass CORS / Web Security Toggle**:
   * Opsi untuk menonaktifkan Same-Origin Policy dan CSP saat menguji integrasi REST API lintas domain.
4. **Security & Pentest Toolkit Pop-up**:
   * **Encoder / Decoder**: URL, Base64, Hex, HTML Entities, Hash MD5/SHA256, dan JWT Token Decoder (Header & Payload Claims).
   * **Vulnerability Payload Cheat Sheet**: Daftar payload pengujian untuk XSS, SQL Injection, Path Traversal / LFI, SSTI, dan Open Redirect dengan tombol salin cepat.
   * **Security Headers Inspector**: Analisis instan keamanan header website (`CSP`, `HSTS`, `X-Frame-Options`, `X-Content-Type-Options`, dll).

---

## ⌨️ Pintasan Keyboard (Shortcuts)

| Pintasan | Fungsi |
| :--- | :--- |
| **`F12`** | Buka Chromium DevTools (Inspect, Console, Network) |
| **`F11`** | Mode Layar Penuh (Toggle Full Screen) |
| **`Ctrl + T`** | Buka Tab Baru |
| **`Ctrl + W`** | Tutup Tab Aktif |
| **`Ctrl + D`** | Tambahkan Halaman Aktif ke Bookmark |
| **`Ctrl + H`** | Buka Jendela Riwayat Penjelajahan (History) & Bookmark |
| **`Ctrl + R`** atau **`F5`** | Muat Ulang Halaman (Reload) |
| **`Ctrl + Shift + R`** | Muat Ulang Keras / Bypass Cache (Hard Reload) |
| **`Ctrl + L`** atau **`Alt + D`** | Fokus ke Address Bar / Omnibox |
| **`Alt + Left`** | Kembali ke Halaman Sebelumnya (Back) |
| **`Alt + Right`** | Maju ke Halaman Berikutnya (Forward) |
| **`Alt + Home`** | Buka Beranda Pengembang (Developer Homepage) |

---

## 📁 Struktur Folder Aplikasi

```
c:\xampp\htdocs\browser\
├── NovaBrowser.exe                    <-- File eksekusi utama (Klik 2x untuk membuka)
├── NovaBrowser.exe.config             <-- Konfigurasi runtime portabel (.NET 4.8)
├── build.bat                          <-- Skrip build / compile otomatis menggunakan csc.exe
├── run.bat                            <-- Skrip peluncur cepat
├── homepage.html                      <-- Beranda dark mode offline
├── README.md                          <-- Dokumentasi lengkap ini
├── profiles/                          <-- Data profil portabel (Cookies, Sandi, Riwayat)
│   ├── Default/                       <-- Profil Utama
│   ├── Work/                          <-- Profil Kerja
│   └── Pentest/                       <-- Profil Keamanan
├── lib/                               <-- Assembly Microsoft Edge WebView2
│   ├── Microsoft.Web.WebView2.WinForms.dll
│   ├── Microsoft.Web.WebView2.Core.dll
│   └── runtimes/ (win-x64 & win-x86)
└── src/                               <-- Kode sumber C# terstruktur
    ├── Program.cs
    ├── MainForm.cs
    ├── BrowserTab.cs
    ├── DevToolbar.cs
    ├── PentestToolbar.cs
    ├── SecurityToolsForm.cs
    ├── ProfileManager.cs
    ├── OmniboxHelper.cs
    └── Homepage.cs
```

---

## 🔨 Cara Membangun Ulang (Recompile)

Jika Anda memodifikasi kode sumber di folder `src/`, cukup jalankan:
```cmd
build.bat
```
Skrip ini otomatis memanggil compiler C# bawaan Windows (`csc.exe`) tanpa perlu menginstal Visual Studio atau SDK tambahan.

