# 📱 NovaLite Browser - Versi Android (APK)

> **Browser Android Ultra-Ringan (~2 MB) dengan Mode Developer Terintegrasi (Eruda Mobile Console, Remote Inspect, Desktop Mode & Simpan Sandi).**

---

## 🌟 Fitur Unggulan Versi Android

1. **Ultra Ringan (~2 MB APK)**:
   * Menggunakan engine native **Android System WebView** (Chromium) yang sudah terpasang di semua HP Android (Android 5.0 s/d 14/15).
   * Tidak membebani memori HP (RAM hanya ~30 MB).
2. **In-App Mobile DevTools (`🛠️`)**:
   * Di browser HP biasa, Anda tidak bisa melakukan *Inspect Element* atau membuka *Console*.
   * Di NovaLite Android, cukup klik tombol **`🛠️`** di toolbar atas untuk membuka **Mobile DevTools (Eruda)** langsung di layar HP Anda:
     * **Elements**: Inspeksi & edit pohon DOM HTML dan styling CSS secara live di HP.
     * **Console**: Melihat pesan log, error, warning, dan mengeksekusi script JavaScript interaktif.
     * **Network**: Memantau request API Fetch/XHR, payload, status code (200, 404, 500).
     * **Storage**: Mengintip Cookies, LocalStorage, dan SessionStorage situs.
3. **Remote USB Debugging**:
   * Terhubung langsung dengan Google Chrome di PC/Laptop via kabel USB di alamat `chrome://inspect`.
4. **Desktop Mode Toggle (`💻` / `📱`)**:
   * 1-klik untuk beralih antara tampilan HP (Mobile) dan tampilan Komputer (Desktop User-Agent).
5. **Simpan Sandi & Autofill**:
   * Menyimpan kata sandi dan mengisi formulir login secara otomatis di perangkat Android Anda.

---

## 🛠️ Cara Membuat File APK (Build APK)

### Cara 1: Menggunakan Android Studio (Paling Mudah)
1. Buka aplikasi **Android Studio** di komputer Anda.
2. Pilih menu **File** -> **Open**, lalu arahkan ke folder ini:
   `c:\xampp\htdocs\browser\android`
3. Tunggu sinkronisasi selesai, lalu klik menu:
   **Build** -> **Build Bundle(s) / APK(s)** -> **Build APK(s)**.
4. File APK siap dipasang (`.apk`) akan muncul di folder:
   `android/app/build/outputs/apk/debug/app-debug.apk`.
5. Salin file `.apk` tersebut ke HP Android Anda dan pasang (*Install*).

### Cara 2: Menggunakan Terminal / Command Line
Jika Anda memiliki Android SDK di komputer Anda:
```bash
cd android
gradlew assembleDebug
```
File APK akan otomatis ter-generate di folder output.

---

## 📁 Struktur Folder Proyek Android

```
android/
├── build.gradle
├── settings.gradle
└── app/
    ├── build.gradle
    └── src/main/
        ├── AndroidManifest.xml
        ├── java/com/novalite/browser/MainActivity.java
        └── res/
            ├── layout/activity_main.xml
            └── values/strings.xml, colors.xml
```

