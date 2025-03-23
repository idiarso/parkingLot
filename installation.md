# Panduan Instalasi dan Konfigurasi Perangkat Keras

## Daftar Isi
1. [Persyaratan Sistem](#persyaratan-sistem)
2. [Instalasi Sistem](#instalasi-sistem)
3. [Panduan Menjalankan Aplikasi](#panduan-menjalankan-aplikasi)
4. [Konfigurasi Database](#konfigurasi-database)
5. [Konfigurasi Printer Thermal](#konfigurasi-printer-thermal)
6. [Konfigurasi Barcode Scanner](#konfigurasi-barcode-scanner)
7. [Konfigurasi MCU ATMEL](#konfigurasi-mcu-atmel)
8. [Konfigurasi Kamera](#konfigurasi-kamera)
9. [Konfigurasi Gate Barrier (Portal)](#konfigurasi-gate-barrier-portal)
10. [Pemecahan Masalah](#pemecahan-masalah)
11. [Panduan Dashboard dan Login](#panduan-dashboard-dan-login)

## Persyaratan Sistem

### Perangkat Keras
- PC dengan minimal:
  - Processor: Intel i3 atau setara
  - RAM: 4GB
  - Storage: 100GB HDD/SSD
  - Port: 2x USB, 1x RS232/DB9
- Printer Thermal (58mm atau 80mm)
- Barcode Scanner
- Microcontroller ATMEL
- Push Button dan Sensor
- Kabel RS232 (DB9)
- Kamera IP atau webcam lokal
- Sistem gate barrier (portal otomatis)

### Perangkat Lunak
- Windows 10/11 (64-bit)
- .NET 6.0 Runtime
- PostgreSQL 14 atau yang lebih baru (untuk database parkingdb)
- Driver printer thermal

## Instalasi Sistem

1. Instal semua perangkat lunak yang diperlukan:
   - Windows 10/11 (64-bit)
   - [.NET 6.0 Runtime](https://dotnet.microsoft.com/download/dotnet/6.0)
   - [PostgreSQL 14](https://www.postgresql.org/download/)
   - Driver printer thermal

2. Clone atau ekstrak kode sumber di folder D:\21maret

3. Jalankan file `buildall.bat` untuk membangun semua aplikasi
4. Pastikan semua aplikasi berhasil dibangun sebelum melanjutkan

### Mengatasi Masalah "buildall.bat Force Close"

Jika `buildall.bat` tiba-tiba menutup (force close) saat dijalankan, coba solusi berikut:

1. **Jalankan sebagai Administrator**
   - Klik kanan pada file `buildall.bat`
   - Pilih "Run as Administrator"

2. **Jalankan melalui Command Prompt**
   - Buka Command Prompt sebagai Administrator
   - Navigasi ke direktori: `cd /d D:\21maret`
   - Jalankan: `buildall.bat > buildlog.txt 2>&1`
   - Periksa file `buildlog.txt` untuk informasi error

3. **Membangun Aplikasi Secara Manual**
   Jika buildall.bat tetap bermasalah, Anda dapat membangun aplikasi secara manual:
   
   a. ParkingServer:
   ```
   cd /d D:\21maret\clean_code\ParkingServer
   dotnet restore
   dotnet build
   ```

   b. ParkingIN:
   ```
   cd /d D:\21maret\clean_code\ParkingIN
   dotnet restore
   dotnet build
   ```

   c. ParkingOut:
   ```
   cd /d D:\21maret\clean_code\ParkingOut
   dotnet restore
   dotnet build ParkingOut.csproj
   ```

4. **Verifikasi .NET SDK**
   - Pastikan .NET SDK versi 6.0 terinstal
   - Periksa dengan menjalankan: `dotnet --list-sdks`
   - Jika belum terinstal, unduh dari [website resmi Microsoft](https://dotnet.microsoft.com/download/dotnet/6.0)

5. **Periksa Akses Direktori**
   - Pastikan user memiliki akses penuh (full access) ke direktori D:\21maret
   - Klik kanan pada folder → Properties → Security → Edit → Add/Modify permissions

## Panduan Menjalankan Aplikasi

Sistem parkir terdiri dari beberapa komponen utama yang perlu dijalankan bersama. Berikut adalah panduan lengkap untuk menjalankan sistem:

### Menggunakan Menu Launcher

Cara termudah untuk menjalankan aplikasi adalah menggunakan menu launcher:

1. Jalankan `run.bat` di folder utama (D:\21maret)
2. Pilih opsi yang diinginkan dari menu:
   - **[1] Run All Applications** - Menjalankan semua komponen (ParkingServer, ParkingIN, ParkingOut)
   - **[2] Run ParkingServer Only** - Menjalankan hanya server database
   - **[3] Run ParkingIN Only** - Menjalankan hanya aplikasi gate masuk
   - **[4] Run ParkingOut Only** - Menjalankan hanya aplikasi gate keluar
   - **[5] Check Application Status** - Memeriksa status semua komponen
   - **[6] Check PostgreSQL Service** - Memeriksa koneksi database PostgreSQL
   - **[7] Test Database Connection** - Menguji koneksi ke database parkingdb
   - **[8] Restart All Services** - Restart semua komponen
   - **[9] Exit** - Keluar dari launcher

### Menjalankan Komponen Secara Manual

#### ParkingServer
ParkingServer adalah komponen backend yang mengelola database dan logika bisnis.

1. Pastikan database PostgreSQL sudah berjalan dan database parkingdb telah dibuat
2. Buka command prompt
3. Masuk ke direktori aplikasi: `cd /d D:\21maret\clean_code\ParkingServer`
4. Jalankan: `dotnet run`

#### ParkingIN
ParkingIN adalah aplikasi untuk mengelola pintu masuk parkir.

1. Pastikan ParkingServer sudah berjalan dan terhubung ke database parkingdb
2. Buka command prompt baru
3. Masuk ke direktori aplikasi: `cd /d D:\21maret\clean_code\ParkingIN`
4. Jalankan: `dotnet run`

#### ParkingOut
ParkingOut adalah aplikasi untuk mengelola pintu keluar dan pembayaran.

1. Pastikan ParkingServer sudah berjalan dan terhubung ke database parkingdb
2. Buka command prompt baru
3. Masuk ke direktori aplikasi: `cd /d D:\21maret\clean_code\ParkingOut`
4. Jalankan: `dotnet run --project ParkingOut.csproj`

### Panduan Lengkap Build dan Run ParkingOut

Berikut adalah langkah lengkap untuk membangun dan menjalankan aplikasi ParkingOut:

#### Persiapan

1. **Pastikan Prerequisites Terinstall**
   - .NET 6.0 SDK terinstall - verifikasi dengan `dotnet --list-sdks`
   - PostgreSQL berjalan dan database parkingdb sudah dibuat
   - ParkingServer sudah berjalan (opsional, tergantung apakah Anda ingin menguji koneksi)

2. **Persiapkan Environment**
   - Buka Command Prompt sebagai Administrator
   - Pastikan semua instance ParkingOut yang mungkin berjalan sudah ditutup

#### Build ParkingOut

1. **Navigasi ke Direktori ParkingOut**
   ```
   cd /d D:\21maret\clean_code\ParkingOut
   ```

2. **Clean Project (Opsional tapi Direkomendasikan)**
   ```
   dotnet clean ParkingOut.csproj
   ```

3. **Restore Packages**
   ```
   dotnet restore ParkingOut.csproj
   ```
   Tunggu hingga semua package berhasil didownload dan dipulihkan.

4. **Build Project**
   ```
   dotnet build ParkingOut.csproj
   ```
   Pastikan build sukses tanpa error. Jika terdapat warning, umumnya masih bisa dilanjutkan.

#### Run ParkingOut

1. **Metode 1: Menggunakan dotnet run**
   ```
   dotnet run --project ParkingOut.csproj
   ```
   Ini akan membangun (jika diperlukan) dan menjalankan aplikasi dalam mode debug.

2. **Metode 2: Menjalankan Executable Langsung**
   ```
   cd /d D:\21maret\clean_code\ParkingOut\bin\Debug\net6.0-windows
   ParkingOut.exe
   ```
   Gunakan metode ini jika aplikasi sudah berhasil dibangun sebelumnya.

3. **Metode 3: Menggunakan Visual Studio (jika terinstall)**
   - Buka file `SimpleParkingAdmin.sln` dengan Visual Studio
   - Set ParkingOut sebagai Startup Project
   - Tekan F5 untuk menjalankan dalam mode debug

#### Menjalankan Aplikasi WPF (TestWpfApp)

Aplikasi TestWpfApp adalah versi WPF baru dari sistem parkir dengan dashboard modern dan sistem login.

1. **Build Aplikasi WPF**
   ```
   cd /d D:\21maret\TestWpfApp
   dotnet restore
   dotnet build
   ```

2. **Jalankan Aplikasi WPF**
   ```
   dotnet run
   ```
   Atau jalankan executable secara langsung:
   ```
   cd /d D:\21maret\TestWpfApp\bin\Debug\net6.0-windows
   TestWpfApp.exe
   ```

## Panduan Dashboard dan Login

### Menggunakan Sistem Login

Aplikasi WPF terbaru dilengkapi dengan sistem autentikasi yang mengharuskan login sebelum dapat mengakses dashboard.

1. **Halaman Login**
   - Saat aplikasi dijalankan, halaman login akan muncul terlebih dahulu
   - Masukkan kredensial berikut untuk login:
     - Username: `admin`
     - Password: `password123`
   - Klik tombol "SIGN IN" atau tekan Enter pada keyboard untuk melakukan login

2. **Fitur Login**
   - Validasi input untuk memastikan username dan password tidak kosong
   - Pesan error akan ditampilkan jika kredensial tidak valid
   - Opsi "Remember me" untuk menyimpan informasi login (fitur ini masih dalam pengembangan)
   - Link "Forgot Password" yang akan menampilkan petunjuk reset password

### Menggunakan Dashboard

Setelah login berhasil, dashboard utama akan ditampilkan dengan berbagai informasi dan fitur.

1. **Tampilan Dashboard**
   - **Header**: Menampilkan nama pengguna, peran, dan waktu login terakhir
   - **Statistik**: Menampilkan 4 kartu statistik (Available Slots, Today's Entries, Today's Exits, Today's Revenue)
   - **Quick Actions**: Tombol untuk akses cepat ke fungsi utama (Vehicle Entry, Vehicle Exit, Vehicle Monitoring)
   - **Recent Activity**: Tabel yang menampilkan aktivitas parkir terbaru

2. **Navigasi**
   - Gunakan sidebar di sebelah kiri untuk navigasi antar halaman
   - Semua halaman dilindungi dan hanya dapat diakses setelah login berhasil
   - Jika mencoba mengakses halaman tanpa login, sistem akan mengarahkan kembali ke halaman login

3. **Logout**
   - Klik tombol "Logout" di pojok kanan atas dashboard untuk keluar dari aplikasi
   - Konfirmasi dialog akan muncul untuk memastikan Anda ingin logout
   - Setelah logout, Anda akan diarahkan kembali ke halaman login

## Catatan Teknis Dashboard

1. **Keamanan**
   - Sistem login masih menggunakan kredensial hardcoded untuk demo
   - Dalam pengembangan selanjutnya akan terintegrasi dengan database untuk autentikasi
   - Hindari menyimpan password yang sensitif di aplikasi ini untuk saat ini

2. **Keterbatasan**
   - Dashboard saat ini menampilkan data statis untuk demonstrasi
   - Integrasi dengan data real-time dari database masih dalam pengembangan
   - Beberapa tombol Quick Action mungkin belum berfungsi sepenuhnya

3. **Troubleshooting**
   - Jika dashboard tidak muncul setelah login, restart aplikasi dan coba lagi
   - Jika tombol tidak responsif, periksa error di Output window jika menggunakan Visual Studio
   - Untuk masalah lain, periksa file log aplikasi di folder logs
