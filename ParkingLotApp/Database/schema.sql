-- Create the roles table
CREATE TABLE IF NOT EXISTS roles (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    description TEXT,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

-- Insert default roles
INSERT INTO roles (name, description)
VALUES 
    ('Admin', 'Full system access and management'),
    ('Operator', 'Day-to-day parking operations'),
    ('Viewer', 'View-only access to reports and statistics')
ON CONFLICT (name) DO NOTHING;

-- Create the users table
CREATE TABLE IF NOT EXISTS users (
    id SERIAL PRIMARY KEY,
    username VARCHAR(50) UNIQUE NOT NULL,
    password_hash VARCHAR(256) NOT NULL,
    password_salt VARCHAR(256),
    email VARCHAR(100),
    first_name VARCHAR(50),
    last_name VARCHAR(50),
    role_id INTEGER REFERENCES roles(id),
    is_active BOOLEAN DEFAULT true,
    last_login TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

-- Create the shifts table
CREATE TABLE IF NOT EXISTS shifts (
    id SERIAL PRIMARY KEY,
    name VARCHAR(50) NOT NULL,
    start_time TIME NOT NULL,
    end_time TIME NOT NULL,
    description TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP
);

-- Create the user_shifts table for shift assignments
CREATE TABLE IF NOT EXISTS user_shifts (
    id SERIAL PRIMARY KEY,
    user_id INTEGER REFERENCES users(id),
    shift_id INTEGER REFERENCES shifts(id),
    assigned_date DATE NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(user_id, shift_id, assigned_date)
);

-- Create the settings table
CREATE TABLE IF NOT EXISTS settings (
    key VARCHAR(50) PRIMARY KEY,
    value TEXT NOT NULL,
    description TEXT,
    updated_by INTEGER REFERENCES users(id),
    updated_at TIMESTAMP
);

-- Create the parking_activities table
CREATE TABLE IF NOT EXISTS parking_activities (
    id SERIAL PRIMARY KEY,
    vehicle_number VARCHAR(20) NOT NULL,
    vehicle_type VARCHAR(20) NOT NULL,
    action VARCHAR(10) NOT NULL CHECK (action IN ('Entry', 'Exit')),
    entry_time TIMESTAMP NOT NULL,
    exit_time TIMESTAMP,
    duration VARCHAR(50),
    fee DECIMAL(10,2),
    notes TEXT,
    created_by INTEGER REFERENCES users(id),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Create indexes
CREATE INDEX IF NOT EXISTS idx_parking_activities_vehicle_number ON parking_activities(vehicle_number);
CREATE INDEX IF NOT EXISTS idx_parking_activities_entry_time ON parking_activities(entry_time);
CREATE INDEX IF NOT EXISTS idx_parking_activities_exit_time ON parking_activities(exit_time);
CREATE INDEX IF NOT EXISTS idx_users_username ON users(username);
CREATE INDEX IF NOT EXISTS idx_user_shifts_user_id ON user_shifts(user_id);
CREATE INDEX IF NOT EXISTS idx_user_shifts_shift_id ON user_shifts(shift_id);
CREATE INDEX IF NOT EXISTS idx_user_shifts_assigned_date ON user_shifts(assigned_date);

-- Insert default settings
INSERT INTO settings (key, value, description)
VALUES 
    ('total_spots', '100', 'Total number of parking spots available'),
    ('car_rate', '5000', 'Hourly rate for cars in Rupiah'),
    ('motorcycle_rate', '2000', 'Hourly rate for motorcycles in Rupiah'),
    ('truck_rate', '10000', 'Hourly rate for trucks in Rupiah'),
    ('bus_rate', '8000', 'Hourly rate for buses in Rupiah'),
    ('company_name', 'Parking Lot Management System', 'Company name displayed in reports'),
    ('company_address', 'Jakarta, Indonesia', 'Company address displayed in reports'),
    ('report_footer', 'Thank you for your business', 'Footer text for reports')
ON CONFLICT (key) DO NOTHING;

-- Insert default admin user (password: admin)
INSERT INTO users (username, password_hash, password_salt, email, first_name, last_name, role_id, is_active)
VALUES (
    'admin',
    'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=',  -- This is 'admin' hashed with SHA256
    'randomsalt',  -- This would normally be a randomly generated salt
    'admin@example.com',
    'System',
    'Administrator',
    (SELECT id FROM roles WHERE name = 'Admin'),
    true
)
ON CONFLICT (username) DO NOTHING;

-- Insert default shifts
INSERT INTO shifts (name, start_time, end_time, description)
VALUES 
    ('Morning', '06:00:00', '14:00:00', 'Morning shift'),
    ('Afternoon', '14:00:00', '22:00:00', 'Afternoon shift'),
    ('Night', '22:00:00', '06:00:00', 'Night shift')
ON CONFLICT DO NOTHING; 