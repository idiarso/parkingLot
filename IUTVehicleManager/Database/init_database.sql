-- Create Database
CREATE DATABASE IUTVehicleManager;
GO

USE IUTVehicleManager;
GO

-- Create Tables

-- Users Table
CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) NOT NULL UNIQUE,
    Password NVARCHAR(256) NOT NULL,
    FullName NVARCHAR(100) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    LastLogin DATETIME NULL
);

-- Vehicle Types Table
CREATE TABLE VehicleTypes (
    TypeID INT IDENTITY(1,1) PRIMARY KEY,
    TypeName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    RatePerHour DECIMAL(10,2) NOT NULL,
    IsActive BIT DEFAULT 1
);

-- Priority Levels Table
CREATE TABLE PriorityLevels (
    PriorityID INT IDENTITY(1,1) PRIMARY KEY,
    PriorityName NVARCHAR(50) NOT NULL,
    Description NVARCHAR(200),
    IsActive BIT DEFAULT 1
);

-- Vehicles Table
CREATE TABLE Vehicles (
    ID INT PRIMARY KEY IDENTITY(1,1),
    PlateNumber VARCHAR(20),
    VehicleType VARCHAR(50),
    Priority VARCHAR(20)
);

-- Entry Transactions Table
CREATE TABLE EntryTransactions (
    ID INT PRIMARY KEY IDENTITY(1,1),
    VehicleID INT,
    EntryTime DATETIME,
    TicketNumber VARCHAR(50),
    OperatorID INT,
    FOREIGN KEY (VehicleID) REFERENCES Vehicles(ID)
);

-- Exit Transactions Table
CREATE TABLE ExitTransactions (
    ID INT PRIMARY KEY IDENTITY(1,1),
    EntryTransactionID INT,
    ExitTime DATETIME,
    Duration INT, -- dalam menit
    Amount DECIMAL(10,2),
    PaymentMethod VARCHAR(50),
    PaymentStatus VARCHAR(20),
    OperatorID INT,
    FOREIGN KEY (EntryTransactionID) REFERENCES EntryTransactions(ID)
);

-- Payment Methods Table
CREATE TABLE PaymentMethods (
    MethodID INT IDENTITY(1,1) PRIMARY KEY,
    MethodName NVARCHAR(50) NOT NULL,
    IsActive BIT DEFAULT 1
);

-- Device Settings Table
CREATE TABLE DeviceSettings (
    DeviceID INT IDENTITY(1,1) PRIMARY KEY,
    DeviceName NVARCHAR(50) NOT NULL,
    DeviceType NVARCHAR(50) NOT NULL,
    PortName NVARCHAR(50),
    BaudRate INT,
    IsActive BIT DEFAULT 1,
    LastMaintenance DATETIME,
    Notes NVARCHAR(500)
);

-- System Settings Table
CREATE TABLE SystemSettings (
    SettingID INT IDENTITY(1,1) PRIMARY KEY,
    SettingKey NVARCHAR(50) NOT NULL UNIQUE,
    SettingValue NVARCHAR(MAX),
    Description NVARCHAR(200),
    LastModified DATETIME DEFAULT GETDATE()
);

-- Create Sync Tables
CREATE TABLE EntryTransactions_Sync (
    ID INT PRIMARY KEY IDENTITY(1,1),
    OriginalID INT,
    VehicleID INT,
    EntryTime DATETIME,
    TicketNumber VARCHAR(50),
    OperatorID INT,
    SyncStatus VARCHAR(20) DEFAULT 'PENDING',
    SyncTime DATETIME,
    FOREIGN KEY (OriginalID) REFERENCES EntryTransactions(ID)
);

CREATE TABLE ExitTransactions_Sync (
    ID INT PRIMARY KEY IDENTITY(1,1),
    OriginalID INT,
    EntryTransactionID INT,
    ExitTime DATETIME,
    Duration INT,
    Amount DECIMAL(10,2),
    PaymentMethod VARCHAR(50),
    PaymentStatus VARCHAR(20),
    OperatorID INT,
    SyncStatus VARCHAR(20) DEFAULT 'PENDING',
    SyncTime DATETIME,
    FOREIGN KEY (OriginalID) REFERENCES ExitTransactions(ID)
);

CREATE TABLE SyncStatus (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableName VARCHAR(50),
    LastSyncTime DATETIME,
    PendingRecords INT,
    SyncStatus VARCHAR(20),
    UpdatedAt DATETIME DEFAULT GETDATE()
);

