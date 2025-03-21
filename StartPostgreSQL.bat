@echo off
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo This script requires administrator privileges
    echo Please right-click and select "Run as administrator"
    powershell -Command "Start-Process '%~dpnx0' -Verb RunAs"
    exit /b
)

echo ===================================================
echo Starting PostgreSQL Service
echo ===================================================

echo.
echo 1. Stopping any existing PostgreSQL processes...
taskkill /F /IM postgres.exe /T 2>nul
net stop postgresql-x64-14 2>nul
timeout /t 5

echo.
echo 2. Starting PostgreSQL service...
net start postgresql-x64-14
timeout /t 5

echo.
echo 3. Verifying service status...
sc query postgresql-x64-14

echo.
echo 4. Testing connection...
set PGPASSWORD=root@rsi
"C:\Program Files\PostgreSQL\14\bin\psql.exe" -U postgres -d postgres -h 127.0.0.1 -c "\conninfo"

echo.
echo Press any key to exit...
pause 