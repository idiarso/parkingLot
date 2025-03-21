-- Create tables script for PostgreSQL
-- This will create the basic tables needed for the parking system

-- Create users table
CREATE TABLE IF NOT EXISTS t_user (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    password VARCHAR(255) NOT NULL,
    nama VARCHAR(100) DEFAULT NULL,
    role VARCHAR(20) DEFAULT 'OPERATOR',
    status INTEGER DEFAULT 1,
    last_login TIMESTAMP DEFAULT NULL,
    email VARCHAR(100) DEFAULT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE (username)
);

-- Create tariff table
CREATE TABLE IF NOT EXISTS t_tarif (
    id SERIAL PRIMARY KEY,
    jenis_kendaraan VARCHAR(50) NOT NULL,
    tarif_awal DECIMAL(10,2) NOT NULL DEFAULT 0.00,
    tarif_per_jam DECIMAL(10,2) NOT NULL DEFAULT 0.00
);

-- Insert default admin user if not exists
INSERT INTO t_user (username, password, nama, role, status)
SELECT 'admin', 'admin', 'Administrator', 'ADMIN', 1
WHERE NOT EXISTS (SELECT 1 FROM t_user WHERE username = 'admin');

-- Insert default vehicle types if not exists
INSERT INTO t_tarif (jenis_kendaraan, tarif_awal, tarif_per_jam) 
SELECT 'Mobil', 5000.00, 2000.00
WHERE NOT EXISTS (SELECT 1 FROM t_tarif WHERE jenis_kendaraan = 'Mobil');

INSERT INTO t_tarif (jenis_kendaraan, tarif_awal, tarif_per_jam) 
SELECT 'Motor', 2000.00, 1000.00
WHERE NOT EXISTS (SELECT 1 FROM t_tarif WHERE jenis_kendaraan = 'Motor');

-- Add any additional tables needed by the application here

-- Display success message
DO $$
BEGIN
    RAISE NOTICE 'Database tables created successfully';
END $$; 