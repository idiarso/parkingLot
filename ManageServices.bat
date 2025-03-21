@echo off
echo ===================================================
echo Parking System Service Manager
echo ===================================================

echo.
echo 1. Checking running services and processes...
echo -------------------------------------------

echo Checking ParkingIN processes...
tasklist /FI "IMAGENAME eq ParkingIN.exe" /FO TABLE

echo.
echo Checking ParkingOUT processes...
tasklist /FI "IMAGENAME eq ParkingOUT.exe" /FO TABLE

echo.
echo Checking ParkingServer processes...
tasklist /FI "IMAGENAME eq ParkingServer.exe" /FO TABLE

echo.
echo 2. PostgreSQL Service Status
echo --------------------------
sc query postgresql-x64-15

echo.
echo ===================================================
echo Clean Services Menu
echo ===================================================
echo 1. Kill all Parking System processes
echo 2. Restart PostgreSQL service
echo 3. Kill specific process
echo 4. Exit
echo.
set /p choice="Enter your choice (1-4): "

if "%choice%"=="1" (
    echo.
    echo Stopping all Parking System processes...
    taskkill /F /IM ParkingIN.exe 2>nul
    taskkill /F /IM ParkingOUT.exe 2>nul
    taskkill /F /IM ParkingServer.exe 2>nul
    echo All parking processes terminated.
)

if "%choice%"=="2" (
    echo.
    echo Restarting PostgreSQL service...
    net stop postgresql-x64-15
    timeout /t 5
    net start postgresql-x64-15
    echo PostgreSQL service restarted.
)

if "%choice%"=="3" (
    echo.
    set /p pid="Enter Process ID to kill: "
    taskkill /F /PID %pid%
)

if "%choice%"=="4" (
    exit
)

echo.
echo Process completed. Press any key to exit...
pause > nul 