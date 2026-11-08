---
name: app-versioning
description: Standar penomoran versi (Semantic Versioning) dan portabilitas skill lokal untuk setiap aplikasi yang dibuat dan dikembangkan. Gunakan skill ini setiap kali membuat aplikasi baru atau melakukan perubahan kode untuk memastikan nomor versi selalu dinaikkan dan skill terpasang di direktori lokal proyek (.agents/skills/app-versioning/SKILL.md).
---

# Standar Penomoran Versi & Portabilitas Skill Lokal

Standar ini mengatur dua hal fundamental:
1. **Setiap aplikasi WAJIB memiliki versi** dan **WAJIB dinaikkan versinya** setiap kali ada perubahan/modifikasi.
2. **Skill versioning ini WAJIB disalin ke skill lokal aplikasi** di folder `.agents/skills/app-versioning/SKILL.md` agar saat proyek dipindahkan ke komputer lain, aturan ini tetap ada dan melekat permanen di dalam repositori.

---

## 1. Format Penomoran Versi (Semantic Versioning - SemVer)

Gunakan format standar: **`vMAJOR.MINOR.PATCH`** (contoh: `v1.0.0`, `v1.0.1`, `v1.1.0`, `v2.0.0`):

1. **PATCH (x.y.Z)**: Naikkan saat ada perbaikan bug kecil, refactoring, optimasi performa, atau tweak UI (contoh: `1.0.0` -> `1.0.1`).
2. **MINOR (x.Y.0)**: Naikkan saat ada penambahan fitur baru yang tidak merusak fungsi yang sudah ada (contoh: `1.0.1` -> `1.1.0`).
3. **MAJOR (X.0.0)**: Naikkan saat ada perombakan arsitektur besar, desain fundamental, atau breaking changes (contoh: `1.1.0` -> `2.0.0`).

---

## 2. Replikasi ke Skill Lokal Proyek (.agents/skills/)

Setiap kali membuat atau memodifikasi sebuah aplikasi di workspace:
* Buat direktori: `.agents/skills/app-versioning/` di root proyek.
* Simpan file `SKILL.md` ini di dalamnya.
* Commit file `.agents/` ke version control (Git).
* Dengan demikian, ketika proyek di-clone di PC lain atau dikerjakan oleh developer/agent lain di mesin berbeda, panduan versioning ini langsung otomatis aktif tanpa perlu setup ulang.

---

## 3. Penempatan Nomor Versi di Aplikasi

* **UI Antarmuka**: Titlebar jendela (`AppName v1.1.0`), Status bar, atau About dialog.
* **Metadata Kode**:
  * C# / .NET: `public const string APP_VERSION = "v1.1.0";` dan assembly version.
  * Android: `versionCode` dan `versionName "1.1.0"` di `app/build.gradle`.
  * Node.js / Web: `"version": "1.1.0"` di `package.json`.
  * Python: `__version__ = "1.1.0"`.
* **Dokumentasi**: Badge versi dan changelog di `README.md`.

