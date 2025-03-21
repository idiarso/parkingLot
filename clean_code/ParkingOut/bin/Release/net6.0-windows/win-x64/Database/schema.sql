-- Parking Management System Database Schema
-- This script creates the basic database structure for the parking management system

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = 'ParkingDB')
BEGIN
    CREATE DATABASE ParkingDB;
END
GO

USE ParkingDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='m_users' AND xtype='U')
BEGIN
    CREATE TABLE m_users (
        user_id INT IDENTITY(1,1) PRIMARY KEY,
        username VARCHAR(50) NOT NULL UNIQUE,
        password VARCHAR(100) NOT NULL, -- MD5 hash of password
        nama_lengkap VARCHAR(100) NOT NULL,
        level VARCHAR(20) NOT NULL CHECK (level IN ('ADMIN', 'SUPERVISOR', 'OPERATOR')),
        aktif BIT DEFAULT 1,
        last_login DATETIME NULL,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );
    
    -- Create default admin user (password: 123456, MD5: e10adc3949ba59abbe56e057f20f883e)
    INSERT INTO m_users (username, password, nama_lengkap, level)
    VALUES ('admin', 'e10adc3949ba59abbe56e057f20f883e', 'Administrator', 'ADMIN');
END
GO

-- Create Parking Rates table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='m_tarif' AND xtype='U')
BEGIN
    CREATE TABLE m_tarif (
        tarif_id INT IDENTITY(1,1) PRIMARY KEY,
        jenis_kendaraan VARCHAR(50) NOT NULL,
        tarif_awal DECIMAL(10,2) NOT NULL,
        tarif_berikutnya DECIMAL(10,2) NOT NULL,
        masa_berlaku_awal INT NOT NULL, -- in minutes
        interval_berikutnya INT NOT NULL, -- in minutes
        aktif BIT DEFAULT 1,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );
    
    -- Insert default rates
    INSERT INTO m_tarif (jenis_kendaraan, tarif_awal, tarif_berikutnya, masa_berlaku_awal, interval_berikutnya)
    VALUES 
        ('Mobil', 5000, 2000, 120, 60),  -- First 2 hours 5000, then 2000 per hour
        ('Motor', 2000, 1000, 120, 60);  -- First 2 hours 2000, then 1000 per hour
END
GO

-- Create Member Cards table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='m_member' AND xtype='U')
BEGIN
    CREATE TABLE m_member (
        member_id INT IDENTITY(1,1) PRIMARY KEY,
        nomor_kartu VARCHAR(50) NOT NULL UNIQUE,
        nama_pemilik VARCHAR(100) NOT NULL,
        no_ktp VARCHAR(20) NOT NULL,
        alamat VARCHAR(200) NULL,
        no_telp VARCHAR(20) NULL,
        jenis_kendaraan VARCHAR(50) NOT NULL,
        no_polisi VARCHAR(20) NOT NULL,
        tanggal_daftar DATETIME DEFAULT GETDATE(),
        tanggal_expired DATETIME NOT NULL,
        aktif BIT DEFAULT 1,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );
END
GO

-- Create Parking Transactions table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='t_parkir' AND xtype='U')
BEGIN
    CREATE TABLE t_parkir (
        parkir_id INT IDENTITY(1,1) PRIMARY KEY,
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
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE(),
        FOREIGN KEY (user_id_masuk) REFERENCES m_users(user_id),
        FOREIGN KEY (user_id_keluar) REFERENCES m_users(user_id)
    );
END
GO

-- Create Daily Revenue table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='t_pendapatan_harian' AND xtype='U')
BEGIN
    CREATE TABLE t_pendapatan_harian (
        pendapatan_id INT IDENTITY(1,1) PRIMARY KEY,
        tanggal DATE NOT NULL UNIQUE,
        total_kendaraan_masuk INT NOT NULL DEFAULT 0,
        total_kendaraan_keluar INT NOT NULL DEFAULT 0,
        total_pendapatan DECIMAL(12,2) NOT NULL DEFAULT 0,
        created_at DATETIME DEFAULT GETDATE(),
        updated_at DATETIME DEFAULT GETDATE()
    );
END
GO

-- Create stored procedure to calculate parking fee
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_hitung_biaya_parkir')
    DROP PROCEDURE sp_hitung_biaya_parkir
GO

CREATE PROCEDURE sp_hitung_biaya_parkir
    @parkir_id INT
