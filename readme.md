# Panduan Migrasi Database ke PostgreSQL

## Pengantar

Sistem Parkir kini menggunakan PostgreSQL sebagai database utama, menggantikan MySQL. Panduan ini akan membantu Anda mengonfigurasi dan memigrasi database dengan benar.

## Prasyarat

1. PostgreSQL 14 atau yang lebih baru sudah terinstal
2. .NET 6.0 SDK sudah terinstal
3. Visual Studio 2022 atau yang lebih baru (opsional)

## Langkah Migrasi

### 1. Instalasi PostgreSQL

Jika belum menginstal PostgreSQL:
1. Unduh PostgreSQL dari [situs resmi](https://www.postgresql.org/download/)
2. Instal dengan pengaturan berikut:
   - Password untuk user 'postgres': `root@rsi`
   - Port: `5432`
   - Locale: `Default`

### 2. Membuat Database

Database dapat dibuat secara otomatis dengan menjalankan script:

```
D:\21maret\clean_code\ParkingIN\Database\init_postgres_db.bat
```

Script ini akan:
- Memastikan service PostgreSQL berjalan
- Membuat database `parkirdb`
- Membuat tabel-tabel yang diperlukan
- Menambahkan data awal yang diperlukan

### 3. Konfigurasi Aplikasi

Konfigurasi koneksi database ada di file berikut:
- ParkingIN: `App.config`
- ParkingServer: `config.ini` dan `Program.cs`
- ParkingOut: `App.config`

String koneksi yang digunakan adalah:
```
Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;
```

### 4. Membangun dan Menjalankan Aplikasi

1. Untuk membangun semua aplikasi, jalankan:
   ```
   D:\21maret\buildall.bat
   ```

2. Untuk menjalankan aplikasi, gunakan:
   ```
   D:\21maret\run.bat
   ```

## Pemecahan Masalah

1. **Masalah Koneksi Database**
   - Pastikan PostgreSQL berjalan dengan menjalankan `StartPostgreSQL.bat`
   - Verifikasi kredensial login di file konfigurasi

2. **Masalah Aplikasi Tidak Merespons**
   - Restart aplikasi menggunakan opsi "Restart All Services" dalam `run.bat`
   - Periksa log aplikasi di folder `logs`

## Pembaharuan Terakhir

Migrasi selesai pada 22 Maret 2025. Semua fitur telah diuji dan berfungsi dengan baik.

## Informasi Tambahan

Dokumentasi lebih lanjut tentang pengoperasian sistem dapat ditemukan di `jejak.md` dan panduan perangkat keras di `installation.md`.

## Integrasi Hardware

Sistem ini terintegrasi dengan perangkat keras berikut:

### 1. Push Button dan Mikrokontroler ATMEL

Sistem ini menggunakan mikrokontroler ATMEL yang terhubung ke PC melalui port RS232 (DB9) untuk mengirimkan sinyal ketika push button ditekan. Flow proses adalah sebagai berikut:

#### Proses Masuk Parkir:
1. Kendaraan terdeteksi oleh sensor
2. Pengguna menekan push button
3. MCU ATMEL mengirim sinyal "IN:<ID>" ke PC
4. Sistem mengambil foto kendaraan secara otomatis
5. Aplikasi ParkingIN memproses data
6. Printer thermal mencetak tiket masuk dengan barcode

#### Proses Keluar Parkir:
1. Kendaraan terdeteksi di pintu keluar
2. Petugas memindai barcode di tiket
3. Sistem menampilkan data parkir dan foto kendaraan saat masuk untuk verifikasi
4. Petugas memvalidasi kendaraan yang akan keluar
5. Setelah pembayaran (jika diperlukan), petugas menekan tombol "Buka Gate"
6. Sistem mengirim sinyal ke MCU untuk membuka portal
7. Printer mencetak tiket keluar atau struk pembayaran

### 2. Printer Thermal

Sistem menggunakan printer thermal untuk mencetak tiket dengan barcode. Tiket akan otomatis tercetak ketika:
- Push button ditekan pada proses masuk
- Barcode dipindai dan divalidasi pada proses keluar

### 3. Kamera (Webcam/IP Camera)

Sistem mendukung penggunaan kamera IP atau webcam lokal untuk:
- Pengambilan gambar otomatis saat kendaraan terdeteksi
- Capture foto saat push button ditekan
- Identifikasi visual kendaraan (tidak perlu membaca plat nomor)
- Verifikasi kendaraan saat proses keluar dengan menampilkan foto yang diambil saat masuk

Ketika kendaraan keluar, petugas dapat membandingkan kendaraan fisik dengan foto yang ditampilkan untuk memverifikasi bahwa kendaraan yang akan keluar adalah kendaraan yang sama saat masuk.

### 4. Gate Barrier (Portal)

Sistem terintegrasi dengan portal otomatis untuk:
- Membuka portal masuk setelah tiket dicetak
- Membuka portal keluar setelah validasi dan pembayaran selesai

Untuk detail konfigurasi perangkat keras, lihat `installation.md`.
