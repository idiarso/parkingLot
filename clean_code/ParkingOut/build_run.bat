@echo off
echo Building and running ParkingOut...
echo.

REM Check if MySQL is running in XAMPP
echo Checking MySQL service...
tasklist /FI "IMAGENAME eq mysqld.exe" 2>NUL | find /I /N "mysqld.exe">NUL
if "%ERRORLEVEL%"=="0" (
    echo MySQL is running
) else (
    echo Starting MySQL...
    start "" /B "D:\XAMPP\mysql\bin\mysqld.exe"
    timeout /t 5
)

REM Build the project
echo.
echo Building ParkingOut...
dotnet restore ParkingOut.csproj
if %ERRORLEVEL% NEQ 0 (
    echo Error: Package restore failed
    pause
    exit /b 1
)

dotnet build ParkingOut.csproj --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo Error: Build failed
    pause
    exit /b 1
)
echo Build completed successfully

REM Start the application
echo.
echo Starting ParkingOut...
start "" "bin\Release\net6.0-windows\ParkingOut.exe"

echo.
echo Process completed successfully!
pause 