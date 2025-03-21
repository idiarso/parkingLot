@echo off
echo ===================================================
echo Testing PostgreSQL Connection for ParkingIN
echo ===================================================
echo.
echo Make sure you've updated the password in App.config!
echo.
echo Running ParkingIN application...
echo Click the "Test PostgreSQL Connection" button in the application window.
echo.

cd /d D:\21maret\clean_code\ParkingIN\bin\Release\net6.0-windows
start "" "ParkingIN.exe"

echo.
echo If the application doesn't start, check the path above.
echo.
pause 