AS
BEGIN
    DECLARE @jam_masuk DATETIME, @jam_keluar DATETIME, @jenis_kendaraan VARCHAR(50),
            @tarif_awal DECIMAL(10,2), @tarif_berikutnya DECIMAL(10,2),
            @masa_berlaku_awal INT, @interval_berikutnya INT,
            @durasi_menit INT, @biaya DECIMAL(10,2), @nomor_kartu_member VARCHAR(50)
    
    -- Get parking data
    SELECT @jam_masuk = jam_masuk, @jam_keluar = jam_keluar, 
           @jenis_kendaraan = jenis_kendaraan, @nomor_kartu_member = nomor_kartu_member
    FROM t_parkir
    WHERE parkir_id = @parkir_id
    
    -- If exit time is NULL, use current time
    IF @jam_keluar IS NULL
        SET @jam_keluar = GETDATE()
    
    -- Calculate duration in minutes
    SET @durasi_menit = DATEDIFF(MINUTE, @jam_masuk, @jam_keluar)
    
    -- Check if member
    IF @nomor_kartu_member IS NOT NULL AND EXISTS(
        SELECT 1 FROM m_member 
        WHERE nomor_kartu = @nomor_kartu_member 
        AND aktif = 1 
        AND tanggal_expired > GETDATE()
    )
    BEGIN
        -- Member gets 50% discount
        SELECT @tarif_awal = tarif_awal * 0.5,
               @tarif_berikutnya = tarif_berikutnya * 0.5,
               @masa_berlaku_awal = masa_berlaku_awal,
               @interval_berikutnya = interval_berikutnya
        FROM m_tarif
        WHERE jenis_kendaraan = @jenis_kendaraan AND aktif = 1
    END
    ELSE
    BEGIN
        -- Regular rate
        SELECT @tarif_awal = tarif_awal,
               @tarif_berikutnya = tarif_berikutnya,
               @masa_berlaku_awal = masa_berlaku_awal,
               @interval_berikutnya = interval_berikutnya
        FROM m_tarif
        WHERE jenis_kendaraan = @jenis_kendaraan AND aktif = 1
    END
    
    -- Calculate fee
    IF @durasi_menit <= @masa_berlaku_awal
    BEGIN
        SET @biaya = @tarif_awal
    END
    ELSE
    BEGIN
        DECLARE @additional_intervals INT
        SET @additional_intervals = CEILING((CAST(@durasi_menit - @masa_berlaku_awal AS FLOAT)) / @interval_berikutnya)
        SET @biaya = @tarif_awal + (@additional_intervals * @tarif_berikutnya)
    END
    
    -- Update parking record
    UPDATE t_parkir
    SET jam_keluar = @jam_keluar,
        durasi_menit = @durasi_menit,
        biaya = @biaya,
        updated_at = GETDATE()
    WHERE parkir_id = @parkir_id
    
    -- Return calculated values
    SELECT @durasi_menit AS durasi_menit, @biaya AS biaya
END
GO

-- Create trigger to update daily revenue
IF EXISTS (SELECT * FROM sys.triggers WHERE name = 'trg_update_pendapatan_harian')
    DROP TRIGGER trg_update_pendapatan_harian
GO

CREATE TRIGGER trg_update_pendapatan_harian
ON t_parkir
AFTER INSERT, UPDATE
AS
BEGIN
    DECLARE @tanggal_masuk DATE, @tanggal_keluar DATE, @biaya DECIMAL(10,2)
    
    -- For INSERT
    IF EXISTS (SELECT 1 FROM inserted WHERE jam_keluar IS NULL)
    BEGIN
        SELECT @tanggal_masuk = CONVERT(DATE, jam_masuk)
        FROM inserted
        WHERE jam_keluar IS NULL
        
        -- Update or insert daily revenue for entry date
        IF EXISTS (SELECT 1 FROM t_pendapatan_harian WHERE tanggal = @tanggal_masuk)
        BEGIN
            UPDATE t_pendapatan_harian
            SET total_kendaraan_masuk = total_kendaraan_masuk + 1,
                updated_at = GETDATE()
            WHERE tanggal = @tanggal_masuk
        END
        ELSE
        BEGIN
            INSERT INTO t_pendapatan_harian (tanggal, total_kendaraan_masuk)
            VALUES (@tanggal_masuk, 1)
        END
    END
    
    -- For UPDATE with exit information
    IF EXISTS (SELECT 1 FROM inserted i JOIN deleted d ON i.parkir_id = d.parkir_id 
               WHERE i.jam_keluar IS NOT NULL AND d.jam_keluar IS NULL)
    BEGIN
        SELECT @tanggal_keluar = CONVERT(DATE, jam_keluar),
               @biaya = biaya
        FROM inserted
        WHERE jam_keluar IS NOT NULL
        
        -- Update daily revenue for exit date
        IF EXISTS (SELECT 1 FROM t_pendapatan_harian WHERE tanggal = @tanggal_keluar)
        BEGIN
            UPDATE t_pendapatan_harian
            SET total_kendaraan_keluar = total_kendaraan_keluar + 1,
                total_pendapatan = total_pendapatan + ISNULL(@biaya, 0),
                updated_at = GETDATE()
            WHERE tanggal = @tanggal_keluar
        END
        ELSE
        BEGIN
            INSERT INTO t_pendapatan_harian (tanggal, total_kendaraan_keluar, total_pendapatan)
            VALUES (@tanggal_keluar, 1, ISNULL(@biaya, 0))
        END
    END
END
GO

PRINT 'Database schema created successfully!'; 