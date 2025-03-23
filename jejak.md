# Jejak Perubahan Proyek Parking System

## Tanggal: 22 Maret 2025

### ParkingOut (Aplikasi Klien)

1. **Perbaikan Koneksi Database**
   - Ditambahkan metode `IsPostgreSqlInstalled()` untuk verifikasi instalasi PostgreSQL
   - Ditingkatkan penanganan error pada metode `InitializeConnectionString()`
   - Ditambahkan logging yang lebih detil untuk memudahkan debugging
   - Perbaikan pada pengecekan tabel schema untuk memastikan struktur database valid

2. **Penanganan Error**
   - Ditingkatkan pesan error yang lebih informatif pada `TestConnection()`
   - Ditambahkan pengecekan error pada pembuatan database
   - Implementasi try-catch yang lebih baik di berbagai metode koneksi database

3. **Package dan Dependensi**
   - Ditambahkan referensi package Npgsql untuk koneksi PostgreSQL
   - Ditambahkan referensi package ClosedXML untuk fungsionalitas laporan
   - Memastikan semua project memiliki dependensi yang tepat

4. **File dan Kode**
   - Diperbarui `BackupRestoreForm.cs` dan `LoginForm.cs` untuk menggunakan Npgsql
   - Diperbarui schema PostgreSQL untuk menangani pembuatan database dengan benar
   - Ditambahkan metode kompatibilitas untuk mendukung kode lama

### ParkingServer (Aplikasi Server)

1. **Perbaikan Koneksi Database**
   - Ditambahkan metode `IsPostgreSqlInstalled()` untuk verifikasi instalasi PostgreSQL
   - Ditambahkan metode `TestDatabaseConnection()` untuk pengujian koneksi
   - Ditambahkan metode `CreateParkirDatabase()` untuk pembuatan database otomatis
   - Implementasi indikator status database dengan property `IsDatabaseAvailable`

2. **WebSocket Server**
   - Ditambahkan metode `InitializeDatabase()` untuk inisialisasi koneksi database
   - Diperbarui alur kerja server untuk meneruskan operasi meskipun tanpa database
   - Ditingkatkan logging untuk memudahkan pemantauan server

3. **Penanganan Error**
   - Pesan error yang lebih informatif dan user-friendly
   - Server dapat beradaptasi saat database tidak tersedia dengan membatasi fungsionalitas
   - Penanganan kasus PostgreSQL tidak terinstal atau tidak berjalan

4. **Performa dan Stabilitas**
   - Ditingkatkan penanganan koneksi WebSocket untuk stabilitas lebih baik
   - Ditambahkan logging status koneksi dan jumlah client aktif
   - Implementasi pengelolaan siklus hidup server yang lebih baik

## Catatan Tambahan

- Kedua aplikasi sekarang dapat berjalan dengan PostgreSQL dan telah diuji pada sistem Windows
- Database parkirdb dibuat secara otomatis jika belum ada
- Schema database otomatis dimuat saat pembuatan database baru
- Semua koneksi database menggunakan kredensial default: username=postgres, password=root@rsi
- Server WebSocket berjalan pada port 8181 

## Tanggal: 27 Maret 2025

### Perbaikan Lanjutan ParkingOut (Aplikasi Klien)

1. **Peningkatan Ketahanan Koneksi Database**
   - Ditambahkan property `IsDatabaseAvailable` dan `LastError` di class `Database` untuk melacak status koneksi
   - Dimodifikasi static constructor pada `Database.cs` untuk menangkap exception tanpa menghentikan aplikasi
   - Implementasi mekanisme fallback agar aplikasi tetap bisa berjalan meskipun database tidak tersedia

2. **Perbaikan Form Login**
   - Diperbarui metode `TestDatabaseConnection()` untuk mengecek status koneksi secara lebih akurat
   - Ditambahkan UI yang lebih informatif saat database tidak tersedia (pesan error dan status)
   - Implementasi tombol login dengan validasi dan hashing password yang lebih baik
   - Ditingkatkan fitur animasi form untuk UX yang lebih baik

3. **Peningkatan Error Handling di Program Utama**
   - Dimodifikasi `Program.cs` untuk menangani exception dengan lebih baik
   - Ditambahkan pembuatan direktori log otomatis jika belum ada
   - Diubah alur startup aplikasi agar tetap bisa berjalan dengan fungsionalitas terbatas
   - Ditambahkan metode `ShowErrorMessage()` untuk menampilkan pesan error yang lebih informatif

4. **Keamanan dan Robustness**
   - Implementasi hashing password menggunakan SHA-256 di `LoginForm.cs`
   - Pemisahan logika login ke metode terpisah untuk maintainability yang lebih baik
   - Penanganan kondisi edge-case dan validasi input yang lebih ketat
   - Logging yang lebih komprehensif untuk memudahkan troubleshooting

### Masalah yang Berhasil Diselesaikan

1. **Form Kosong Saat Startup**
   - Aplikasi sekarang selalu menampilkan form login meskipun koneksi database gagal
   - Pesan error yang jelas ditampilkan kepada user saat koneksi database bermasalah
   - User bisa melihat status koneksi dan mendapat informasi akurat tentang masalah

2. **Crash Aplikasi Saat Database Tidak Tersedia**
   - Aplikasi tidak lagi crash saat PostgreSQL service tidak berjalan
   - Error handling yang lebih baik di semua lapisan aplikasi
   - Tombol exit selalu tersedia untuk menutup aplikasi dengan aman

## Catatan Teknis

- Untuk menjalankan aplikasi dengan baik, pastikan PostgreSQL service aktif (`postgresql-x64-14`)
- Kredensial default: username=postgres, password=root@rsi
- Database: parkirdb (dibuat otomatis jika belum ada)
- File log aplikasi tersedia di folder `Logs` di direktori aplikasi untuk troubleshooting lanjutan 

