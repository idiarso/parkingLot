# Diagram Alur Sistem Parkir

## Komponen Sistem

```
+------------------+      +------------------+      +------------------+
|                  |      |                  |      |                  |
|    ParkingIN     |<---->|  ParkingServer   |<---->|    ParkingOUT    |
|    (Entry)       |      | (WebSocket/DB)   |      |    (Exit)        |
|                  |      |                  |      |                  |
+--------^---------+      +------------------+      +--------^---------+
         |                                                   |
         |                                                   |
+--------v---------+                                +--------v---------+
|                  |                                |                  |
| Mikrokontroler   |                                | Mikrokontroler   |
|    ATMEL         |                                |    ATMEL         |
| (Entry Gate)     |                                | (Exit Gate)      |
|                  |                                |                  |
+--------^---------+                                +--------^---------+
         |                                                   |
         |                                                   |
+--------v---------+      +------------------+      +--------v---------+
|                  |      |                  |      |                  |
|  Push Button     |      |   PostgreSQL     |      |  Barcode Scanner |
|  & Sensor Masuk  |      |    Database      |      |  & Printer Keluar|
|                  |      |                  |      |                  |
+--------^---------+      +------------------+      +--------^---------+
         |                                                   |
         |                                                   |
+--------v---------+                                +--------v---------+
|                  |                                |                  |
|    Kamera        |                                |  Gate Barrier    |
|  (Entry Point)   |                                |  (Exit Point)    |
|                  |                                |                  |
+------------------+                                +------------------+
```

## Alur Proses Sistem

### Proses Masuk (Entry)

```
+---------------+     +---------------+     +---------------+
| Kendaraan     |     | Push Button   |     | ATMEL MCU     |
| Terdeteksi    |---->| Ditekan       |---->| Kirim Sinyal  |
|               |     |               |     | "IN:<ID>"     |
+---------------+     +---------------+     +-------+-------+
                                                    |
                                                    v
+---------------+     +---------------+     +---------------+
| Gambar        |     | Data          |     | ParkingIN     |
| Disimpan      |<----| Diproses      |<----| Terima Sinyal |
|               |     |               |     |               |
+-------+-------+     +-------+-------+     +---------------+
        |                     |
        v                     v
+---------------+     +---------------+     +---------------+
| Tiket Dengan  |     | Portal        |     | Kendaraan     |
| Barcode       |---->| Terbuka       |---->| Masuk         |
| Dicetak       |     |               |     |               |
+---------------+     +---------------+     +---------------+
```

### Proses Keluar (Exit)

```
+---------------+     +---------------+     +---------------+
| Kendaraan     |     | Barcode Tiket |     | ParkingOUT    |
| di Exit Gate  |---->| Dipindai      |---->| Proses Data   |
|               |     |               |     |               |
+---------------+     +---------------+     +-------+-------+
                                                    |
                                                    v
+---------------+     +---------------+     +---------------+
| Petugas       |     | Sistem        |     | Data Parkir   |
| Verifikasi    |<----| Tampilkan     |<----| & Foto        |
| Kendaraan     |     | Foto Entry    |     | Diambil       |
+-------+-------+     +---------------+     +---------------+
        |
        v
+---------------+     +---------------+     +---------------+
| Transaksi     |     | Tombol        |     | Sinyal Buka   |
| Pembayaran    |---->| "Buka Gate"   |---->| Dikirim ke MCU|
| (jika ada)    |     | Ditekan       |     |               |
+---------------+     +---------------+     +-------+-------+
                                                    |
                                                    v
+---------------+     +---------------+     +---------------+
| Struk         |     | Portal Exit   |     | Kendaraan     |
| Pembayaran    |---->| Terbuka       |---->| Keluar        |
| Dicetak       |     |               |     |               |
+---------------+     +---------------+     +---------------+
```

## Rincian Komponen

### Hardware
- **PC Server**: Menjalankan PostgreSQL dan ParkingServer
- **PC Entry**: Menjalankan ParkingIN
- **PC Exit**: Menjalankan ParkingOUT
- **ATMEL MCU**: Mikrokontroler untuk komunikasi dengan push button dan gate barrier
- **Printer Thermal**: Untuk mencetak tiket dan struk
- **Kamera**: Webcam atau Kamera IP untuk pengambilan gambar kendaraan
- **Barcode Scanner**: Untuk memindai tiket masuk
- **Push Button**: Memicu proses cetak tiket dan pengambilan gambar
- **Gate Barrier**: Portal otomatis untuk entry dan exit

### Software
- **ParkingIN**: Aplikasi untuk mengelola proses masuk kendaraan
- **ParkingOUT**: Aplikasi untuk mengelola proses keluar kendaraan
- **ParkingServer**: WebSocket server untuk komunikasi real-time
- **PostgreSQL**: Database untuk menyimpan data transaksi, pengguna, dan log

## Alur Data
1. Data kendaraan masuk disimpan di database dengan ID unik
2. Foto kendaraan disimpan dengan nama sesuai ID unik
3. ID unik dicetak dalam bentuk barcode pada tiket
4. Saat kendaraan keluar, data diambil berdasarkan ID dari barcode
5. Foto kendaraan saat masuk ditampilkan untuk verifikasi
6. Setelah verifikasi, gate dibuka dan data transaksi diupdate
