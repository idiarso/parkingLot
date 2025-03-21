-- Drop database if exists and create new one
DROP DATABASE IF EXISTS parkirdb;
CREATE DATABASE parkirdb WITH ENCODING = 'UTF8';

\c parkirdb

-- Create tables if they don't exist
CREATE TABLE IF NOT EXISTS t_user (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(50) NOT NULL,
    nama VARCHAR(100),
    role VARCHAR(20),
    status BOOLEAN DEFAULT true
);

-- Create default admin user if not exists
INSERT INTO t_user (username, password, nama, role, status)
SELECT 'admin', 'admin', 'Administrator', 'ADMIN', true
WHERE NOT EXISTS (
    SELECT 1 FROM t_user WHERE username = 'admin'
);

CREATE TABLE IF NOT EXISTS t_kendaraan (
    id SERIAL PRIMARY KEY,
    no_polisi VARCHAR(20) NOT NULL,
    jenis VARCHAR(50),
    waktu_masuk TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    waktu_keluar TIMESTAMP,
    biaya DECIMAL(10,2),
    status INTEGER DEFAULT 1
);

CREATE TABLE IF NOT EXISTS t_setting (
    id SERIAL PRIMARY KEY,
    setting_key VARCHAR(50) NOT NULL UNIQUE,
    setting_value TEXT,
    description TEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_by INTEGER REFERENCES t_user(id)
);

-- Insert default settings if not exists
INSERT INTO t_setting (setting_key, setting_value, description)
VALUES 
    ('APP_NAME', 'Modern Parking System', 'Nama aplikasi'),
    ('COMPANY_NAME', 'PT Parkir Jaya', 'Nama perusahaan'),
    ('GRACE_PERIOD_MINUTES', '10', 'Periode tenggang dalam menit'),
    ('PRINT_RECEIPT', 'true', 'Cetak struk'),
    ('CAPTURE_PHOTO', 'true', 'Ambil foto kendaraan'),
    ('BACKUP_PATH', 'D:\Backup', 'Path backup database')
ON CONFLICT (setting_key) DO NOTHING; 