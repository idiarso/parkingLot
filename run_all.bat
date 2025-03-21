@echo off
echo.
echo Starting all Parking System components...
echo.

echo Starting PostgreSQL...
cd /d D:\21maret\clean_code\ParkingIN\Database
call StartPostgreSQL.bat
timeout /t 5 /nobreak > nul

echo Starting ParkingServer...
start cmd /k "cd /d D:\21maret\clean_code\ParkingServer && dotnet run"
timeout /t 5 /nobreak > nul

echo Starting ParkingIN...
start cmd /k "cd /d D:\21maret\clean_code\ParkingIN && dotnet run"
timeout /t 3 /nobreak > nul

echo Starting ParkingOut...
start cmd /k "cd /d D:\21maret\clean_code\ParkingOut && dotnet run"

echo All components started successfully.
echo.
echo Press any key to exit...
pause
