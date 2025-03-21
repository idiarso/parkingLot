# Panduan Instalasi dan Konfigurasi Perangkat Keras

## Daftar Isi
1. [Persyaratan Sistem](#persyaratan-sistem)
2. [Instalasi Sistem](#instalasi-sistem)
3. [Konfigurasi Printer Thermal](#konfigurasi-printer-thermal)
4. [Konfigurasi Barcode Scanner](#konfigurasi-barcode-scanner)
5. [Konfigurasi MCU ATMEL](#konfigurasi-mcu-atmel)
6. [Konfigurasi Kamera](#konfigurasi-kamera)
7. [Konfigurasi Gate Barrier (Portal)](#konfigurasi-gate-barrier-portal)
8. [Pemecahan Masalah](#pemecahan-masalah)

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
- PostgreSQL 14 atau yang lebih baru
- Driver printer thermal

## Instalasi Sistem

1. Instal semua perangkat lunak yang diperlukan
2. Clone atau ekstrak kode sumber di folder D:\21maret
3. Jalankan file `buildall.bat` untuk membangun semua aplikasi
4. Pastikan semua aplikasi berhasil dibangun sebelum melanjutkan

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
4. Aplikasi ParkingIN menerima sinyal dan memproses data
5. Printer thermal mencetak tiket masuk dengan barcode

#### Exit Process
1. Kendaraan terdeteksi di pintu keluar
2. Petugas memindai barcode di tiket
3. MCU mengirim sinyal "OUT:<ID>" ke PC
4. Aplikasi ParkingOUT memproses data dan memvalidasi
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

## Pemecahan Masalah

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
