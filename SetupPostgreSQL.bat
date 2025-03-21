@echo off
echo ===================================================
echo Setting up PostgreSQL Database for Parking System
echo ===================================================

echo.
echo This script will initialize the PostgreSQL database with the necessary tables.
echo Please make sure PostgreSQL is running and you have the correct credentials.
echo.

:: Set variables
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=root@rsi
set PGDATABASE=parkirdb

echo Current database settings:
echo Host: %PGHOST%
echo Port: %PGPORT%
echo User: %PGUSER%
echo Password: [hidden]
echo Database: %PGDATABASE%
echo.

echo Checking PostgreSQL installation...
where psql >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo PostgreSQL client tools (psql) not found in PATH.
    echo Please ensure PostgreSQL is installed and added to PATH.
    echo You may need to run this command manually using pgAdmin or another PostgreSQL tool.
    goto :end
)

echo Creating database if it doesn't exist...
psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d postgres -c "CREATE DATABASE %PGDATABASE% WITH ENCODING='UTF8' OWNER=%PGUSER%;" 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Note: Database may already exist or there was an error.
    echo Continuing with table creation...
)

echo.
echo Running SQL script to create tables...
psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDATABASE% -f create_tables.sql

if %ERRORLEVEL% NEQ 0 (
    echo Error running SQL script. Please check error messages above.
    goto :error
)

echo.
echo Database setup completed successfully.
echo.
echo You can now run the ParkingIN application with:
echo   - Username: admin
echo   - Password: admin
echo.
goto :end

:error
echo.
echo ===================================================
echo An error occurred during database setup.
echo ===================================================

:end
pause 