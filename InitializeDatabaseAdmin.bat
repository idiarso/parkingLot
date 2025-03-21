@echo off
:: Check for admin privileges
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :admin
) else (
    echo Requesting administrative privileges...
    powershell -Command "Start-Process '%~dpnx0' -Verb RunAs"
    exit /b
)

:admin
echo ===================================================
echo Initializing PostgreSQL Database for ParkingIN
echo ===================================================

:: Set PostgreSQL paths and connection parameters
set PGBIN="C:\Program Files\PostgreSQL\14\bin"
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=root@rsi
set PGDATABASE=parkirdb

echo.
echo 1. Checking PostgreSQL Service...
net start postgresql-x64-14
timeout /t 5

echo.
echo 2. Creating Database if not exists...
%PGBIN%\psql -U postgres -c "CREATE DATABASE parkirdb WITH ENCODING='UTF8' OWNER=postgres;" 2>nul

echo.
echo 3. Initializing Tables...
%PGBIN%\psql -U postgres -d parkirdb -f InitDatabase.sql

echo.
echo 4. Verifying Database...
%PGBIN%\psql -U postgres -d parkirdb -c "\dt"
%PGBIN%\psql -U postgres -d parkirdb -c "SELECT * FROM t_user;"

echo.
echo Database initialization completed.
echo.
echo Now you can:
echo 1. Start ParkingIN application
echo 2. Login with:
echo    Username: admin
echo    Password: admin
echo.
pause 