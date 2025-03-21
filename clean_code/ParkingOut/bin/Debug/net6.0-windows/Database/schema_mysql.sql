-- Create the database if it doesn't exist
CREATE DATABASE IF NOT EXISTS parkingdb;

-- Use the database
USE parkingdb;

-- Create users table
CREATE TABLE IF NOT EXISTS `users` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `username` varchar(50) NOT NULL,
  `password` varchar(255) NOT NULL,
  `nama` varchar(100) DEFAULT NULL,
  `role` varchar(20) DEFAULT 'operator',
  `last_login` datetime DEFAULT NULL,
  `status` tinyint(1) DEFAULT 1,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `username` (`username`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default admin user if it doesn't exist (password: admin123)
INSERT IGNORE INTO `users` (`username`, `password`, `nama`, `role`) 
VALUES ('admin', '240be518fabd2724ddb6f04eeb1da5967448d7e831c08c8fa822809f74c720a9', 'Administrator', 'admin');

-- Create t_parkir table
CREATE TABLE IF NOT EXISTS `t_parkir` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nomor_polisi` varchar(20) NOT NULL,
  `jenis_kendaraan` varchar(50) NOT NULL,
  `waktu_masuk` datetime NOT NULL,
  `waktu_keluar` datetime DEFAULT NULL,
  `durasi` int(11) DEFAULT NULL,
  `biaya` decimal(10,2) DEFAULT NULL,
  `foto_masuk` varchar(255) DEFAULT NULL,
  `foto_keluar` varchar(255) DEFAULT NULL,
  `keterangan` varchar(255) DEFAULT NULL,
  `status_tiket` varchar(20) DEFAULT 'NORMAL',
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  KEY `nomor_polisi` (`nomor_polisi`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create t_tarif table
CREATE TABLE IF NOT EXISTS `t_tarif` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jenis_kendaraan` varchar(50) NOT NULL,
  `tarif_perjam` decimal(10,2) NOT NULL,
  `tarif_harian` decimal(10,2) DEFAULT NULL,
  `tarif_mingguan` decimal(10,2) DEFAULT NULL,
  `tarif_overtime` decimal(10,2) DEFAULT NULL,
  `grace_period` int(11) DEFAULT 10,
  `status` tinyint(1) DEFAULT 1,
  `created_at` datetime DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Create settings table
CREATE TABLE IF NOT EXISTS `settings` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `nama` varchar(100) NOT NULL,
  `nilai` varchar(255) NOT NULL,
  `deskripsi` varchar(255) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nama` (`nama`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Insert default settings
INSERT IGNORE INTO `settings` (`nama`, `nilai`, `deskripsi`) VALUES
('denda_tiket_hilang', '25000', 'Denda untuk tiket yang hilang'),
('refresh_interval', '60000', 'Interval refresh dalam milidetik'),
('long_parking_threshold', '120', 'Batas waktu parkir lama dalam menit'),
('critical_capacity', '90', 'Persentase kapasitas kritis'),
('warning_capacity', '75', 'Persentase kapasitas peringatan'),
('total_kapasitas', '100', 'Total kapasitas parkir');

-- Insert default tariffs
INSERT IGNORE INTO `t_tarif` (`jenis_kendaraan`, `tarif_perjam`, `tarif_harian`) VALUES
('MOBIL', 5000.00, 50000.00),
('MOTOR', 2000.00, 20000.00);

-- Tabel tarif_khusus untuk menyimpan tarif khusus berdasarkan waktu
CREATE TABLE IF NOT EXISTS `tarif_khusus` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `jenis_kendaraan` varchar(50) NOT NULL,
  `jenis_tarif` varchar(50) NOT NULL,
  `jam_mulai` time DEFAULT NULL,
  `jam_selesai` time DEFAULT NULL,
  `hari` varchar(100) DEFAULT NULL,
  `tarif_flat` decimal(10,2) DEFAULT NULL,
  `deskripsi` varchar(255) DEFAULT NULL,
  `status` tinyint(1) DEFAULT 1,
  `created_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

-- Tabel denda untuk mencatat denda
CREATE TABLE IF NOT EXISTS `denda` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `parkir_id` int(11) NOT NULL,
  `jenis_denda` varchar(50) NOT NULL,
  `jumlah` decimal(10,0) NOT NULL,
  `waktu` datetime NOT NULL,
  `keterangan` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `parkir_id` (`parkir_id`),
  CONSTRAINT `denda_ibfk_1` FOREIGN KEY (`parkir_id`) REFERENCES `t_parkir` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci; 