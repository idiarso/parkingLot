-- First, check if users table exists
CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    nama_lengkap VARCHAR(100),
    level VARCHAR(20) DEFAULT 'operator',
    active BOOLEAN DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Then, check if t_user table exists (older schema)
CREATE TABLE IF NOT EXISTS t_user (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password VARCHAR(100) NOT NULL,
    nama VARCHAR(100),
    role VARCHAR(20) DEFAULT 'operator',
    status INT DEFAULT 1,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Add a default admin user to users table
-- The password is 'admin123' (use proper hashing in production)
INSERT IGNORE INTO users (username, password, nama_lengkap, level)
VALUES ('admin', 'admin123', 'Administrator', 'admin');

-- Add the same user to t_user table for compatibility
INSERT IGNORE INTO t_user (username, password, nama, role, status)
VALUES ('admin', 'admin123', 'Administrator', 'admin', 1);

-- Display users for confirmation
SELECT 'Users in users table:' AS message;
SELECT * FROM users;

SELECT 'Users in t_user table:' AS message;
SELECT * FROM t_user; 