## Tanggal: 22 Maret 2025

### Perbaikan dan Pembaruan ParkingOut (WPF Migration)

1. **Migrasi ke WPF**
   - Aplikasi ParkingOut telah berhasil dimigrasi dari WinForms ke WPF
   - Diimplementasikan UI modern dengan kontrol sidebar dan navigasi yang lebih baik
   - Ditambahkan halaman Vehicle Entry dan Vehicle Exit dengan antarmuka yang lebih intuitif
   - Dibuat kontrol SidebarControl khusus untuk navigasi aplikasi

2. **Perbaikan Parameter Logger**
   - Diperbaiki urutan parameter pada metode logger.Error() di seluruh aplikasi
   - Dibuat kelas AppLogger yang membungkus NLogLogger untuk standardisasi logging
   - Ditambahkan utility MessageHelper untuk menampilkan pesan error dengan format konsisten
   - Dibuat interface IAppLogger untuk memudahkan mocking dan testing

3. **Penanganan Nullable Reference Types**
   - Diaktifkan fitur nullable reference types untuk meningkatkan keamanan tipe
   - Ditambahkan null checking untuk mencegah NullReferenceException
   - Diperbarui property di kelas model untuk mendukung nullable types
   - Ditambahkan null coalescing operators (??) untuk menangani nilai null dengan aman

4. **Perbaikan Build Error**
   - Dihapus referensi ke file icon yang hilang
   - Diperbarui project file untuk menggunakan App.xaml sebagai startup object
   - Ditambahkan .gitignore untuk mengabaikan file-file yang tidak perlu di-track

5. **Repository Management**
   - Diinisialisasi Git repository lokal
   - Kode ParkingOut berhasil di-push ke GitHub di https://github.com/SIJA-SKANSAPUNG/parking-lot.git
   - Diperbarui dan ditambahkan file dokumentasi (README.md, jejak.md)

### Masalah yang Berhasil Diselesaikan

1. **Argumen Logger Error**
   - Diperbaiki error CS1503 terkait ketidakcocokan parameter pada logger.Error() di Program.cs
   - Program.cs diganti dengan App.xaml.cs sebagai entrypoint aplikasi WPF

2. **Kesalahan Referensi Null**
   - Diperbaiki warning CS8602 dan CS8601 terkait dereferensi referensi yang mungkin null
   - Ditambahkan perlindungan dengan null checking dan operator conditional

3. **File Icon yang Hilang**
   - Dihapus referensi ke file parking-meter.ico yang menyebabkan error build
   - Diset ApplicationIcon kosong di project file

## Catatan Teknis

- Aplikasi ParkingOut sekarang menggunakan .NET 6.0 dengan WPF
- Struktur proyek telah dioptimalkan dengan memisahkan kode UI dan logika bisnis
- Implementasi pattern MVVM sedang dalam proses untuk meningkatkan testability 

## Tanggal: 23 Maret 2025

### Implementasi Dashboard dan Sistem Login (WPF)

1. **Penambahan Sistem Autentikasi**
   - Diimplementasikan halaman login (LoginPage.xaml) dengan validasi pengguna
   - Dibuat sistem sesi pengguna (UserSession) untuk mengelola status autentikasi
   - Ditambahkan class User untuk menyimpan informasi pengguna yang terautentikasi
   - Implementasi validasi credential dengan username/password (admin/password123)

2. **Pengembangan Dashboard**
   - Diperbarui DashboardPage.xaml dengan desain modern dan responsif
   - Ditambahkan empat kartu statistik: Available Slots, Today's Entries, Today's Exits, dan Today's Revenue
   - Implementasi panel Quick Actions untuk navigasi cepat ke fungsi utama
   - Ditambahkan DataGrid untuk menampilkan aktivitas parkir terbaru

3. **Peningkatan User Experience**
   - Ditambahkan tombol Logout di dashboard untuk keluar dari sesi pengguna
   - Diperbarui welcome message yang menampilkan informasi pengguna yang login
   - Ditambahkan validasi untuk memastikan pengguna sudah login sebelum mengakses halaman tertentu
   - Implementasi dialog konfirmasi saat pengguna memilih untuk logout

4. **Integrasi Navigasi Aplikasi**
   - Dimodifikasi MainWindow.xaml.cs untuk menampilkan LoginPage sebagai halaman awal
   - Ditambahkan proteksi pada navigasi untuk mencegah akses tanpa autentikasi
   - Implementasi NavigationService untuk transisi halus antar halaman
   - Diperbarui event handler untuk button sidebar agar memverifikasi status login

### Fitur Baru

1. **Sistem Autentikasi**
   - Login menggunakan username dan password predefinisi
   - Penyimpanan informasi pengguna dalam sesi aplikasi
   - Logout dengan konfirmasi untuk memastikan pengguna tidak keluar secara tidak sengaja
   - Validasi input untuk mencegah masukan kosong atau tidak valid

2. **Dasbor Informatif**
   - Tampilan statistik real-time untuk monitoring status parkir
   - Panel aktivitas terbaru yang menampilkan transaksi parkir terakhir
   - Tombol Quick Action untuk akses cepat ke fungsi-fungsi utama
   - Tampilan user-friendly dengan desain modern dan profesional

## Catatan Teknis

- Untuk testing, gunakan kredensial: username=admin, password=password123
- Struktur project diperbaiki dengan menambahkan folder Models untuk class User
- Sistem navigasi aplikasi disesuaikan untuk menggunakan Page berbasis autentikasi
- Button style yang digunakan masih dalam tahap pengembangan untuk standardisasi UI