CREATE TABLE SyncErrors (
    ID INT PRIMARY KEY IDENTITY(1,1),
    TableName VARCHAR(50),
    ErrorMessage NVARCHAR(MAX),
    ErrorTime DATETIME DEFAULT GETDATE(),
    ResolvedAt DATETIME,
    Resolution NVARCHAR(MAX)
);

-- Insert Default Data

-- Insert Default Admin User
INSERT INTO Users (Username, Password, FullName, Role)
VALUES ('admin', 'admin123', 'System Administrator', 'Administrator');

-- Insert Default Vehicle Types
INSERT INTO VehicleTypes (TypeName, Description, RatePerHour)
VALUES 
('Car', 'Standard passenger car', 5000),
('Motorcycle', 'Two-wheeled vehicle', 3000),
('Truck', 'Commercial truck', 10000),
('Bus', 'Passenger bus', 15000);

-- Insert Default Priority Levels
INSERT INTO PriorityLevels (PriorityName, Description)
VALUES 
('Normal', 'Standard priority'),
('High', 'High priority vehicle'),
('VIP', 'Very Important Person');

-- Insert Default Payment Methods
INSERT INTO PaymentMethods (MethodName)
VALUES 
('Cash'),
('Credit Card'),
('Debit Card'),
('E-Wallet');

-- Insert Default System Settings
INSERT INTO SystemSettings (SettingKey, SettingValue, Description)
VALUES 
('MaxParkingCapacity', '100', 'Maximum number of vehicles allowed in parking'),
('AutoPrintTicket', 'true', 'Automatically print ticket on entry'),
('BackupFrequency', 'daily', 'Database backup frequency'),
('ReportPath', 'C:\Reports', 'Default path for report storage');

-- Create Indexes
CREATE INDEX IX_Vehicles_PlateNumber ON Vehicles(PlateNumber);
CREATE INDEX IX_EntryTransactions_EntryTime ON EntryTransactions(EntryTime);
CREATE INDEX IX_ExitTransactions_ExitTime ON ExitTransactions(ExitTime);
CREATE INDEX IX_Vehicles_Status ON Vehicles(Status);

-- Create Views
CREATE VIEW vw_CurrentParkingStatus AS
SELECT 
    v.PlateNumber,
    vt.TypeName AS VehicleType,
    pl.PriorityName AS Priority,
    er.EntryTime,
    er.TicketNumber,
    er.EntryPoint,
    DATEDIFF(MINUTE, er.EntryTime, GETDATE()) AS Duration
FROM Vehicles v
JOIN EntryTransactions er ON v.ID = er.VehicleID
JOIN VehicleTypes vt ON v.VehicleType = vt.TypeName
JOIN PriorityLevels pl ON v.Priority = pl.PriorityName
WHERE v.Status = 'IN';

-- Create Sync Status View
CREATE VIEW vw_SyncStatus AS
SELECT 
    'EntryTransactions' as TableName,
    COUNT(CASE WHEN SyncStatus = 'PENDING' THEN 1 END) as PendingCount,
    MAX(CASE WHEN SyncStatus = 'SYNCED' THEN EntryTime END) as LastSyncTime
FROM EntryTransactions
UNION ALL
SELECT 
    'ExitTransactions' as TableName,
    COUNT(CASE WHEN SyncStatus = 'PENDING' THEN 1 END) as PendingCount,
    MAX(CASE WHEN SyncStatus = 'SYNCED' THEN ExitTime END) as LastSyncTime
FROM ExitTransactions;

-- Create Stored Procedures
CREATE PROCEDURE sp_ProcessVehicleEntry
    @PlateNumber NVARCHAR(20),
    @VehicleTypeID INT,
    @PriorityID INT,
    @EntryPoint NVARCHAR(50),
    @OperatorID INT,
    @TicketNumber NVARCHAR(50)
