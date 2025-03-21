-- Parking Management System Database Schema for MySQL
-- This script creates the basic database structure for the parking management system

-- Create tables
CREATE TABLE IF NOT EXISTS m_users (
    user_id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    nama_lengkap VARCHAR(100) NOT NULL,
    level VARCHAR(20) NOT NULL CHECK (level IN ('ADMIN', 'SUPERVISOR', 'OPERATOR')),
    aktif BOOLEAN DEFAULT 1,
    last_login DATETIME NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create default admin user (password: 123456, MD5: e10adc3949ba59abbe56e057f20f883e)
INSERT INTO m_users (username, password, nama_lengkap, level)
SELECT 'admin', 'e10adc3949ba59abbe56e057f20f883e', 'Administrator', 'ADMIN'
WHERE NOT EXISTS (SELECT 1 FROM m_users WHERE username = 'admin');

CREATE TABLE IF NOT EXISTS m_tarif (
    tarif_id INT AUTO_INCREMENT PRIMARY KEY,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    tarif_awal DECIMAL(10,2) NOT NULL,
    tarif_berikutnya DECIMAL(10,2) NOT NULL,
    masa_berlaku_awal INT NOT NULL,
    interval_berikutnya INT NOT NULL,
    aktif BOOLEAN DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Insert default rates if not exists
INSERT INTO m_tarif (jenis_kendaraan, tarif_awal, tarif_berikutnya, masa_berlaku_awal, interval_berikutnya)
SELECT 'Mobil', 5000, 2000, 120, 60
WHERE NOT EXISTS (SELECT 1 FROM m_tarif WHERE jenis_kendaraan = 'Mobil');

INSERT INTO m_tarif (jenis_kendaraan, tarif_awal, tarif_berikutnya, masa_berlaku_awal, interval_berikutnya)
SELECT 'Motor', 2000, 1000, 120, 60
WHERE NOT EXISTS (SELECT 1 FROM m_tarif WHERE jenis_kendaraan = 'Motor');

CREATE TABLE IF NOT EXISTS m_member (
    member_id INT AUTO_INCREMENT PRIMARY KEY,
    nomor_kartu VARCHAR(50) NOT NULL UNIQUE,
    nama_pemilik VARCHAR(100) NOT NULL,
    no_ktp VARCHAR(20) NOT NULL,
    alamat VARCHAR(200) NULL,
    no_telp VARCHAR(20) NULL,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    no_polisi VARCHAR(20) NOT NULL,
    tanggal_daftar DATETIME DEFAULT CURRENT_TIMESTAMP,
    tanggal_expired DATETIME NOT NULL,
    aktif BOOLEAN DEFAULT 1,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS t_parkir (
    parkir_id INT AUTO_INCREMENT PRIMARY KEY,
    nomor_tiket VARCHAR(50) NOT NULL UNIQUE,
    no_polisi VARCHAR(20) NOT NULL,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    jam_masuk DATETIME NOT NULL,
    jam_keluar DATETIME NULL,
    durasi_menit INT NULL,
    biaya DECIMAL(10,2) NULL,
    nomor_kartu_member VARCHAR(50) NULL,
    user_id_masuk INT NOT NULL,
    user_id_keluar INT NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    FOREIGN KEY (user_id_masuk) REFERENCES m_users(user_id),
    FOREIGN KEY (user_id_keluar) REFERENCES m_users(user_id)
);

CREATE TABLE IF NOT EXISTS t_pendapatan_harian (
    pendapatan_id INT AUTO_INCREMENT PRIMARY KEY,
    tanggal DATE NOT NULL UNIQUE,
    total_kendaraan_masuk INT NOT NULL DEFAULT 0,
    total_kendaraan_keluar INT NOT NULL DEFAULT 0,
    total_pendapatan DECIMAL(12,2) NOT NULL DEFAULT 0,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Create stored procedure to calculate parking fee
DELIMITER //

DROP PROCEDURE IF EXISTS sp_hitung_biaya_parkir //

CREATE PROCEDURE sp_hitung_biaya_parkir(IN p_parkir_id INT)
BEGIN
    DECLARE v_jam_masuk DATETIME;
    DECLARE v_jam_keluar DATETIME;
    DECLARE v_jenis_kendaraan VARCHAR(50);
    DECLARE v_tarif_awal DECIMAL(10,2);
    DECLARE v_tarif_berikutnya DECIMAL(10,2);
    DECLARE v_masa_berlaku_awal INT;
    DECLARE v_interval_berikutnya INT;
    DECLARE v_durasi_menit INT;
    DECLARE v_biaya DECIMAL(10,2);
    DECLARE v_nomor_kartu_member VARCHAR(50);
    
    -- Get parking data
    SELECT jam_masuk, COALESCE(jam_keluar, NOW()), jenis_kendaraan, nomor_kartu_member
    INTO v_jam_masuk, v_jam_keluar, v_jenis_kendaraan, v_nomor_kartu_member
    FROM t_parkir
    WHERE parkir_id = p_parkir_id;
    
    -- Calculate duration in minutes
    SET v_durasi_menit = TIMESTAMPDIFF(MINUTE, v_jam_masuk, v_jam_keluar);
    
    -- Check if member
    IF v_nomor_kartu_member IS NOT NULL AND EXISTS(
        SELECT 1 FROM m_member 
        WHERE nomor_kartu = v_nomor_kartu_member 
        AND aktif = 1 
        AND tanggal_expired > NOW()
    ) THEN
        -- Member gets 50% discount
        SELECT tarif_awal * 0.5, tarif_berikutnya * 0.5,
               masa_berlaku_awal, interval_berikutnya
        INTO v_tarif_awal, v_tarif_berikutnya,
             v_masa_berlaku_awal, v_interval_berikutnya
        FROM m_tarif
        WHERE jenis_kendaraan = v_jenis_kendaraan AND aktif = 1
        LIMIT 1;
    ELSE
        -- Regular rate
        SELECT tarif_awal, tarif_berikutnya,
               masa_berlaku_awal, interval_berikutnya
        INTO v_tarif_awal, v_tarif_berikutnya,
             v_masa_berlaku_awal, v_interval_berikutnya
        FROM m_tarif
        WHERE jenis_kendaraan = v_jenis_kendaraan AND aktif = 1
        LIMIT 1;
    END IF;
    
    -- Calculate fee
    IF v_durasi_menit <= v_masa_berlaku_awal THEN
        SET v_biaya = v_tarif_awal;
    ELSE
        SET v_biaya = v_tarif_awal + (CEILING((v_durasi_menit - v_masa_berlaku_awal) / v_interval_berikutnya) * v_tarif_berikutnya);
    END IF;
    
    -- Update parking record
    UPDATE t_parkir
    SET jam_keluar = v_jam_keluar,
        durasi_menit = v_durasi_menit,
        biaya = v_biaya,
        updated_at = NOW()
    WHERE parkir_id = p_parkir_id;
    
    -- Return calculated values
    SELECT v_durasi_menit AS durasi_menit, v_biaya AS biaya;
END //

DELIMITER ;

-- Create trigger to update daily revenue
DELIMITER //

DROP TRIGGER IF EXISTS trg_update_pendapatan_harian_insert //
DROP TRIGGER IF EXISTS trg_update_pendapatan_harian_update //

CREATE TRIGGER trg_update_pendapatan_harian_insert
AFTER INSERT ON t_parkir
FOR EACH ROW
BEGIN
    IF NEW.jam_keluar IS NULL THEN
        INSERT INTO t_pendapatan_harian (tanggal, total_kendaraan_masuk)
        VALUES (DATE(NEW.jam_masuk), 1)
        ON DUPLICATE KEY UPDATE
            total_kendaraan_masuk = total_kendaraan_masuk + 1,
            updated_at = NOW();
    END IF;
END //

CREATE TRIGGER trg_update_pendapatan_harian_update
AFTER UPDATE ON t_parkir
FOR EACH ROW
BEGIN
    IF NEW.jam_keluar IS NOT NULL AND OLD.jam_keluar IS NULL THEN
        INSERT INTO t_pendapatan_harian (tanggal, total_kendaraan_keluar, total_pendapatan)
        VALUES (DATE(NEW.jam_keluar), 1, IFNULL(NEW.biaya, 0))
        ON DUPLICATE KEY UPDATE
            total_kendaraan_keluar = total_kendaraan_keluar + 1,
            total_pendapatan = total_pendapatan + IFNULL(NEW.biaya, 0),
            updated_at = NOW();
    END IF;
END //

DELIMITER ; 