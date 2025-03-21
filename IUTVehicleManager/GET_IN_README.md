# IUT Vehicle Manager - GET IN System Documentation

## Arsitektur Sistem

### Konfigurasi Hardware pada PC GET IN
```
[Mikrokontroler] <---> [PC GET IN] <---> [Printer Termal]
   (COM1/COM2)           (PC Hub)         (COM3/COM4)
```

### Koneksi Port pada PC GET IN
- Mikrokontroler: Menggunakan port COM terpisah (default: COM1)
- Printer Termal: Menggunakan port COM berbeda (default: COM2)

## Alur Komunikasi Sistem

```
[Sensor/RFID] -> [Mikrokontroler] -> [PC GET IN] -> [Printer]
    Deteksi        Serial COM1        Proses        Serial COM2
```

## Proses pada PC GET IN

1. **Penerimaan Data**
   - Menerima trigger dari mikrokontroler (COM1)
   - Format data: "IN:VEHICLEID" (contoh: "IN:B1234CD")
   - Validasi format data yang diterima

2. **Pemrosesan Data**
   - Ekstrak Vehicle ID
   - Update display status
   - Set priority level
   - Record ke history

3. **Output**
   - Cetak tiket via printer (COM2)
   - Kirim acknowledgment ke MCU
   - Format acknowledgment: "ACK:VEHICLEID"

## Format Data Komunikasi

### Input dari MCU
```
Format: "IN:VEHICLEID"
Contoh: "IN:B1234CD"
```

### Output ke MCU (Acknowledgment)
```
Format: "ACK:VEHICLEID"
Contoh: "ACK:B1234CD"
```

## Fitur Khusus GET IN

1. **Manajemen Kendaraan**
   - Otomatis set priority level
   - Deteksi tipe kendaraan
   - Validasi ID kendaraan

2. **Pencetakan Tiket**
   - Format tiket khusus masuk
   - Termasuk barcode (opsional)
   - Auto-cut setelah print

3. **Monitoring**
   - Status printer dan MCU
   - History kendaraan masuk
   - Last trigger display

## Keamanan Sistem

1. **Validasi Data**
   - Cek format data dari MCU
   - Validasi Vehicle ID
   - Verifikasi koneksi

2. **Error Handling**
   - Deteksi kesalahan komunikasi
   - Backup data transaksi
   - Recovery mechanism

3. **Monitoring Status**
   - Status koneksi real-time
   - Log error dan events
   - Backup data otomatis

## Setup dan Konfigurasi

### Setup Hardware
```
1. Hubungkan Mikrokontroler ke COM1
2. Hubungkan Printer Termal ke COM2
3. Pastikan power supply cukup untuk kedua device
```

### Konfigurasi Software
1. **File Konfigurasi** (`getin_config.json`)
   - Station settings
   - Printer settings
   - MCU settings
   - Entry settings

2. **Port Settings**
   - Baud Rate: 9600
   - Data Bits: 8
   - Parity: None
   - Stop Bits: 1

## Operasional Sistem

### Langkah Start-up
```
1. Jalankan aplikasi IUT Vehicle Manager
2. Pilih COM1 untuk MCU dan COM2 untuk printer
3. Klik "Connect" pada kedua device
4. Sistem siap menerima trigger
```

### Monitoring Operasional
```
1. Cek status koneksi (hijau = connected)
2. Pantau "Last Trigger" untuk aktivitas terakhir
3. Lihat history untuk semua transaksi
```

## Troubleshooting

### Masalah Koneksi
- MCU tidak terdeteksi: Cek COM1
- Printer tidak respon: Cek COM2
- Data tidak valid: Cek format trigger dari MCU
- Print gagal: Cek koneksi printer dan kertas

### Solusi Umum
1. **Masalah MCU**
   - Restart MCU
   - Cek kabel koneksi
   - Verifikasi power supply

2. **Masalah Printer**
   - Cek kertas printer
   - Reset printer
   - Cek status error

3. **Masalah Software**
   - Restart aplikasi
   - Cek log error
   - Verifikasi konfigurasi

## Maintenance

### Maintenance Harian
- Cek status koneksi
- Verifikasi print quality
- Monitor log transaksi

### Maintenance Mingguan
- Backup data transaksi
- Cek error logs
- Bersihkan printer

### Maintenance Bulanan
- Update konfigurasi
- Analisis performa
- Optimasi sistem

## Kontak Support

Untuk bantuan teknis, hubungi:
- Technical Support: support@iut.com
- Emergency Contact: emergency@iut.com
- System Admin: admin@iut.com 