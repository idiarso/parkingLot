@echo off
echo ===================================================
echo ParkingIN Setup Status Check
echo ===================================================

echo.
echo 1. PostgreSQL Status:
sc query postgresql-x64-15

echo.
echo 2. Database Connection:
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=root@rsi
set PGDATABASE=parkirdb

echo Testing connection...
psql -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDATABASE% -c "\conninfo" 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Database connection failed!
) else (
    echo Database connection successful!
)

echo.
echo 3. ParkingIN Application:
if exist "D:\21maret\clean_code\ParkingIN\bin\Release\net6.0-windows\ParkingIN.exe" (
    echo Application is built
) else (
    echo Application needs to be built
)

echo.
echo 4. Running Processes:
tasklist /FI "IMAGENAME eq ParkingIN.exe" /FO TABLE

echo.
pause 