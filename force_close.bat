@echo off
echo ===================================================
echo Force Closing All Parking System Applications
echo ===================================================
echo.

echo Terminating all running Parking System applications...

echo Checking for ParkingIN processes...
taskkill /f /im ParkingIN.exe 2>nul
if %ERRORLEVEL% EQU 0 (
    echo - ParkingIN.exe closed successfully.
) else (
    echo - No ParkingIN.exe processes found.
)

echo Checking for ParkingOUT processes...
taskkill /f /im ParkingOUT.exe 2>nul
if %ERRORLEVEL% EQU 0 (
    echo - ParkingOUT.exe closed successfully.
) else (
    echo - No ParkingOUT.exe processes found.
)

echo Checking for ParkingServer processes...
taskkill /f /im ParkingServer.exe 2>nul
if %ERRORLEVEL% EQU 0 (
    echo - ParkingServer.exe closed successfully.
) else (
    echo - No ParkingServer.exe processes found.
)

echo Checking for dotnet processes running Parking applications...
for /f "tokens=2 delims=," %%p in ('tasklist /fi "IMAGENAME eq dotnet.exe" /fo csv /nh') do (
    echo Checking process %%p
    taskkill /f /pid %%p 2>nul
)

echo Checking for any remaining related processes...
taskkill /f /im dotnet.exe /fi "WINDOWTITLE eq ParkingIN" 2>nul
taskkill /f /im dotnet.exe /fi "WINDOWTITLE eq ParkingOUT" 2>nul
taskkill /f /im dotnet.exe /fi "WINDOWTITLE eq ParkingServer" 2>nul

echo.
echo All Parking System applications have been force closed.
echo.

echo ===================================================
echo Checking System Logs
echo ===================================================
echo.

rem Get proper date format for log files - handle different regional settings
for /f "tokens=2 delims==" %%a in ('wmic os get LocalDateTime /value') do set "dt=%%a"
set "today=%dt:~0,8%"
echo Today's date code: %today% (Format: YYYYMMDD)

echo Checking ParkingIN logs...
if exist "D:\21maret\clean_code\ParkingIN\logs\app%today%.log" (
    echo.
    echo Recent ParkingIN application log entries:
    echo -------------------------------------------
    type "D:\21maret\clean_code\ParkingIN\logs\app%today%.log" | findstr /C:"ERROR" /C:"WARN" /C:"Exception"
    if %ERRORLEVEL% NEQ 0 (
        echo No errors or warnings found in ParkingIN logs.
    )
) else (
    echo No ParkingIN logs found for today.
)

echo.
echo Checking ParkingOUT logs...
if exist "D:\21maret\clean_code\ParkingOut\Logs\app%today%.log" (
    echo.
    echo Recent ParkingOUT application log entries:
    echo -------------------------------------------
    type "D:\21maret\clean_code\ParkingOut\Logs\app%today%.log" | findstr /C:"ERROR" /C:"WARN" /C:"Exception"
    if %ERRORLEVEL% NEQ 0 (
        echo No errors or warnings found in ParkingOUT logs.
    )
) else (
    echo No ParkingOUT logs found for today.
)

echo.
echo Checking ParkingServer logs...
if exist "D:\21maret\clean_code\ParkingServer\logs\server_error_%today%.log" (
    echo.
    echo Recent ParkingServer error log entries:
    echo -------------------------------------------
    type "D:\21maret\clean_code\ParkingServer\logs\server_error_%today%.log" | findstr /C:"ERROR" /C:"WARN" /C:"Exception"
    if %ERRORLEVEL% NEQ 0 (
        echo No errors or warnings found in ParkingServer logs.
    )
) else (
    echo No ParkingServer logs found for today.
)

echo.
echo ===================================================
echo.
pause
