-- PostgreSQL Schema for Parking Management System
-- Compatible with ParkingOut application

-- Drop tables if they exist (with CASCADE to handle dependencies)
DROP TABLE IF EXISTS t_pendapatan_harian CASCADE;
DROP TABLE IF EXISTS t_parkir CASCADE;
DROP TABLE IF EXISTS m_member CASCADE;
DROP TABLE IF EXISTS t_tarif CASCADE;
DROP TABLE IF EXISTS tarif_khusus CASCADE;
DROP TABLE IF EXISTS t_user CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS settings CASCADE;

-- Create Users table (standard table name used in the code)
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    password VARCHAR(255) NOT NULL,
    nama VARCHAR(100) DEFAULT NULL,
    role VARCHAR(20) DEFAULT 'operator',
    last_login TIMESTAMP DEFAULT NULL,
    status BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT username_unique UNIQUE (username)
);

-- Create alternative user table (t_user) for backward compatibility
CREATE TABLE IF NOT EXISTS t_user (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    nama VARCHAR(100) NOT NULL,
    role VARCHAR(20) DEFAULT 'OPERATOR',
    email VARCHAR(100),
    last_login TIMESTAMP DEFAULT NULL,
    status BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Create Parking Rates table
CREATE TABLE IF NOT EXISTS t_tarif (
    id SERIAL PRIMARY KEY,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    tarif_perjam DECIMAL(10,2) NOT NULL,
    tarif_maksimal DECIMAL(10,2) DEFAULT NULL,
    denda_tiket_hilang DECIMAL(10,2) DEFAULT 25000.00,
    grace_period INTEGER DEFAULT 10,
    status BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT jenis_kendaraan_unique UNIQUE (jenis_kendaraan)
);

-- Create Member Cards table
CREATE TABLE IF NOT EXISTS m_member (
    member_id SERIAL PRIMARY KEY,
    nomor_kartu VARCHAR(50) NOT NULL UNIQUE,
    nama_pemilik VARCHAR(100) NOT NULL,
    no_ktp VARCHAR(20) NOT NULL,
    alamat VARCHAR(200) NULL,
    no_telp VARCHAR(20) NULL,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    no_polisi VARCHAR(20) NOT NULL,
    tanggal_daftar TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    tanggal_expired TIMESTAMP NOT NULL,
    aktif BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Parking Transactions table
CREATE TABLE IF NOT EXISTS t_parkir (
    id SERIAL PRIMARY KEY,
    nomor_polisi VARCHAR(20) NOT NULL,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    waktu_masuk TIMESTAMP NOT NULL,
    waktu_keluar TIMESTAMP DEFAULT NULL,
    durasi INTEGER DEFAULT NULL,
    biaya DECIMAL(10,2) DEFAULT NULL,
    foto_masuk VARCHAR(255) DEFAULT NULL,
    foto_keluar VARCHAR(255) DEFAULT NULL,
    keterangan VARCHAR(255) DEFAULT NULL,
    status_tiket VARCHAR(20) DEFAULT 'NORMAL',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
CREATE INDEX IF NOT EXISTS idx_nomor_polisi ON t_parkir(nomor_polisi);

-- Create Daily Revenue table
CREATE TABLE IF NOT EXISTS t_pendapatan_harian (
    pendapatan_id SERIAL PRIMARY KEY,
    tanggal DATE NOT NULL UNIQUE,
    total_kendaraan_masuk INT NOT NULL DEFAULT 0,
    total_kendaraan_keluar INT NOT NULL DEFAULT 0,
    total_pendapatan DECIMAL(12,2) NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create settings table
CREATE TABLE IF NOT EXISTS settings (
  id SERIAL PRIMARY KEY,
  setting_key VARCHAR(100) NOT NULL,
  setting_value VARCHAR(255) NOT NULL,
  deskripsi VARCHAR(255) DEFAULT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT setting_key_unique UNIQUE (setting_key)
);

-- Tabel tarif_khusus untuk menyimpan tarif khusus berdasarkan waktu
CREATE TABLE IF NOT EXISTS tarif_khusus (
  id SERIAL PRIMARY KEY,
  jenis_kendaraan VARCHAR(50) NOT NULL,
  jenis_tarif VARCHAR(50) NOT NULL,
  nilai_tarif DECIMAL(10,2) NOT NULL,
  aktif BOOLEAN DEFAULT TRUE,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create Function to calculate parking fee (replacing stored procedure)
CREATE OR REPLACE FUNCTION hitung_biaya_parkir(p_parkir_id INT)
RETURNS TABLE(durasi_menit INT, biaya DECIMAL) AS $$
DECLARE
    v_jam_masuk TIMESTAMP;
    v_jam_keluar TIMESTAMP;
    v_jenis_kendaraan VARCHAR(50);
    v_tarif_perjam DECIMAL(10,2);
    v_tarif_maksimal DECIMAL(10,2);
    v_grace_period INT;
    v_biaya DECIMAL(10,2);
    v_durasi_menit INT;
    v_nomor_kartu_member VARCHAR(50);
BEGIN
    -- Get parking data
    SELECT waktu_masuk, COALESCE(waktu_keluar, NOW()),
           jenis_kendaraan, nomor_polisi
    INTO v_jam_masuk, v_jam_keluar, v_jenis_kendaraan, v_nomor_kartu_member
    FROM t_parkir
    WHERE id = p_parkir_id;
    
    -- Calculate duration in minutes
    v_durasi_menit = EXTRACT(EPOCH FROM (v_jam_keluar - v_jam_masuk))/60;
    
    -- Check if member
    IF v_nomor_kartu_member IS NOT NULL AND EXISTS(
        SELECT 1 FROM m_member 
        WHERE nomor_kartu = v_nomor_kartu_member 
        AND aktif = TRUE 
        AND tanggal_expired > NOW()
    ) THEN
        -- Member gets 50% discount
        SELECT tarif_perjam * 0.5,
               tarif_maksimal * 0.5,
               grace_period
        INTO v_tarif_perjam, v_tarif_maksimal, v_grace_period
        FROM t_tarif
        WHERE jenis_kendaraan = v_jenis_kendaraan AND status = TRUE;
    ELSE
        -- Regular rate
        SELECT tarif_perjam,
               tarif_maksimal,
               grace_period
        INTO v_tarif_perjam, v_tarif_maksimal, v_grace_period
        FROM t_tarif
        WHERE jenis_kendaraan = v_jenis_kendaraan AND status = TRUE;
    END IF;
    
    -- Calculate fee
    IF v_durasi_menit <= v_grace_period THEN
        v_biaya = v_tarif_perjam;
    ELSE
        v_biaya = v_tarif_perjam + (CEIL((v_durasi_menit - v_grace_period)::FLOAT / 60) * v_tarif_perjam);
        -- Cap at maximum rate if applicable
        IF v_tarif_maksimal IS NOT NULL AND v_biaya > v_tarif_maksimal THEN
            v_biaya = v_tarif_maksimal;
        END IF;
    END IF;
    
    -- Update parking record
    UPDATE t_parkir
    SET waktu_keluar = v_jam_keluar,
        durasi = v_durasi_menit,
        biaya = v_biaya,
        updated_at = NOW()
    WHERE id = p_parkir_id;
    
    -- Return calculated values
    RETURN QUERY SELECT v_durasi_menit, v_biaya;
END;
$$ LANGUAGE plpgsql;

-- Create function to update daily revenue (replacing trigger)
CREATE OR REPLACE FUNCTION update_pendapatan_harian()
RETURNS TRIGGER AS $$
BEGIN
    -- For INSERT with no exit time (vehicle entry)
    IF TG_OP = 'INSERT' AND NEW.waktu_keluar IS NULL THEN
        -- Update or insert daily revenue for entry date
        INSERT INTO t_pendapatan_harian (tanggal, total_kendaraan_masuk)
        VALUES (DATE(NEW.waktu_masuk), 1)
        ON CONFLICT (tanggal) DO UPDATE 
        SET total_kendaraan_masuk = t_pendapatan_harian.total_kendaraan_masuk + 1,
            updated_at = NOW();
    END IF;
    
    -- For UPDATE with new exit information (vehicle exit)
    IF TG_OP = 'UPDATE' AND NEW.waktu_keluar IS NOT NULL AND OLD.waktu_keluar IS NULL THEN
        -- Update or insert daily revenue for exit date
        INSERT INTO t_pendapatan_harian (tanggal, total_kendaraan_keluar, total_pendapatan)
        VALUES (DATE(NEW.waktu_keluar), 1, COALESCE(NEW.biaya, 0))
        ON CONFLICT (tanggal) DO UPDATE 
        SET total_kendaraan_keluar = t_pendapatan_harian.total_kendaraan_keluar + 1,
            total_pendapatan = t_pendapatan_harian.total_pendapatan + COALESCE(NEW.biaya, 0),
            updated_at = NOW();
    END IF;
    
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Create trigger for update_pendapatan_harian
DROP TRIGGER IF EXISTS trg_update_pendapatan_harian ON t_parkir;
CREATE TRIGGER trg_update_pendapatan_harian
AFTER INSERT OR UPDATE ON t_parkir
FOR EACH ROW EXECUTE FUNCTION update_pendapatan_harian();

-- Insert default data
-- Default admin user (password: admin123)
INSERT INTO users (username, password, nama, role) 
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin')
ON CONFLICT (username) DO NOTHING;

INSERT INTO t_user (username, password, nama, role) 
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'ADMIN')
ON CONFLICT (username) DO NOTHING;

-- Default parking rates
INSERT INTO t_tarif (jenis_kendaraan, tarif_perjam, tarif_maksimal, denda_tiket_hilang)
VALUES 
    ('MOBIL', 5000.00, 50000.00, 25000.00),
    ('MOTOR', 2000.00, 20000.00, 10000.00)
ON CONFLICT (jenis_kendaraan) DO NOTHING;

-- Insert default settings
INSERT INTO settings (setting_key, setting_value, deskripsi) VALUES
('denda_tiket_hilang', '25000', 'Denda untuk tiket yang hilang'),
('refresh_interval', '60000', 'Interval refresh dalam milidetik'),
('long_parking_threshold', '120', 'Batas waktu parkir lama dalam menit'),
('critical_capacity', '90', 'Persentase kapasitas kritis'),
('warning_capacity', '75', 'Persentase kapasitas peringatan'),
('total_kapasitas', '100', 'Total kapasitas parkir')
ON CONFLICT (setting_key) DO NOTHING; 