@echo off
echo ===================================================
echo          PARKING SYSTEM - STATUS MONITOR
echo ===================================================
echo.
echo System Status Check: %date% %time%
echo.

rem Get proper date format for log files
for /f "tokens=2 delims==" %%a in ('wmic os get LocalDateTime /value') do set "dt=%%a"
set "today=%dt:~0,8%"

echo HARDWARE STATUS:
echo ----------------
echo.

echo 1. Checking PostgreSQL Service...
sc query postgresql-x64-12 | findstr "STATE" | findstr "RUNNING"
if %ERRORLEVEL% EQU 0 (
    echo   [OK] PostgreSQL Service is running
) else (
    echo   [ERROR] PostgreSQL Service is not running
    echo   Try running StartPostgreSQL.bat as administrator
)
echo.

echo 2. Checking Camera Connection...
if exist "D:\21maret\clean_code\ParkingIN\config\camera.ini" (
    echo   [INFO] Camera configuration found
    type "D:\21maret\clean_code\ParkingIN\config\camera.ini" | findstr "Type"
    type "D:\21maret\clean_code\ParkingIN\config\camera.ini" | findstr "IP_Address"
) else (
    echo   [WARN] Camera configuration not found
)
echo.

echo 3. Checking Gate Controller...
if exist "D:\21maret\clean_code\ParkingIN\config\gate.ini" (
    echo   [INFO] Gate configuration found
    type "D:\21maret\clean_code\ParkingIN\config\gate.ini" | findstr "COM_Port"
    type "D:\21maret\clean_code\ParkingIN\config\gate.ini" | findstr "Baud_Rate"
) else (
    echo   [WARN] Gate configuration not found
)
echo.

echo 4. Checking Printer Configuration...
if exist "D:\21maret\clean_code\ParkingIN\config\printer.ini" (
    echo   [INFO] Printer configuration found
    type "D:\21maret\clean_code\ParkingIN\config\printer.ini" | findstr "Printer_Name"
) else (
    echo   [WARN] Printer configuration not found
)
echo.

echo APPLICATION STATUS:
echo ------------------
echo.

echo 1. Checking Running Applications...
tasklist /FI "IMAGENAME eq ParkingIN.exe" | findstr "ParkingIN.exe"
if %ERRORLEVEL% EQU 0 (
    echo   [RUNNING] ParkingIN.exe
) else (
    echo   [STOPPED] ParkingIN.exe
)

tasklist /FI "IMAGENAME eq ParkingOUT.exe" | findstr "ParkingOUT.exe"
if %ERRORLEVEL% EQU 0 (
    echo   [RUNNING] ParkingOUT.exe
) else (
    echo   [STOPPED] ParkingOUT.exe
)

tasklist /FI "IMAGENAME eq ParkingServer.exe" | findstr "ParkingServer.exe"
if %ERRORLEVEL% EQU 0 (
    echo   [RUNNING] ParkingServer.exe
) else (
    echo   [STOPPED] ParkingServer.exe
)

for /f "tokens=1" %%p in ('tasklist /fi "WINDOWTITLE eq ParkingIN" ^| find /i "dotnet.exe"') do (
    echo   [RUNNING] ParkingIN (dotnet)
    set parkingin_running=1
)

for /f "tokens=1" %%p in ('tasklist /fi "WINDOWTITLE eq ParkingOUT" ^| find /i "dotnet.exe"') do (
    echo   [RUNNING] ParkingOUT (dotnet)
    set parkingout_running=1
)

for /f "tokens=1" %%p in ('tasklist /fi "WINDOWTITLE eq ParkingServer" ^| find /i "dotnet.exe"') do (
    echo   [RUNNING] ParkingServer (dotnet)
    set server_running=1
)
echo.

echo LOG STATUS:
echo -----------
echo.

echo 1. ParkingIN Logs:
if exist "D:\21maret\clean_code\ParkingIN\logs\app%today%.log" (
    echo   [FOUND] Today's log: app%today%.log
    for %%F in ("D:\21maret\clean_code\ParkingIN\logs\app%today%.log") do echo   Size: %%~zF bytes
    
    echo   Recent activity:
    type "D:\21maret\clean_code\ParkingIN\logs\app%today%.log" | findstr /C:"ERROR" /C:"WARN"
    if %ERRORLEVEL% NEQ 0 (
        echo   No errors or warnings found.
    )
) else (
    echo   [NOT FOUND] Today's log: app%today%.log
)
echo.

echo 2. ParkingOUT Logs:
if exist "D:\21maret\clean_code\ParkingOut\Logs\app%today%.log" (
    echo   [FOUND] Today's log: app%today%.log
    for %%F in ("D:\21maret\clean_code\ParkingOut\Logs\app%today%.log") do echo   Size: %%~zF bytes
    
    echo   Recent activity:
    type "D:\21maret\clean_code\ParkingOut\Logs\app%today%.log" | findstr /C:"ERROR" /C:"WARN"
    if %ERRORLEVEL% NEQ 0 (
        echo   No errors or warnings found.
    )
) else (
    echo   [NOT FOUND] Today's log: app%today%.log
)
echo.

echo 3. ParkingServer Logs:
if exist "D:\21maret\clean_code\ParkingServer\logs\server_error_%today%.log" (
    echo   [FOUND] Today's log: server_error_%today%.log
    for %%F in ("D:\21maret\clean_code\ParkingServer\logs\server_error_%today%.log") do echo   Size: %%~zF bytes
    
    echo   Recent activity:
    type "D:\21maret\clean_code\ParkingServer\logs\server_error_%today%.log" | findstr /C:"ERROR" /C:"WARN"
    if %ERRORLEVEL% NEQ 0 (
        echo   No errors or warnings found.
    )
) else (
    echo   [NOT FOUND] Today's log: server_error_%today%.log
)
echo.

echo ===================================================
echo HARDWARE CONNECTIONS:
echo ===================================================

echo 1. Available COM Ports:
mode | findstr "COM"
echo.

echo 2. Available Printers:
wmic printer get name
echo.

echo 3. Available Network Connections:
ipconfig | findstr "IPv4"
echo.

echo ===================================================
echo.
echo For more detailed log information, run view_logs.bat
echo To force close applications, run force_close.bat
echo.
echo ===================================================

pause
