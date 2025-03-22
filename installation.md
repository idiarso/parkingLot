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

#### Troubleshooting ParkingOut

1. **Error "MSB1011: Multiple Projects"**
   - Selalu gunakan flag `--project ParkingOut.csproj` saat menggunakan dotnet run/build

2. **Error Koneksi Database**
   - Verifikasi string koneksi di `App.config` sudah benar
   - Pastikan PostgreSQL berjalan dengan `sc query postgresql-x64-14` (sesuaikan versinya)
   - Pastikan database parkingdb sudah dibuat

3. **Error "The application was unable to start correctly (0xc000007b)"**
   - Ini biasanya berarti DLL yang dibutuhkan tidak dapat dimuat
   - Pastikan .NET Runtime terinstall dengan benar
   - Jalankan perintah `dotnet restore ParkingOut.csproj` untuk memastikan semua dependensi terunduh

4. **UI Error atau Exception**
   - Jalankan dengan mengikuti log error: `dotnet run --project ParkingOut.csproj > error-log.txt 2>&1`
   - Periksa log untuk detail error

5. **Aplikasi Tidak Merespons**
   - Pastikan ParkingServer berjalan jika aplikasi memerlukan koneksi ke server
   - Periksa penggunaan memory dan CPU sistem

### Database

#### PostgreSQL
1. Pastikan PostgreSQL sudah terinstal dan berjalan
2. Kredensial default: 
   - Username: postgres
   - Password: root@rsi
   - Port: 5432
   - Database: parkingdb
3. Untuk menjalankan PostgreSQL secara manual, gunakan:
   ```
   cd /d D:\21maret\clean_code\ParkingIN\Database
   call StartPostgreSQL.bat
   ```

## Konfigurasi Database

### PostgreSQL

