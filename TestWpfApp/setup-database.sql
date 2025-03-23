-- Script untuk membuat tabel users jika belum ada
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(255) NOT NULL,
    salt VARCHAR(50),
    display_name VARCHAR(100),
    role VARCHAR(20),
    last_login TIMESTAMP
);

-- Cek apakah user admin sudah ada
DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM users WHERE username = 'admin') THEN
        -- Tambahkan user admin default
        -- Note: Di aplikasi sebenarnya, password akan di-hash dengan salt
        -- Untuk saat ini kita menggunakan password 'password123' secara langsung
        INSERT INTO users (username, password, display_name, role)
        VALUES ('admin', 'password123', 'Administrator', 'Admin');
    END IF;
END
$$;
