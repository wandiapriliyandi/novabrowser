# 📱 Panduan Mengunduh File APK NovaLite Browser via GitHub Actions

Dengan konfigurasi otomatis yang telah disiapkan di folder [`.github/workflows/build-apk.yml`](file:///.github/workflows/build-apk.yml), Anda dapat mengompilasi dan mendapatkan file **`.apk` Android** secara **gratis, otomatis, dan tanpa membebani laptop Anda**.

---

## 🚀 Langkah 1: Buat Repository Baru di GitHub
1. Buka [github.com](https://github.com) dan login ke akun Anda.
2. Klik tombol **`New`** (atau `+` di kanan atas) untuk membuat repository baru:
   * Nama Repository: misal `novalite-browser`
   * Pilih **Public** atau **Private** (keduanya gratis didukung GitHub Actions).
   * Klik **Create repository**.

---

## 📤 Langkah 2: Upload / Push Folder Ini ke GitHub
Buka terminal (CMD / PowerShell) di folder `c:\xampp\htdocs\browser`, lalu jalankan:

```cmd
git init
git add .
git commit -m "Initial NovaLite Browser"
git branch -M main
git remote add origin https://github.com/USERNAME_ANDA/novalite-browser.git
git push -u origin main
```
*(Ganti `USERNAME_ANDA` dengan username akun GitHub Anda).*

> **Atau Tanpa Git Terminal**:
> Anda juga bisa langsung drag-and-drop / upload folder proyek ini melalui web GitHub di tombol **Add file -> Upload files**.

---

## ⚡ Langkah 3: Server GitHub Otomatis Mengompilasi APK (~1 Menit)
1. Setelah Anda meng-upload kode, buka halaman repository GitHub Anda.
2. Klik tab **`Actions`** di menu atas.
3. Anda akan melihat proses workflow bernama **`Build Android APK (NovaLite)`** sedang berjalan.
4. Tunggu sekitar 1 sampai 2 menit hingga muncul ikon centang hijau **(Success)**.

---

## 📥 Langkah 4: Download File APK ke HP Android
1. Klik pada proses workflow yang sudah sukses centang hijau tersebut.
2. Gulir ke bagian paling bawah ke bagian **Artifacts**.
3. Klik tombol **`NovaLite-Browser-APK`**.
4. File zip berisi **`app-debug.apk`** akan langsung terunduh!
5. Kirim file `.apk` tersebut ke HP Android Anda (lewat WhatsApp, kabel USB, atau buka langsung GitHub Actions dari browser HP Anda).
6. Buka file `.apk` di HP Anda dan pilih **Pasang / Install**. Selesai! 🎉

