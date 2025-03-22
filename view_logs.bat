@echo off
setlocal EnableDelayedExpansion
title Parking System Log Viewer

:menu
cls
echo ===================================================
echo      PARKING SYSTEM - LOG VIEWER UTILITY
echo ===================================================
echo.
echo  [1] View ParkingIN Logs
echo  [2] View ParkingOUT Logs  
echo  [3] View ParkingServer Logs
echo  [4] View All Logs (Errors and Warnings Only)
echo  [5] Show Log Directory Sizes
echo  [6] Exit
echo.
echo ===================================================
echo.

set /p choice=Enter your choice [1-6]: 

if "%choice%"=="1" (
    call :view_parkingin_logs
) else if "%choice%"=="2" (
    call :view_parkingout_logs
) else if "%choice%"=="3" (
    call :view_parkingserver_logs
) else if "%choice%"=="4" (
    call :view_all_errors
) else if "%choice%"=="5" (
    call :show_log_sizes
) else if "%choice%"=="6" (
    exit /b 0
) else (
    echo Invalid choice. Please try again.
    timeout /t 2 /nobreak > nul
    goto :menu
)

goto :menu

:view_parkingin_logs
    cls
    echo ===================================================
    echo              ParkingIN Log Files
    echo ===================================================
    echo.
    
    echo Available log files:
    echo -------------------
    dir /B "D:\21maret\clean_code\ParkingIN\logs\*.log" 2>nul
    echo.
    
    set /p logfile=Enter log filename to view (or press Enter to return): 
    
    if "!logfile!"=="" (
        goto :eof
    )
    
    if exist "D:\21maret\clean_code\ParkingIN\logs\!logfile!" (
        cls
        echo ===================================================
        echo          Contents of !logfile!
        echo ===================================================
        echo.
        type "D:\21maret\clean_code\ParkingIN\logs\!logfile!"
        echo.
        echo ===================================================
        pause
    ) else (
        echo File not found. Please check the filename.
        timeout /t 3 /nobreak > nul
    )
    
    goto :eof

:view_parkingout_logs
    cls
    echo ===================================================
    echo              ParkingOUT Log Files
    echo ===================================================
    echo.
    
    echo Available log files:
    echo -------------------
    dir /B "D:\21maret\clean_code\ParkingOut\Logs\*.log" 2>nul
    echo.
    
    set /p logfile=Enter log filename to view (or press Enter to return): 
    
    if "!logfile!"=="" (
        goto :eof
    )
    
    if exist "D:\21maret\clean_code\ParkingOut\Logs\!logfile!" (
        cls
        echo ===================================================
        echo          Contents of !logfile!
        echo ===================================================
        echo.
        type "D:\21maret\clean_code\ParkingOut\Logs\!logfile!"
        echo.
        echo ===================================================
        pause
    ) else (
        echo File not found. Please check the filename.
        timeout /t 3 /nobreak > nul
    )
    
    goto :eof

:view_parkingserver_logs
    cls
    echo ===================================================
    echo              ParkingServer Log Files
    echo ===================================================
    echo.
    
    echo Available log files:
    echo -------------------
    dir /B "D:\21maret\clean_code\ParkingServer\logs\*.log" 2>nul
    echo.
    
    set /p logfile=Enter log filename to view (or press Enter to return): 
    
    if "!logfile!"=="" (
        goto :eof
    )
    
    if exist "D:\21maret\clean_code\ParkingServer\logs\!logfile!" (
        cls
        echo ===================================================
        echo          Contents of !logfile!
        echo ===================================================
        echo.
        type "D:\21maret\clean_code\ParkingServer\logs\!logfile!"
        echo.
        echo ===================================================
        pause
    ) else (
        echo File not found. Please check the filename.
        timeout /t 3 /nobreak > nul
    )
    
    goto :eof

:view_all_errors
    cls
    echo ===================================================
    echo       All Errors and Warnings (Today's Logs)
    echo ===================================================
    echo.
    
    rem Get proper date format for log files - handle different regional settings
    for /f "tokens=2 delims==" %%a in ('wmic os get LocalDateTime /value') do set "dt=%%a"
    set "today=!dt:~0,8!"
    echo Today's date code: !today! (Format: YYYYMMDD)
    echo.
    
    echo ParkingIN Errors and Warnings:
    echo -----------------------------
    findstr /I /C:"ERROR" /C:"WARN" /C:"Exception" "D:\21maret\clean_code\ParkingIN\logs\app!today!.log" 2>nul
    if !ERRORLEVEL! NEQ 0 (
        echo No errors or warnings found in ParkingIN logs.
    )
    echo.
    
    echo ParkingOUT Errors and Warnings:
    echo -----------------------------
    findstr /I /C:"ERROR" /C:"WARN" /C:"Exception" "D:\21maret\clean_code\ParkingOut\Logs\app!today!.log" 2>nul
    if !ERRORLEVEL! NEQ 0 (
        echo No errors or warnings found in ParkingOUT logs.
    )
    echo.
    
    echo ParkingServer Errors and Warnings:
    echo -------------------------------
    findstr /I /C:"ERROR" /C:"WARN" /C:"Exception" "D:\21maret\clean_code\ParkingServer\logs\server_error_!today!.log" 2>nul
    if !ERRORLEVEL! NEQ 0 (
        echo No errors or warnings found in ParkingServer logs.
    )
    echo.
    
    echo ===================================================
    pause
    goto :eof

:show_log_sizes
    cls
    echo ===================================================
    echo           Log Directory Sizes
    echo ===================================================
    echo.
    
    echo Calculating log directory sizes...
    echo.
    
    echo ParkingIN Log Directory:
    echo --------------------------
    dir "D:\21maret\clean_code\ParkingIN\logs\*.log" /S /-C 2>nul | findstr "File(s)"
    echo.
    
    echo ParkingOUT Log Directory:
    echo --------------------------
    dir "D:\21maret\clean_code\ParkingOut\Logs\*.log" /S /-C 2>nul | findstr "File(s)"
    echo.
    
    echo ParkingServer Log Directory:
    echo --------------------------
    dir "D:\21maret\clean_code\ParkingServer\logs\*.log" /S /-C 2>nul | findstr "File(s)"
    echo.
    
    echo ===================================================
    pause
    goto :eof
