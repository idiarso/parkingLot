@echo off
echo ===================================================
echo Verifying Services and Starting ParkingIN
echo ===================================================

echo.
echo 1. Checking PostgreSQL Service...
sc query postgresql-x64-14
if %ERRORLEVEL% NEQ 0 (
    echo Starting PostgreSQL service...
    net start postgresql-x64-14
    timeout /t 5
)

echo.
echo 2. Testing Database Connection...
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=root@rsi
set PGDATABASE=parkirdb

psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDATABASE% -c "\conninfo" 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Database connection failed! Starting PostgreSQL...
    net start postgresql-x64-14
    timeout /t 5
) else (
    echo Database connection successful!
)

echo.
echo 3. Checking Running Processes...
echo Checking ParkingIN processes...
tasklist /FI "IMAGENAME eq ParkingIN.exe" /FO TABLE
if %ERRORLEVEL% EQU 0 (
    echo Stopping existing ParkingIN processes...
    taskkill /F /IM ParkingIN.exe /T
    timeout /t 2
)

echo.
echo 4. Starting ParkingIN Application...
cd /d "D:\21maret\clean_code\ParkingIN\bin\Release\net6.0-windows"
if not exist "ParkingIN.exe" (
    echo Error: ParkingIN.exe not found!
    echo Please build the application first.
    goto :error
)

echo Creating logs directory if not exists...
if not exist "logs" mkdir logs

echo.
echo Starting ParkingIN...
echo - Username: admin
echo - Password: admin
echo.
echo Check logs\database.log for connection details
start ParkingIN.exe

echo.
echo Application started! Please test the login functionality.
goto :end

:error
echo.
echo ===================================================
echo Error occurred during verification/startup
echo ===================================================
exit /b 1

:end
echo.
echo Process completed successfully.
timeout /t 3 