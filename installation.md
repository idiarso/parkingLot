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

### Jenis Kamera yang Didukung
1. **Webcam Lokal**
   - Kamera USB yang langsung terhubung ke PC
   - Resolusi minimal 720p disarankan untuk hasil gambar yang baik
   - Pastikan driver kamera terinstal dengan benar

2. **Kamera IP**
   - Kamera yang terhubung melalui jaringan
   - Mendukung protokol RTSP/HTTP
   - Format URL: `rtsp://username:password@ip-address:port/path` atau `http://ip-address/video`

### Instalasi Kamera
1. **Webcam Lokal**
   - Pasang webcam pada port USB
   - Tunggu hingga driver terinstal secara otomatis
   - Verifikasi kamera terdeteksi di Device Manager Windows

2. **Kamera IP**
   - Pasang kamera IP pada jaringan lokal
   - Atur alamat IP statis atau DHCP reservation
   - Pastikan PC dapat mengakses kamera melalui browser

### Konfigurasi di Aplikasi
1. Buka file konfigurasi di `D:\21maret\clean_code\ParkingIN\App.config`
2. Atur parameter kamera sesuai dengan jenis yang digunakan:
   ```xml
   <!-- Untuk webcam lokal -->
   <add key="CameraType" value="Webcam" />
   <add key="WebcamIndex" value="0" /> <!-- 0 untuk webcam pertama -->
   
   <!-- Untuk kamera IP -->
   <add key="CameraType" value="IPCamera" />
   <add key="IPCameraUrl" value="rtsp://admin:admin@192.168.1.100:554/stream1" />
   ```
3. Atur parameter kualitas gambar:
   ```xml
   <add key="ImageResolution" value="1280x720" /> <!-- resolusi gambar -->
   <add key="ImageQuality" value="90" /> <!-- kualitas JPG 0-100 -->
   <add key="CaptureTimeout" value="3000" /> <!-- timeout dalam milidetik -->
   ```

### Pengujian Kamera
1. Jalankan aplikasi ParkingIN
2. Buka menu `Pengaturan` > `Kamera`
3. Klik tombol `Test Capture` untuk memastikan kamera berfungsi
4. Gambar hasil capture seharusnya ditampilkan di layar preview

### Fungsi Otomatis
Kamera akan secara otomatis mengambil gambar kendaraan saat:
1. Sensor mendeteksi kehadiran kendaraan
2. Push button ditekan oleh petugas atau pengemudi
3. Gambar disimpan dengan ID yang sama dengan ID barcode tiket

## Konfigurasi Gate Barrier (Portal)

### Koneksi Hardware
1. Hubungkan sistem gate barrier ke MCU ATMEL
2. Konfigurasikan pin output pada MCU untuk mengirim sinyal ke gate barrier:
   - Gate Entry: Pin D5
   - Gate Exit: Pin D6

### Pengaturan Software
1. Buka file konfigurasi di `D:\21maret\clean_code\ParkingOUT\App.config`
2. Atur parameter gate control:
   ```xml
   <add key="EnableGateControl" value="true" />
   <add key="GateOpenDuration" value="5000" /> <!-- durasi dalam milidetik -->
   <add key="RequirePaymentConfirmation" value="true" /> <!-- true jika perlu konfirmasi pembayaran -->
   ```

### Flow Proses Keluar dengan Gate Control
1. Petugas memindai barcode tiket
2. Sistem menampilkan data parkir dan foto kendaraan saat masuk
3. Petugas memverifikasi kendaraan dengan foto
4. Setelah pembayaran (jika ada), petugas menekan tombol "Buka Gate"
5. Sistem mengirim sinyal ke MCU untuk membuka portal
6. Portal terbuka selama durasi yang ditentukan
7. Kendaraan keluar dan proses selesai

## Pemecahan Masalah

### Masalah Printer
| Masalah | Solusi |
|---------|--------|
| Printer tidak merespons | 1. Periksa koneksi USB<br>2. Pastikan printer menyala<br>3. Periksa driver di Device Manager<br>4. Restart spooler printer dengan perintah: `net stop spooler && net start spooler` |
| Kualitas cetak buruk | 1. Periksa kualitas kertas thermal<br>2. Bersihkan print head<br>3. Sesuaikan pengaturan heat/density |
| Printer mencetak karakter tidak jelas | Pastikan encoding diatur dengan benar di kode program |

### Masalah Komunikasi Serial
| Masalah | Solusi |
|---------|--------|
| "Port tidak ditemukan" | 1. Periksa nomor COM di Device Manager<br>2. Sesuaikan pengaturan di App.config<br>3. Coba port COM yang berbeda |
| Data tidak diterima | 1. Periksa konfigurasi baudrate, parity, dll<br>2. Pastikan kabel DB9 terhubung dengan benar<br>3. Periksa apakah MCU aktif dan berfungsi |
| Data corrupt/tidak lengkap | Periksa timing komunikasi dan buffer length di kode program |

### Masalah Barcode Scanner
| Masalah | Solusi |
|---------|--------|
| Scanner tidak mendeteksi barcode | 1. Pastikan jenis barcode didukung<br>2. Sesuaikan jarak scan<br>3. Periksa kualitas cetak barcode |
| Data barcode tidak sesuai | Pastikan format data barcode sesuai dengan yang diharapkan aplikasi |

### Masalah Kamera
| Masalah | Solusi |
|---------|--------|
| Kamera tidak terdeteksi | 1. Periksa koneksi kamera<br>2. Pastikan driver kamera terinstal<br>3. Coba kamera lain |
| Gambar tidak jelas | 1. Periksa resolusi dan kualitas gambar<br>2. Sesuaikan pengaturan kamera<br>3. Coba kamera lain |

### Log dan Diagnostik
Semua kesalahan komunikasi perangkat dicatat dalam file log yang dapat ditemukan di:
- `D:\21maret\clean_code\ParkingIN\logs\hardware_YYYYMMDD.log`

Untuk diagnosis lebih lanjut, aktifkan mode debugging hardware di `App.config`:
```xml
<add key="HardwareDebugMode" value="true" />
```

## Informasi Kontak Support
Untuk bantuan teknis, hubungi:
- Email: support@parking-system.com
- Telepon: (021) 1234-5678
- Jam kerja: Senin-Jumat, 08.00-17.00 WIB
