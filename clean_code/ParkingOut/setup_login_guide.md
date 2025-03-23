# Panduan Implementasi Login dengan PostgreSQL di ParkingOut

## Overview
Dokumen ini memberikan panduan langkah demi langkah untuk mengimplementasikan dan menguji sistem login dengan database PostgreSQL di aplikasi ParkingOut.

## Persiapan Database

1. **Pastikan PostgreSQL sudah terinstal dan berjalan**
   - PostgreSQL harus dijalankan di port 5432 (default)
   - Pastikan memiliki user `postgres` dengan password yang benar (default: `root@rsi`)

2. **Buat database `parkirdb`**
   ```sql
   CREATE DATABASE parkirdb;
   ```

3. **Buat tabel `users` dengan menjalankan script SQL berikut**
   ```sql
   CREATE TABLE IF NOT EXISTS users (
      id SERIAL PRIMARY KEY,
      username VARCHAR(50) NOT NULL UNIQUE,
      password VARCHAR(255) NOT NULL,
      nama VARCHAR(100),
      role VARCHAR(20) DEFAULT 'Operator',
      level VARCHAR(20),
      last_login TIMESTAMP,
      status BOOLEAN DEFAULT TRUE,
      created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
      updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
   );
   ```

4. **Tambahkan user admin default**
   ```sql
   -- Hash SHA-256 untuk password123: ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f
   INSERT INTO users (username, password, nama, role, level, status)
   VALUES ('admin', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Administrator', 'Admin', 'Super', TRUE)
   ON CONFLICT (username) DO NOTHING;
   ```

## Koneksi Database di Aplikasi

1. **Verifikasi Pengaturan Koneksi Database**
   - Pastikan ConnectionString di `App.config` atau `config.json` sudah benar:
   ```
   Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi;
   ```

2. **Modifikasi yang Sudah Dilakukan**:
   - `UserManager.cs`: Diperbarui untuk menangani autentikasi dengan database PostgreSQL
   - `LoginForm.cs`: Diperbarui untuk menggunakan UserManager.AuthenticateAsync
   - `App.xaml.cs`: Diperbarui untuk memastikan tabel users dibuat saat startup

## Pengujian Login

1. **Kredensial Login Default**:
   - Username: `admin`
   - Password: `password123`

2. **Langkah Pengujian**:
   - Jalankan aplikasi ParkingOut
   - Masukkan username dan password default
   - Klik tombol "Login"
   - Sistem akan memverifikasi kredensial dengan database
   - Jika berhasil, aplikasi akan menampilkan halaman utama

## Troubleshooting

1. **Jika Login Gagal**:
   - Periksa apakah PostgreSQL berjalan dengan benar
   - Verifikasi database parkirdb sudah dibuat
   - Pastikan tabel users sudah dibuat dengan benar
   - Verifikasi user admin sudah ada dalam tabel users

2. **Untuk Memeriksa Tabel Users**:
   ```sql
   SELECT * FROM users;
   ```

3. **Untuk Reset Password Admin**:
   ```sql
   -- Hash SHA-256 untuk password123: ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f
   UPDATE users SET password = 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f' WHERE username = 'admin';
   ```

## Aplikasi ParkingOut

- Aplikasi ParkingOut adalah sistem manajemen parkir modern yang menggunakan database PostgreSQL untuk autentikasi pengguna
- User admin dapat mengelola semua fitur aplikasi
- Operator memiliki akses terbatas untuk operasi sehari-hari
