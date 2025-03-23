-- Script untuk membuat tabel users di database parkirdb
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

-- Cek apakah user admin sudah ada
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'admin') THEN
        -- Tambahkan user admin default dengan password 'password123' yang di-hash menggunakan SHA-256
        -- Hash untuk password123: ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f
        INSERT INTO users (username, password, nama, role, level, status)
        VALUES ('admin', 'ef92b778bafe771e89245b89ecbc08a44a4e166c06659911881f383d4473e94f', 'Administrator', 'Admin', 'Super', TRUE);
    END IF;
END
$$;
