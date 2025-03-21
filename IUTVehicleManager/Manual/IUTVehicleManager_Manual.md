# IUT Vehicle Manager - User Manual

## Table of Contents
1. [System Overview](#system-overview)
2. [System Requirements](#system-requirements)
3. [Installation Guide](#installation-guide)
4. [Database Setup](#database-setup)
5. [User Roles and Access](#user-roles-and-access)
6. [Administrator Guide](#administrator-guide)
7. [Operator Guide](#operator-guide)
8. [Troubleshooting](#troubleshooting)

## System Overview
IUT Vehicle Manager is a comprehensive vehicle management system designed to handle vehicle entry and exit operations with integrated payment processing. The system consists of three main components:

1. **Server Application**
   - Central database management
   - System configuration
   - User management
   - Report generation

2. **Client Application (Entry Point)**
   - Vehicle entry processing
   - Ticket printing
   - Real-time status monitoring

3. **Exit Point Application**
   - Vehicle exit processing
   - Payment processing
   - Receipt printing

## System Requirements

### Hardware Requirements
- **Server:**
  - Processor: Intel Core i5 or higher
  - RAM: 8GB minimum
  - Storage: 256GB SSD minimum
  - Network: Gigabit Ethernet

- **Client/Exit Point:**
  - Processor: Intel Core i3 or higher
  - RAM: 4GB minimum
  - Storage: 128GB SSD minimum
  - Network: 100Mbps Ethernet
  - USB ports for printer and scanner

### Software Requirements
- **Operating System:**
  - Windows 10/11 Pro or Enterprise
  - Latest Windows updates installed

- **Required Software:**
  1. Microsoft SQL Server 2019 or later
  2. .NET Framework 6.0 or later
  3. Visual C++ Redistributable 2015-2022
  4. Thermal Printer Drivers
  5. Barcode Scanner Drivers

## Installation Guide

### 1. Server Installation
1. Install Windows Server 2019 or later
2. Install SQL Server 2019:
   - Choose "Database Engine" and "Management Tools"
   - Set authentication mode to "Mixed Mode"
   - Create a strong password for SA account
3. Install .NET Framework 6.0
4. Install Visual C++ Redistributable
5. Run IUTVehicleManager_Server_Setup.exe
6. Configure database connection:
   - Open Server Configuration Tool
   - Enter SQL Server details
   - Test connection
7. Create initial admin account

### 2. Client Installation (Entry Point)
1. Install Windows 10/11 Pro
2. Install required software:
   - .NET Framework 6.0
   - Visual C++ Redistributable
   - Thermal Printer Drivers
   - Barcode Scanner Drivers
3. Run IUTVehicleManager_Client_Setup.exe
4. Configure server connection:
   - Enter server IP address
   - Test connection
5. Configure local devices:
   - Set up thermal printer
   - Configure barcode scanner
   - Test all devices

### 3. Exit Point Installation
1. Install Windows 10/11 Pro
2. Install required software:
   - .NET Framework 6.0
   - Visual C++ Redistributable
   - Thermal Printer Drivers
   - Barcode Scanner Drivers
3. Run IUTVehicleManager_Exit_Setup.exe
4. Configure server connection:
   - Enter server IP address
   - Test connection
5. Configure local devices:
   - Set up thermal printer
   - Configure barcode scanner
   - Test all devices

## Database Setup

### Initial Database Configuration
1. Open SQL Server Management Studio
2. Create new database named "IUTVehicleManager"
3. Run database initialization script
4. Configure backup schedule:
   - Daily full backup
   - Hourly transaction log backup
5. Set up database maintenance plan

### Database Maintenance
1. Regular backup verification
2. Index maintenance
3. Statistics updates
4. Log file management

## User Roles and Access

### Administrator Role
- Full system access
- User management
- System configuration
- Report generation
- Database management

### Operator Role
- Vehicle entry/exit processing
- Payment processing
- Basic reporting
- Device management

## Administrator Guide

### System Configuration
1. **User Management**
   - Create new users
   - Assign roles
   - Reset passwords
   - Deactivate accounts

2. **Device Configuration**
   - Configure printer settings
   - Set up scanner parameters
   - Test device connections

3. **System Settings**
   - Configure payment rates
   - Set up vehicle types
   - Define priority levels
   - Configure backup settings

### Report Generation
1. **Daily Reports**
   - Vehicle entry/exit summary
   - Payment collection report
   - Occupancy statistics

2. **Monthly Reports**
   - Revenue analysis
   - Vehicle type distribution
   - Peak hour analysis

3. **Custom Reports**
   - Date range selection
   - Filter options
   - Export formats

### Database Management
1. **Backup and Restore**
   - Schedule backups
   - Verify backup integrity
   - Restore from backup

2. **Data Maintenance**
   - Archive old records
   - Clean up temporary data
   - Optimize database

## Operator Guide

### Entry Point Operations
1. **Vehicle Entry**
   - Scan vehicle barcode
   - Verify vehicle information
   - Print entry ticket
   - Update status

2. **Device Management**
   - Check printer status
   - Test scanner
   - Report device issues

### Exit Point Operations
1. **Vehicle Exit**
   - Scan exit barcode
   - Calculate parking fee
   - Process payment
   - Print receipt

2. **Payment Processing**
   - Accept various payment methods
   - Handle payment issues
   - Generate payment reports

### Common Tasks
1. **Status Monitoring**
   - Check occupancy status
   - Monitor device status
   - View recent transactions

2. **Basic Reporting**
   - Generate shift reports
   - Check payment status
   - View error logs

## Troubleshooting

### Common Issues
1. **Connection Problems**
   - Check network connectivity
   - Verify server status
   - Test database connection

2. **Device Issues**
   - Printer not responding
   - Scanner not working
   - Payment device errors

3. **Application Errors**
   - Login problems
   - Data synchronization issues
   - Report generation errors

### Support Contact
- Technical Support: [Support Contact Information]
- Emergency Contact: [Emergency Contact Information]
- Service Hours: [Service Hours]

---

**Note:** This manual should be kept updated with any system changes or new features. For the latest version, please check the system's help menu or contact the system administrator. 