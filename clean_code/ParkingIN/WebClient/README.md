# ParkingIN Web Client

Aplikasi web sederhana untuk memantau status sistem ParkingIN secara real-time.

## Fitur

- Melihat status perangkat secara detail (gate controller, kamera, printer, loop detector)
- Memantau statistik parkir secara real-time
- Melihat log aktivitas kendaraan masuk dan keluar
- Pemberitahuan langsung saat kendaraan masuk atau keluar
- Monitor status gerbang (buka/tutup)
- Informasi detail perangkat:
  - Kamera: resolusi, FPS, waktu frame terakhir
  - Gate: posisi, aksi terakhir, jumlah error
  - Printer: status kertas, pesan error, waktu cetak terakhir
  - Loop Detector: sensitivitas, jumlah error, waktu deteksi terakhir
- Penanganan error otomatis dengan mekanisme retry
- Reconnect otomatis untuk koneksi yang terputus
- Logging komprehensif untuk troubleshooting

## Cara Menggunakan

1. Pastikan aplikasi server ParkingIN berjalan terlebih dahulu
2. Buka file `index.html` di browser web apa pun
3. Aplikasi akan otomatis terhubung ke server WebSocket (port 8181 secara default)

## Konfigurasi

### WebSocket Server

Alamat server WebSocket dapat dikonfigurasi melalui file `config/websocket.ini`:

```ini
[WebSocket]
Address=ws://localhost:8181
```

### Perangkat

#### Kamera
```ini
[Camera]
Resolution=1920x1080
FPS=30
```

#### Gate Controller
```ini
[Gate]
COM_Port=COM3
Baud_Rate=9600
Data_Bits=8
Stop_Bits=1
Parity=None
```

#### Printer
```ini
[Printer]
Name=PrinterName
```

#### Loop Detector
```ini
[LoopDetector]
Sensitivity=Medium
```

## Protokol Komunikasi

### WebSocket Messages

1. Vehicle Entry Notification:
```json
{
    "type": "vehicle_entry",
    "licensePlate": "ABC123",
    "vehicleType": "Car",
    "ticketNumber": "TICKET_20240315123456",
    "entryTime": "2024-03-15 12:34:56",
    "timestamp": "2024-03-15 12:34:56",
    "status": {
        "gate": "opening",
        "camera": "active",
        "printer": "ready"
    }
}
```

2. System Status:
```json
{
    "type": "system_status",
    "status": {
        "server": "online",
        "time": "2024-03-15 12:34:56",
        "db_connection": "connected",
        "devices": [
            {
                "name": "camera",
                "status": "active",
                "last_frame": "2024-03-15 12:34:56",
                "resolution": "1920x1080",
                "fps": 30
            },
            // ... other devices
        ]
    },
    "statistics": {
        "vehicles_inside": 10,
        "today_entries": 25,
        "active_connections": 3,
        "total_connections": 5
    }
}
```

### Loop Detector Protocol

Mikrokontroler ATmega mengirimkan data dengan format:
- `VEHICLE_DETECTED`: Kendaraan terdeteksi
- `NO_VEHICLE`: Tidak ada kendaraan
- `LOOP_DETECTOR_ERROR`: Error pada loop detector

## Penanganan Error

Sistem menerapkan mekanisme retry untuk error yang dapat dipulihkan:
1. Network errors (timeout, connection reset)
2. WebSocket state errors
3. Temporary device communication issues

Maksimal 3 kali percobaan untuk setiap operasi yang gagal.

## Logging

Log sistem disimpan di direktori `logs/`:
- `websocket.log`: Log komunikasi WebSocket
- `gate.log`: Log operasi gate
- `printer.log`: Log operasi printer
- `loop_detector.log`: Log deteksi kendaraan

## Persyaratan Sistem

- Browser modern yang mendukung WebSocket (Chrome, Firefox, Edge, Safari, dll.)
- Koneksi jaringan ke server ParkingIN
- Port COM yang tersedia untuk koneksi loop detector (opsional)
- Akses ke direktori logs untuk monitoring
- Akses ke direktori config untuk konfigurasi 