AS
BEGIN
    BEGIN TRANSACTION;
    
    -- Insert or update vehicle
    MERGE Vehicles AS target
    USING (SELECT @PlateNumber AS PlateNumber) AS source
    ON target.PlateNumber = source.PlateNumber
    WHEN MATCHED THEN
        UPDATE SET 
            VehicleType = (SELECT TypeName FROM VehicleTypes WHERE TypeID = @VehicleTypeID),
            Priority = (SELECT PriorityName FROM PriorityLevels WHERE PriorityID = @PriorityID),
            Status = 'IN',
            EntryTime = GETDATE()
    WHEN NOT MATCHED THEN
        INSERT (PlateNumber, VehicleType, Priority, Status, EntryTime)
        VALUES (@PlateNumber, (SELECT TypeName FROM VehicleTypes WHERE TypeID = @VehicleTypeID), (SELECT PriorityName FROM PriorityLevels WHERE PriorityID = @PriorityID), 'IN', GETDATE());

    -- Get VehicleID
    DECLARE @VehicleID INT;
    SELECT @VehicleID = ID FROM Vehicles WHERE PlateNumber = @PlateNumber;

    -- Insert entry record
    INSERT INTO EntryTransactions (VehicleID, EntryTime, TicketNumber, OperatorID)
    VALUES (@VehicleID, GETDATE(), @TicketNumber, @OperatorID);

    COMMIT;
END;

CREATE PROCEDURE sp_ProcessVehicleExit
    @PlateNumber NVARCHAR(20),
    @ExitPoint NVARCHAR(50),
    @OperatorID INT,
    @PaymentMethod NVARCHAR(50),
    @Amount DECIMAL(10,2)
AS
BEGIN
    BEGIN TRANSACTION;

    -- Get vehicle and entry record
    DECLARE @VehicleID INT;
    DECLARE @EntryTransactionID INT;
    DECLARE @EntryTime DATETIME;
    
    SELECT 
        @VehicleID = v.ID,
        @EntryTransactionID = et.ID,
        @EntryTime = et.EntryTime
    FROM Vehicles v
    JOIN EntryTransactions et ON v.ID = et.VehicleID
    WHERE v.PlateNumber = @PlateNumber AND v.Status = 'IN';

    -- Calculate duration
    DECLARE @Duration INT;
    SET @Duration = DATEDIFF(MINUTE, @EntryTime, GETDATE());

    -- Generate receipt number
    DECLARE @ReceiptNumber NVARCHAR(50);
    SET @ReceiptNumber = 'RCP' + FORMAT(GETDATE(), 'yyyyMMddHHmmss');

    -- Insert exit record
    INSERT INTO ExitTransactions (
        EntryTransactionID, ExitTime, Duration, Amount, PaymentMethod,
        PaymentStatus, OperatorID
    )
    VALUES (
        @EntryTransactionID, GETDATE(), @Duration, @Amount, @PaymentMethod,
        'PAID', @OperatorID
    );

    -- Update vehicle status
    UPDATE Vehicles
    SET Status = 'OUT', ExitTime = GETDATE()
    WHERE ID = @VehicleID;

    COMMIT;
END;

-- Create Triggers
CREATE TRIGGER trg_UpdateVehicleStatus
ON EntryTransactions
AFTER INSERT
AS
BEGIN
    UPDATE Vehicles
    SET Status = 'IN', EntryTime = GETDATE()
    FROM Vehicles v
    JOIN inserted i ON v.ID = i.VehicleID;
END;

CREATE TRIGGER trg_LogExitTransaction
ON ExitTransactions
AFTER INSERT
AS
BEGIN
    UPDATE Vehicles
    SET Status = 'OUT', ExitTime = GETDATE()
    FROM Vehicles v
    JOIN inserted i ON v.ID = i.EntryTransactionID;
END;

-- Create Functions
CREATE FUNCTION fn_CalculateParkingFee
(
    @EntryTime DATETIME,
    @VehicleTypeID INT
)
RETURNS DECIMAL(10,2)
AS
BEGIN
    DECLARE @Duration INT;
    DECLARE @RatePerHour DECIMAL(10,2);
    
    SET @Duration = DATEDIFF(MINUTE, @EntryTime, GETDATE());
    SELECT @RatePerHour = RatePerHour FROM VehicleTypes WHERE TypeID = @VehicleTypeID;
    
    RETURN CEILING(@Duration / 60.0) * @RatePerHour;
END;

-- Grant Permissions
GRANT SELECT ON vw_CurrentParkingStatus TO [IUTVehicleManager_Operator];
GRANT EXECUTE ON sp_ProcessVehicleEntry TO [IUTVehicleManager_Operator];
GRANT EXECUTE ON sp_ProcessVehicleExit TO [IUTVehicleManager_Operator];

-- Create Database Roles
CREATE ROLE [IUTVehicleManager_Administrator];
CREATE ROLE [IUTVehicleManager_Operator];

