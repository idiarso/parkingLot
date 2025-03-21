# ParkingIN - Sistem Manajemen Parkir Modern

ParkingIN adalah sistem manajemen parkir modern yang menyediakan otomatisasi lengkap untuk pengoperasian area parkir. Sistem ini mendukung deteksi kendaraan otomatis, pengenalan plat nomor, pencetakan tiket, dan pembukaan/penutupan gerbang.

## Fitur Utama

- Antarmuka pengguna yang intuitif dan modern
- Pencatatan kendaraan masuk dan keluar
- Pencetakan tiket dengan barcode
- Deteksi otomatis kendaraan menggunakan loop detector
- Pengenalan plat nomor menggunakan OCR (jika diaktifkan)
- Kontrol gerbang otomatis
- Dashboard monitoring real-time berbasis web
- Otomatisasi penuh tanpa memerlukan intervensi operator

## Komponen Sistem

1. **Aplikasi Desktop .NET 6.0 (C#)**
   - MenuForm: Antarmuka utama sistem
   - ParkingInForm: Pengelolaan kendaraan masuk
   - CameraSettingsForm: Konfigurasi kamera dan OCR
   - GateSettingsForm: Konfigurasi gerbang dan loop detector
   - PrinterSettingsForm: Konfigurasi printer tiket
   - LogViewerForm: Tampilan log sistem dan aktivitas
   - ReportsForm: Laporan dan statistik parkir

2. **Modul Kamera & OCR**
   - Dukungan untuk kamera IP dan webcam lokal
   - Pengenalan plat nomor menggunakan Emgu.CV
   - Pengambilan gambar otomatis saat kendaraan terdeteksi

3. **Sistem Loop Detector**
   - Deteksi otomatis kendaraan melalui loop induktif
   - Komunikasi serial dengan mikrokontroler ATmega
   - Pemrosesan otomatis ketika kendaraan terdeteksi
   - Dukungan port serial terpisah untuk loop detector dan gate controller

4. **Server WebSocket**
   - Notifikasi real-time ke web client
   - Pemantauan status perangkat secara real-time

5. **Database MySQL**
   - Penyimpanan data kendaraan dan transaksi
   - Dukungan untuk pembuatan laporan dan statistik

## Persyaratan Sistem

- Windows 10 atau yang lebih baru
- .NET SDK 6.0 atau yang lebih baru
- MySQL Server 8.0 atau yang lebih baru

## Dependensi

- .NET 6.0 Windows Forms
- Serilog (untuk logging)
- MySql.Data
- System.IO.Ports (untuk komunikasi serial)
- AForge.Video (untuk capture kamera)
- Fleck (untuk WebSocket server)

## Cara Setup

1. Install .NET 6.0 SDK dari https://dotnet.microsoft.com/download/dotnet/6.0
2. Clone repository ini
3. Buka terminal di folder ParkingIN dan jalankan:
```bash
dotnet restore
dotnet build
```
4. Setup database dengan menjalankan script:
```bash
tool.bat\setup_parking_db.bat
```
5. Jalankan aplikasi:
```bash
tool.bat\run_parkingin.bat
```

## Panduan Instalasi

1. Pastikan Anda telah menginstal .NET 6.0 Runtime atau lebih tinggi
2. Instal MySQL Server dan buat database dengan struktur dari `SQL/create_database.sql`
3. Atur koneksi database di `ParkingIN/App.config`
4. Jalankan aplikasi dengan mengklik 2x pada `ParkingIN.exe` atau melalui Visual Studio
5. Lakukan update database menggunakan script `SQL/update_database.sql` untuk menambahkan kolom baru

## Integrasi Loop Detector

### Persyaratan Perangkat Keras

1. **Mikrokontroler**
   - Arduino Uno/Nano/Mega atau board kompatibel ATmega
   - Sensor Loop Detector atau IR
   - LED indikator (opsional)

2. **Koneksi RS-232**
   - Kabel USB-to-Serial dengan konektor DB9, ATAU
   - Konverter level tegangan MAX232/MAX3232 untuk koneksi RS-232 langsung
   - Kabel RS-232 standar dengan konektor DB9

3. **Driver USB-to-Serial**
   - CH340/CH341: Driver untuk Arduino clone
   - FT232R: Driver untuk FTDI USB-to-Serial
   - CP210x: Driver untuk Silicon Labs USB-to-Serial

### Koneksi DB9

| Pin DB9 | Koneksi | Deskripsi |
|---------|---------|-----------|
| 2 (RX)  | TX Arduino | Data terima |
| 3 (TX)  | RX Arduino | Data kirim |
| 5 (GND) | GND Arduino | Ground |

### Konfigurasi RS-232

File `config/gate.ini`:
```ini
[Gate]
# Port utama untuk loop detector
COM_Port=COM3
Baud_Rate=9600
Data_Bits=8
Stop_Bits=1
Parity=None
Flow_Control=None

[Gate_Control]
# Port terpisah untuk kontrol gerbang (opsional)
Separate_Port=true
Control_COM_Port=COM4

[Timing]
# Timing untuk operasi gerbang (dalam detik)
Open_Delay=3
Close_Delay=5
Timeout=30
```

### Flow Control

1. **Hardware Flow Control**
   ```ini
   Flow_Control=Hardware
   ```
   - Menggunakan pin RTS/CTS
   - Lebih handal untuk data rate tinggi
   - Direkomendasikan untuk koneksi RS-232 langsung

2. **Software Flow Control**
   ```ini
   Flow_Control=Software
   ```
   - Menggunakan karakter XON/XOFF
   - Cocok untuk koneksi USB-to-Serial
   - Tidak memerlukan pin tambahan

### Protokol Komunikasi

1. **Format Pesan**
   - Setiap pesan diakhiri dengan newline (\n)
   - Pesan harus persis sesuai dengan format yang ditentukan
   - Tidak ada spasi tambahan di awal atau akhir pesan

2. **Pesan dari Loop Detector**
   - `VEHICLE_DETECTED`: Kendaraan terdeteksi
   - `NO_VEHICLE`: Tidak ada kendaraan
   - `GATE_STATUS: OPEN`: Status gerbang terbuka
   - `GATE_STATUS: CLOSED`: Status gerbang tertutup

3. **Pesan ke Loop Detector**
   - `OPEN_GATE`: Perintah buka gerbang
   - `CLOSE_GATE`: Perintah tutup gerbang
   - `GET_STATUS`: Minta status terbaru

### Pengujian Loop Detector

#### 1. Pengujian Koneksi RS-232
1. Pastikan konektor DB9 terhubung dengan benar
2. Periksa Device Manager untuk memastikan port COM terdeteksi
3. Gunakan aplikasi seperti PuTTY untuk menguji komunikasi serial
4. Verifikasi baud rate dan pengaturan RS-232 lainnya

#### 2. Pengujian Loop Detector
1. Mode Manual:
   - Gunakan tombol push untuk mensimulasikan kendaraan
   - Verifikasi LED indikator menyala saat kendaraan terdeteksi
   - Periksa pesan yang dikirim ke aplikasi

2. Mode Otomatis:
   - Aktifkan mode simulasi random
   - Verifikasi deteksi kendaraan acak
   - Periksa delay minimal antara deteksi (2 detik)

#### 3. Pengujian Kontrol Gerbang
1. Verifikasi perintah OPEN_GATE dan CLOSE_GATE
2. Periksa respons GATE_STATUS
3. Verifikasi timeout jika tidak ada respons
4. Test mode port terpisah untuk kontrol gerbang

### Pemecahan Masalah RS-232

#### 1. Port COM Tidak Terdeteksi
- Periksa koneksi fisik konektor DB9
- Verifikasi driver USB-to-Serial terinstal:
  1. Buka Device Manager
  2. Cari "Ports (COM & LPT)"
  3. Jika ada "Unknown Device" atau "Other Device":
     - Download driver sesuai chipset (CH340/FT232R/CP210x)
     - Instal driver dan restart komputer
  4. Pastikan port COM muncul di Device Manager
- Coba port COM berbeda
- Periksa kabel USB-to-Serial

#### 2. Komunikasi Gagal
- Verifikasi baud rate (9600)
- Periksa pengaturan data bits (8), stop bits (1), parity (None)
- Pastikan flow control sesuai
- Cek koneksi ground (GND)
- Jika menggunakan MAX232:
  1. Pastikan tegangan supply 5V stabil
  2. Verifikasi koneksi pin VCC dan GND
  3. Cek koneksi TX/RX tidak terbalik

#### 3. Data Berulang atau Noise
- Periksa koneksi ground
- Gunakan kabel RS-232 yang lebih pendek
- Tambahkan kapasitor untuk filter noise
- Verifikasi debouncing di mikrokontroler
- Jika menggunakan MAX232:
  1. Tambahkan kapasitor 0.1µF antara VCC dan GND
  2. Gunakan kabel shielded untuk TX/RX
  3. Hindari kabel paralel dengan sumber noise

#### 4. Timeout atau Tidak Ada Respons
- Periksa koneksi TX/RX terbalik
- Verifikasi format pesan
- Cek timeout settings di aplikasi
- Test dengan aplikasi serial monitor
- Jika menggunakan MAX232:
  1. Verifikasi level tegangan output (sekitar ±5V)
  2. Cek koneksi pin TX/RX di kedua sisi
  3. Pastikan ground terhubung dengan benar

## Konfigurasi Database

1. Database:
   - Server: localhost
   - Port: 3306
   - Database: db_parking
   - Username default: admin
   - Password default: admin123

2. Folder Struktur:
   - /Images - Untuk menyimpan gambar kendaraan
   - /Logs - File log aplikasi
   - /Config - File konfigurasi

## Troubleshooting Umum

Jika mengalami masalah:

1. Pastikan .NET SDK 6.0 terinstall:
```bash
dotnet --version
```

2. Cek status MySQL:
```bash
net start MySQL80
```

3. Periksa file log di folder Logs untuk detail error

## Kontribusi

Silakan berkontribusi dengan membuat pull request atau melaporkan issues.

## Lisensi

MIT License 