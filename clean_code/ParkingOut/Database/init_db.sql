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
VALUES ('admin', 'admin123', 'Administrator', 'admin'); 