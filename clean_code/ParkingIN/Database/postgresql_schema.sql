-- Parking Management System Database Schema for PostgreSQL
-- This script creates the basic database structure for the parking management system

-- Create tables if they don't exist
CREATE TABLE IF NOT EXISTS t_user (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(50) NOT NULL,
    nama VARCHAR(100),
    role VARCHAR(20),
    email VARCHAR(100),
    last_login TIMESTAMP,
    status BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create default admin user if not exists
INSERT INTO t_user (username, password, nama, role, email, status)
SELECT 'admin', 'admin123', 'Administrator', 'ADMIN', 'admin@example.com', true
WHERE NOT EXISTS (SELECT 1 FROM t_user WHERE username = 'admin');

-- Vehicle table
CREATE TABLE IF NOT EXISTS t_kendaraan (
    id SERIAL PRIMARY KEY,
    no_polisi VARCHAR(20) NOT NULL,
    jenis VARCHAR(50),
    waktu_masuk TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    waktu_keluar TIMESTAMP,
    biaya DECIMAL(10,2),
    status INTEGER DEFAULT 1
);

-- Settings table
CREATE TABLE IF NOT EXISTS t_setting (
    id SERIAL PRIMARY KEY,
    setting_key VARCHAR(50) NOT NULL UNIQUE,
    setting_value TEXT,
    description TEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by INTEGER REFERENCES t_user(id)
);

-- Member table
CREATE TABLE IF NOT EXISTS t_member (
    id SERIAL PRIMARY KEY,
    nomor_kartu VARCHAR(50) NOT NULL UNIQUE,
    nama VARCHAR(100) NOT NULL,
    alamat TEXT,
    telepon VARCHAR(20),
    email VARCHAR(100),
    jenis_kendaraan VARCHAR(50),
    status BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expired_at TIMESTAMP
);

-- Parking table
CREATE TABLE IF NOT EXISTS t_parkir (
    id SERIAL PRIMARY KEY,
    no_tiket VARCHAR(50) NOT NULL UNIQUE,
    no_polisi VARCHAR(20) NOT NULL,
    jenis_kendaraan VARCHAR(50),
    waktu_masuk TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    waktu_keluar TIMESTAMP,
    durasi INTEGER,
    biaya DECIMAL(10,2),
    member_id INTEGER REFERENCES t_member(id),
    status INTEGER DEFAULT 1,
    created_by INTEGER REFERENCES t_user(id),
    updated_by INTEGER REFERENCES t_user(id)
);

-- Shift table
CREATE TABLE IF NOT EXISTS t_shift (
    id SERIAL PRIMARY KEY,
    nama VARCHAR(50) NOT NULL,
    jam_mulai TIME NOT NULL,
    jam_selesai TIME NOT NULL,
    status BOOLEAN DEFAULT true
);

-- Rate table
CREATE TABLE IF NOT EXISTS t_tarif (
    id SERIAL PRIMARY KEY,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    tarif_awal DECIMAL(10,2) NOT NULL,
    tarif_berikutnya DECIMAL(10,2) NOT NULL,
    masa_berlaku_awal INTEGER NOT NULL, -- in minutes
    interval_berikutnya INTEGER NOT NULL, -- in minutes
    status BOOLEAN DEFAULT true,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Log table
CREATE TABLE IF NOT EXISTS t_log (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES t_user(id),
    action VARCHAR(50) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Insert default settings if not exists
INSERT INTO t_setting (setting_key, setting_value, description)
SELECT 'APP_NAME', 'Modern Parking System', 'Application name'
WHERE NOT EXISTS (SELECT 1 FROM t_setting WHERE setting_key = 'APP_NAME');

INSERT INTO t_setting (setting_key, setting_value, description)
SELECT 'COMPANY_NAME', 'PT Parkir Jaya', 'Company name'
WHERE NOT EXISTS (SELECT 1 FROM t_setting WHERE setting_key = 'COMPANY_NAME');

INSERT INTO t_setting (setting_key, setting_value, description)
SELECT 'GRACE_PERIOD_MINUTES', '10', 'Grace period in minutes'
WHERE NOT EXISTS (SELECT 1 FROM t_setting WHERE setting_key = 'GRACE_PERIOD_MINUTES');

INSERT INTO t_setting (setting_key, setting_value, description)
SELECT 'PRINT_RECEIPT', 'true', 'Enable receipt printing'
WHERE NOT EXISTS (SELECT 1 FROM t_setting WHERE setting_key = 'PRINT_RECEIPT');

INSERT INTO t_setting (setting_key, setting_value, description)
SELECT 'CAPTURE_PHOTO', 'true', 'Enable photo capture'
WHERE NOT EXISTS (SELECT 1 FROM t_setting WHERE setting_key = 'CAPTURE_PHOTO');

INSERT INTO t_setting (setting_key, setting_value, description)
SELECT 'BACKUP_PATH', 'D:\Backup', 'Path for database backups'
WHERE NOT EXISTS (SELECT 1 FROM t_setting WHERE setting_key = 'BACKUP_PATH');
