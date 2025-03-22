@echo off
echo ===================================================
echo       Quick Log Check for Parking System
echo ===================================================
echo.

rem Get proper date format for log files - handle different regional settings
for /f "tokens=2 delims==" %%a in ('wmic os get LocalDateTime /value') do set "dt=%%a"
set "today=%dt:~0,8%"
echo Today's date code: %today% (Format: YYYYMMDD)
echo.

echo Checking Log Directories...
echo.

rem Function to get latest log file
goto :start

:getLatestLogFile
set "latestLog="
set "latestDate=0"
for %%F in ("%~1\*.log") do (
    for /f "tokens=2 delims=_" %%D in ("%%~nF") do (
        if "%%~xF"==".log" (
            set "logDate=%%D"
            if "!logDate!" gtr "!latestDate!" (
                set "latestDate=%%D"
                set "latestLog=%%~nxF"
            )
        )
    )
)
exit /b

:start
echo 1. ParkingIN Logs:
echo ------------------
if exist "D:\21maret\clean_code\ParkingIN\logs" (
    echo   Directory exists: D:\21maret\clean_code\ParkingIN\logs
    echo   Files:
    dir /B "D:\21maret\clean_code\ParkingIN\logs\*.log" 2>nul | findstr "."
    if %ERRORLEVEL% NEQ 0 (
        echo   No log files found.
    )
    
    echo.
    echo   Latest Log Files:
    call :getLatestLogFile "D:\21maret\clean_code\ParkingIN\logs"
    if defined latestLog (
        echo   Latest: !latestLog!
        echo   Size:
        for %%F in ("D:\21maret\clean_code\ParkingIN\logs\!latestLog!") do @echo   %%~zF bytes
        
        echo   Recent activity:
        type "D:\21maret\clean_code\ParkingIN\logs\!latestLog!" | findstr /C:"ERROR" /C:"WARN" /C:"Exception"
        if %ERRORLEVEL% NEQ 0 (
            echo   No errors or warnings found.
        )
    ) else (
        echo   No log files found.
    )
) else (
    echo   Directory does not exist: D:\21maret\clean_code\ParkingIN\logs
)
echo.

echo 2. ParkingOUT Logs:
echo ------------------
if exist "D:\21maret\clean_code\ParkingOut\Logs" (
    echo   Directory exists: D:\21maret\clean_code\ParkingOut\Logs
    echo   Files:
    dir /B "D:\21maret\clean_code\ParkingOut\Logs\*.log" 2>nul | findstr "."
    if %ERRORLEVEL% NEQ 0 (
        echo   No log files found.
    )
    
    echo.
    echo   Latest Log Files:
    call :getLatestLogFile "D:\21maret\clean_code\ParkingOut\Logs"
    if defined latestLog (
        echo   Latest: !latestLog!
        echo   Size:
        for %%F in ("D:\21maret\clean_code\ParkingOut\Logs\!latestLog!") do @echo   %%~zF bytes
        
        echo   Recent activity:
        type "D:\21maret\clean_code\ParkingOut\Logs\!latestLog!" | findstr /C:"ERROR" /C:"WARN" /C:"Exception"
        if %ERRORLEVEL% NEQ 0 (
            echo   No errors or warnings found.
        )
    ) else (
        echo   No log files found.
    )
) else (
    echo   Directory does not exist: D:\21maret\clean_code\ParkingOut\Logs
)
echo.

echo 3. ParkingServer Logs:
echo --------------------
if exist "D:\21maret\clean_code\ParkingServer\logs" (
    echo   Directory exists: D:\21maret\clean_code\ParkingServer\logs
    echo   Files:
    dir /B "D:\21maret\clean_code\ParkingServer\logs\*.log" 2>nul | findstr "."
    if %ERRORLEVEL% NEQ 0 (
        echo   No log files found.
    )
    
    echo.
    echo   Latest Log Files:
    call :getLatestLogFile "D:\21maret\clean_code\ParkingServer\logs"
    if defined latestLog (
        echo   Latest: !latestLog!
        echo   Size:
        for %%F in ("D:\21maret\clean_code\ParkingServer\logs\!latestLog!") do @echo   %%~zF bytes
        
        echo   Recent activity:
        type "D:\21maret\clean_code\ParkingServer\logs\!latestLog!" | findstr /C:"ERROR" /C:"WARN" /C:"Exception"
        if %ERRORLEVEL% NEQ 0 (
            echo   No errors or warnings found.
        )
    ) else (
        echo   No log files found.
    )
) else (
    echo   Directory does not exist: D:\21maret\clean_code\ParkingServer\logs
)
echo.

echo ===================================================
echo.
pause
