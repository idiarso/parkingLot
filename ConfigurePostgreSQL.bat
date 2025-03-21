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
echo Configuring PostgreSQL for TCP/IP Connections
echo ===================================================

set PGHOME=C:\Program Files\PostgreSQL\14
set PGDATA=%PGHOME%\data

echo.
echo 1. Stopping PostgreSQL service...
net stop postgresql-x64-14
timeout /t 5

echo.
echo 2. Updating postgresql.conf...
echo listen_addresses = '*' >> "%PGDATA%\postgresql.conf"

echo.
echo 3. Updating pg_hba.conf...
echo # IPv4 local connections >> "%PGDATA%\pg_hba.conf"
echo host    all             all             127.0.0.1/32            scram-sha-256 >> "%PGDATA%\pg_hba.conf"
echo host    all             all             0.0.0.0/0               scram-sha-256 >> "%PGDATA%\pg_hba.conf"

echo.
echo 4. Starting PostgreSQL service...
net start postgresql-x64-14
timeout /t 5

echo.
echo 5. Verifying connection...
set PGBIN="%PGHOME%\bin"
set PGUSER=postgres
set PGPASSWORD=root@rsi

%PGBIN%\psql -U postgres -c "\conninfo"

if %ERRORLEVEL% EQU 0 (
    echo PostgreSQL is now configured and accepting connections.
) else (
    echo Failed to connect to PostgreSQL.
    echo Please check the configuration and try again.
)

echo.
echo Configuration completed. Press any key to exit...
pause 