-- Grant Permissions to Roles
GRANT CONTROL SERVER TO [IUTVehicleManager_Administrator];
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES TO [IUTVehicleManager_Administrator];
GRANT EXECUTE ON ALL PROCEDURES TO [IUTVehicleManager_Administrator];

GRANT SELECT ON ALL TABLES TO [IUTVehicleManager_Operator];
GRANT EXECUTE ON sp_ProcessVehicleEntry TO [IUTVehicleManager_Operator];
GRANT EXECUTE ON sp_ProcessVehicleExit TO [IUTVehicleManager_Operator];

-- Add SyncStatus column to existing tables
ALTER TABLE EntryTransactions
ADD SyncStatus VARCHAR(20) DEFAULT 'PENDING';

ALTER TABLE ExitTransactions
ADD SyncStatus VARCHAR(20) DEFAULT 'PENDING';

-- Create Sync Stored Procedures
CREATE PROCEDURE sp_SyncEntryTransactions
AS
BEGIN
    BEGIN TRANSACTION;
    
    INSERT INTO EntryTransactions_Sync (
        OriginalID, VehicleID, EntryTime, TicketNumber, OperatorID, SyncStatus, SyncTime
    )
    SELECT 
        ID, VehicleID, EntryTime, TicketNumber, OperatorID, 'SYNCED', GETDATE()
    FROM EntryTransactions
    WHERE SyncStatus = 'PENDING';

    UPDATE EntryTransactions
    SET SyncStatus = 'SYNCED'
    WHERE SyncStatus = 'PENDING';

    COMMIT;
END;

CREATE PROCEDURE sp_SyncExitTransactions
AS
BEGIN
    BEGIN TRANSACTION;
    
    INSERT INTO ExitTransactions_Sync (
        OriginalID, EntryTransactionID, ExitTime, Duration, Amount,
        PaymentMethod, PaymentStatus, OperatorID, SyncStatus, SyncTime
    )
    SELECT 
        ID, EntryTransactionID, ExitTime, Duration, Amount,
        PaymentMethod, PaymentStatus, OperatorID, 'SYNCED', GETDATE()
    FROM ExitTransactions
    WHERE SyncStatus = 'PENDING';

    UPDATE ExitTransactions
    SET SyncStatus = 'SYNCED'
    WHERE SyncStatus = 'PENDING';

    COMMIT;
END;

-- Create Sync Error Logging Procedure
CREATE PROCEDURE sp_LogSyncError
    @TableName VARCHAR(50),
    @ErrorMessage NVARCHAR(MAX)
AS
BEGIN
    INSERT INTO SyncErrors (TableName, ErrorMessage)
    VALUES (@TableName, @ErrorMessage);
END;

-- Stored procedure untuk menghitung biaya parkir
DELIMITER //
CREATE PROCEDURE CalculateParkingFee(
    IN p_vehicle_type VARCHAR(50),
    IN p_hours INT,
    OUT p_fee DECIMAL(10,2)
)
BEGIN
    DECLARE base_fee DECIMAL(10,2);
    DECLARE hourly_fee DECIMAL(10,2);
    
    -- Ambil tarif dasar dan per jam berdasarkan jenis kendaraan
    SELECT base_rate, hourly_rate 
    INTO base_fee, hourly_fee
    FROM vehicle_rates 
    WHERE vehicle_type = p_vehicle_type;
    
    -- Hitung total biaya
    -- Jika kurang dari 1 jam, hanya bayar tarif dasar
    IF p_hours <= 1 THEN
        SET p_fee = base_fee;
    ELSE
        -- Jika lebih dari 1 jam, hitung tarif per jam
        SET p_fee = base_fee + (hourly_fee * (p_hours - 1));
    END IF;
END //
DELIMITER ;

-- Tabel untuk menyimpan tarif parkir
CREATE TABLE vehicle_rates (
    id INT PRIMARY KEY AUTO_INCREMENT,
    vehicle_type VARCHAR(50) NOT NULL,
    base_rate DECIMAL(10,2) NOT NULL,
    hourly_rate DECIMAL(10,2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
);

-- Insert data tarif default
INSERT INTO vehicle_rates (vehicle_type, base_rate, hourly_rate) VALUES
('Motor', 2000, 1000),
('Mobil', 5000, 2000),
('Bus', 10000, 5000),
('Truk', 10000, 5000); 