1. **Instalasi PostgreSQL**
   - Unduh PostgreSQL 14 atau yang lebih baru dari [website resmi](https://www.postgresql.org/download/)
   - Instal dengan mengikuti panduan instalasi
   - Saat diminta, atur password untuk user "postgres" menjadi "root@rsi"
   - Pastikan service PostgreSQL berjalan di port default 5432

2. **Inisialisasi Database**
   - Jalankan script `D:\21maret\InitializeDatabase.bat` untuk membuat database dan tabel
   - Script akan membuat database "parkingdb" dan semua tabel yang diperlukan

3. **Konfigurasi Koneksi**
   - Aplikasi menggunakan connection string yang terdapat di file konfigurasi masing-masing aplikasi
   - ParkingServer: `D:\21maret\clean_code\ParkingServer\appsettings.json`
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Port=5432;Database=parkingdb;Username=postgres;Password=root@rsi;"
     }
   }
   ```
   - ParkingIN: `D:\21maret\clean_code\ParkingIN\App.config`
   - ParkingOut: `D:\21maret\clean_code\ParkingOut\App.config`
   - Contoh connection string PostgreSQL:
   ```xml
   <connectionStrings>
     <add name="ParkingConnection" connectionString="Host=localhost;Port=5432;Database=parkingdb;Username=postgres;Password=root@rsi;" providerName="Npgsql" />
   </connectionStrings>
   ```

## Konfigurasi Printer Thermal

### Instalasi Driver
1. Instal driver printer thermal sesuai dengan model yang digunakan
2. Verifikasi instalasi dengan mencetak test page dari Windows

### Konfigurasi di Aplikasi
1. Buka file konfigurasi di `D:\21maret\clean_code\ParkingIN\App.config`
2. Atur parameter printer sesuai dengan model yang digunakan:
   ```xml
   <add key="PrinterName" value="POS-58" />
   <add key="PrinterWidth" value="58" />
   <add key="PrinterDPI" value="203" />
   ```
3. Jika menggunakan printer yang berbeda, sesuaikan nama printer dengan yang terdaftar di Windows

### Pengujian Printer
1. Jalankan aplikasi ParkingIN
2. Buka menu `Pengaturan` > `Printer`
3. Klik tombol `Test Print` untuk memastikan printer berfungsi

## Konfigurasi Barcode Scanner

### Koneksi Scanner
1. Hubungkan scanner barcode ke port USB
2. Tunggu hingga Windows menginstal driver secara otomatis
3. Verifikasi instalasi dengan scan barcode test

### Konfigurasi Mode Operasi
1. Set scanner barcode ke mode "Keyboard Emulation" menggunakan kode konfigurasi dari manual scanner
2. Atur sufiks menjadi "Enter" (CR) untuk mengirimkan data setelah scan
3. Atur scanner untuk membaca kode jenis "Code 128" dan "QR Code"

### Pengujian Scanner
1. Buka aplikasi ParkingIN
2. Arahkan kursor ke kolom "Nomor Kendaraan"
3. Scan barcode untuk memastikan data muncul di kolom tersebut

## Konfigurasi MCU ATMEL

### Koneksi Hardware
1. Hubungkan MCU ATMEL ke PC menggunakan kabel RS232 (DB9)
2. Verifikasi port COM yang digunakan (umumnya COM1 atau COM2)
3. Hubungkan push button dan sensor ke pin input MCU sesuai skema:
   - Push Button Entry: Pin D2
   - Vehicle Detector: Pin D3

### Konfigurasi Software
1. Buka file konfigurasi di `D:\21maret\clean_code\ParkingIN\App.config`
2. Atur parameter komunikasi serial:
   ```xml
   <add key="ComPort" value="COM1" />
   <add key="BaudRate" value="9600" />
   <add key="DataBits" value="8" />
   <add key="Parity" value="None" />
   <add key="StopBits" value="One" />
   ```

### Event Flow

#### Entry Process
1. Kendaraan terdeteksi oleh sensor
2. Pengguna menekan push button
3. MCU ATMEL mengirim sinyal "IN:<ID>" ke PC melalui port serial
4. Aplikasi ParkingIN menerima sinyal dan memproses data di database parkingdb
5. Printer thermal mencetak tiket masuk dengan barcode

#### Exit Process
1. Kendaraan terdeteksi di pintu keluar
2. Petugas memindai barcode di tiket
3. MCU mengirim sinyal "OUT:<ID>" ke PC
4. Aplikasi ParkingOUT memproses data dan memvalidasi di database parkingdb
5. Printer mencetak tiket keluar atau struk pembayaran

## Konfigurasi Kamera

Sistem ini mendukung dua jenis kamera untuk pengambilan gambar kendaraan:

### Webcam Lokal

1. **Persyaratan Hardware**
   - Webcam USB standar yang kompatibel dengan DirectShow
   - Resolusi minimal 640x480 (rekomendasi: 1280x720)
   - Pastikan webcam terpasang dengan mantap dan terarah ke area kendaraan

2. **Instalasi Driver**
   - Pasang webcam ke port USB pada komputer
   - Instal driver yang disertakan atau biarkan Windows menginstal driver secara otomatis
   - Pastikan webcam muncul di Device Manager (tekan Win+X, pilih Device Manager)
   - Verifikasi webcam berfungsi dengan aplikasi Camera bawaan Windows

3. **Konfigurasi Sistem**
   - Edit file `D:\21maret\clean_code\ParkingIN\config\camera.ini`
   - Ubah `Type=Webcam`
   - Sesuaikan `Device_Index` jika Anda memiliki beberapa webcam (0 = kamera pertama)
   - Sesuaikan `Resolution` sesuai kemampuan webcam

4. **Testing**
   - Jalankan `test_hardware.bat` dan pilih opsi "Test Camera"
   - Verifikasi bahwa webcam terdeteksi dan dapat mengambil gambar
   - Cek hasil gambar di folder `test_images`

### IP Camera

1. **Persyaratan Hardware**
   - IP Camera dengan dukungan RTSP atau HTTP snapshot
   - Alamat IP statis atau DHCP reservation pada jaringan lokal
   - PoE (Power over Ethernet) atau adaptor daya terpisah
   - Kabel jaringan yang terhubung ke jaringan yang sama dengan PC

2. **Instalasi Kamera**
   - Pasang kamera pada posisi yang tepat untuk memotret kendaraan
   - Hubungkan kamera ke jaringan 
   - Konfigurasi alamat IP kamera melalui software bawaannya
   - Pastikan kamera dapat diakses melalui browser dengan alamat IP nya

3. **Konfigurasi Sistem**
   - Edit file `D:\21maret\clean_code\ParkingIN\config\camera.ini`
   - Ubah `Type=IP`
   - Sesuaikan `IP`, `Username`, `Password`, dan `Port` sesuai konfigurasi kamera
   - Sesuaikan `Resolution` jika kamera mendukung pengaturan resolusi via URL

4. **Testing**
   - Jalankan `test_hardware.bat` dan pilih opsi "Test Camera"
   - Verifikasi bahwa sistem dapat menghubungi IP kamera
   - Verifikasi bahwa snapshot dapat diambil

## Konfigurasi Gate Barrier

Sistem ini terintegrasi dengan gate barrier (portal) menggunakan kontroler mikro ATMEL melalui port serial RS232.

1. **Perangkat yang Dibutuhkan**
   - Gate barrier dengan kontroler mikro ATMEL
   - Kabel RS232 (DB9) atau converter USB-to-RS232
   - Driver USB-to-Serial jika menggunakan converter

2. **Instalasi Hardware**
   - Hubungkan gate barrier controller ke PC menggunakan kabel serial
   - Jika menggunakan converter USB, instal driver yang sesuai
   - Catat nomor port COM yang digunakan (cek di Device Manager)

3. **Konfigurasi Sistem**
   - Edit file `D:\21maret\clean_code\ParkingIN\config\gate.ini`
   - Sesuaikan `COM_Port` dengan port yang digunakan (misal: COM3)
   - Pastikan `Baud_Rate`, `Data_Bits`, `Stop_Bits`, dan `Parity` sesuai dengan spesifikasi kontroler
   - Sesuaikan command di bagian `[Commands]` jika berbeda dengan default kontroler

4. **Pengujian Sambungan**
   - Jalankan `test_hardware.bat` dan pilih opsi "Test Serial Port / Gate"
   - Verifikasi bahwa sistem dapat berkomunikasi dengan kontroler
   - Uji perintah buka dan tutup gate jika aman untuk dilakukan

5. **Konfigurasi Safety**
   - Pastikan sensor keselamatan terpasang dan terkonfigurasi
   - Atur `Enable_Sensors=true` di bagian `[Safety]`
   - Aktifkan `Prevent_Close_When_Occupied=true` untuk mencegah portal menutup saat kendaraan terdeteksi

## Konfigurasi Printer Thermal

Sistem ini menggunakan printer thermal untuk mencetak tiket masuk dan struk keluar.

1. **Persyaratan Hardware**
   - Printer thermal 58mm atau 80mm (Rekomendasi: EPSON TM-T82X, EPSON TM-T20, atau setara)
   - Kabel USB atau Serial untuk koneksi
   - Catu daya printer

2. **Instalasi Driver**
   - Unduh dan instal driver printer dari situs resmi produsen
   - Pastikan printer muncul di Windows sebagai printer terinstall
   - Cetak test page dari Windows untuk memastikan printer berfungsi

3. **Konfigurasi Sistem**
   - Edit file `D:\21maret\clean_code\ParkingIN\config\printer.ini`
   - Sesuaikan `Name` dengan nama printer yang terinstall
   - Sesuaikan `Paper_Width` dengan lebar kertas yang digunakan (80 atau 58)
   - Konfigurasikan header dan footer tiket sesuai kebutuhan
   - Sesuaikan format tiket di bagian `[TicketFormat]`

4. **Pengujian Printer**
   - Jalankan `test_hardware.bat` dan pilih opsi "Test Thermal Printer"
   - Verifikasi bahwa printer terdeteksi
   - Cetak test page untuk memastikan printer berfungsi dengan baik

## Konfigurasi Barcode Scanner

Sistem menggunakan barcode scanner untuk memindai tiket pada proses keluar.

1. **Persyaratan Hardware**
   - Barcode scanner USB yang mendukung QR code dan Code128 (jika menggunakan barcode linear)
   - Scanner harus dikonfigurasi untuk menambahkan Enter (CR) setelah pembacaan

2. **Instalasi Scanner**
   - Hubungkan scanner ke port USB PC
   - Scanner biasanya terdeteksi sebagai keyboard device dan tidak memerlukan driver khusus
   - Jika diperlukan, konfigurasi scanner menggunakan manual bawaan untuk menambahkan Enter setelah scan

3. **Pengujian Scanner**
   - Jalankan `test_hardware.bat` dan pilih opsi "Test Barcode Scanner"
   - Pindai barcode untuk memverifikasi pembacaan

## Pengujian Integrasi Hardware

Setelah semua hardware terkonfigurasi, jalankan pengujian integrasi untuk memastikan semua komponen berfungsi bersama.

1. Jalankan `test_hardware.bat` dan pilih opsi "Test All Hardware"
2. Ikuti instruksi untuk menguji semua komponen
3. Verifikasi bahwa:
   - Kamera dapat mengambil gambar
   - Gate barrier dapat menerima dan merespons perintah
   - Printer dapat mencetak tiket
   - Barcode scanner dapat membaca barcode
   - Semua komponen dapat berinteraksi dengan database parkingdb

## Pemecahan Masalah

### Masalah Database

#### PostgreSQL
- **PostgreSQL service tidak berjalan**: Jalankan `services.msc` dan start service PostgreSQL
- **Error koneksi**: Verifikasi username dan password di connection string
- **Database tidak ditemukan**: Jalankan script inisialisasi database untuk membuat database parkingdb
- **Error saat memulai service**: Periksa log PostgreSQL di Event Viewer atau di folder log PostgreSQL
- **Data tidak konsisten**: Gunakan tool pgAdmin untuk memeriksa database parkingdb secara langsung

### Masalah Aplikasi

#### ParkingServer
- **Tidak dapat memulai**: Pastikan tidak ada instance lain yang berjalan di port yang sama
- **Error koneksi database**: Verifikasi connection string di appsettings.json merujuk ke database parkingdb

#### ParkingOut
- **Error build "MSB1011" (multiple projects)**: Gunakan perintah `dotnet build ParkingOut.csproj`
- **Error restore package**: Pastikan internet aktif dan NuGet dikonfigurasi dengan benar
- **Error koneksi ke server**: Pastikan ParkingServer berjalan dan terhubung ke database parkingdb

### Masalah Kamera
- **Webcam tidak terdeteksi**: Pastikan driver terinstall dengan benar dan webcam muncul di Device Manager
- **IP Camera tidak dapat diakses**: Verifikasi alamat IP, port, dan kredensial. Pastikan kamera berada di jaringan yang sama
- **Gambar tidak tersimpan**: Periksa folder penyimpanan dan pastikan aplikasi memiliki izin tulis

### Masalah Gate Barrier
- **Port COM tidak terdeteksi**: Periksa koneksi kabel dan driver USB-to-Serial jika digunakan
- **Tidak ada respons dari kontroler**: Verifikasi baudrate dan parameter komunikasi lainnya
- **Gate tidak merespons perintah**: Periksa koneksi antara kontroler dan motor gate

### Masalah Printer
- **Printer tidak terdeteksi**: Periksa driver dan koneksi USB/Serial
- **Kertas tidak keluar**: Pastikan printer memiliki kertas dan tidak terjadi paper jam
- **Hasil cetak tidak jelas**: Periksa kualitas kertas thermal dan pengaturan densitas printer

### Masalah Barcode Scanner
- **Scanner tidak membaca**: Periksa apakah barcode jelas dan tidak rusak
- **Scan tidak masuk ke aplikasi**: Pastikan scanner dikonfigurasi untuk menambahkan Enter setelah scan
- **Scan terbaca sebagai karakter salah**: Sesuaikan keyboard layout pada scanner

## Informasi Kontak Support
Untuk bantuan teknis, hubungi:
- Email: support@parking-system.com
- Telepon: (021) 1234-5678
- Jam kerja: Senin-Jumat, 08.00-17.00 WIB
