@echo off
setlocal EnableDelayedExpansion
title ParkingIN System Launcher

:menu
cls
echo.
echo ============================================
echo      PARKING SYSTEM - LAUNCHER UTILITY
echo ============================================
echo.
echo  [1] Run All Applications
echo  [2] Run ParkingServer Only
echo  [3] Run ParkingIN Only
echo  [4] Run ParkingOut Only
echo  [5] Check Application Status
echo  [6] Check PostgreSQL Service
echo  [7] Test Database Connection
echo  [8] Restart All Services
echo  [9] Exit
echo.
echo ============================================
echo.

set /p choice=Enter your choice [1-9]: 

if "%choice%"=="1" (
    call :run_all
) else if "%choice%"=="2" (
    call :run_server
) else if "%choice%"=="3" (
    call :run_parkingin
) else if "%choice%"=="4" (
    call :run_parkingout
) else if "%choice%"=="5" (
    call :check_status
) else if "%choice%"=="6" (
    call :check_postgres
) else if "%choice%"=="7" (
    call :test_database
) else if "%choice%"=="8" (
    call :restart_services
) else if "%choice%"=="9" (
    exit /b 0
) else (
    echo Invalid choice. Please try again.
    timeout /t 2 /nobreak > nul
    goto :menu
)

goto :menu

:run_all
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
    pause
    goto :eof

:run_server
    echo.
    echo Starting Parking Server...
    echo.
    
    echo Starting PostgreSQL first...
    cd /d D:\21maret\clean_code\ParkingIN\Database
    call StartPostgreSQL.bat
    timeout /t 5 /nobreak > nul
    
    start cmd /k "cd /d D:\21maret\clean_code\ParkingServer && dotnet run"
    
    echo Server started successfully.
    pause
    goto :eof

:run_parkingin
    echo.
    echo Starting ParkingIN Application...
    echo.
    
    echo Starting PostgreSQL first...
    cd /d D:\21maret\clean_code\ParkingIN\Database
    call StartPostgreSQL.bat
    timeout /t 5 /nobreak > nul
    
    start cmd /k "cd /d D:\21maret\clean_code\ParkingIN && dotnet run"
    
    echo ParkingIN started successfully.
    pause
    goto :eof

:run_parkingout
    echo.
    echo Starting ParkingOut Application...
    echo.
    
    echo Checking PostgreSQL first...
    cd /d D:\21maret\clean_code\ParkingIN\Database
    call StartPostgreSQL.bat
    timeout /t 5 /nobreak > nul
    
    echo Building ParkingOut first...
    cd /d D:\21maret\clean_code\ParkingOut
    dotnet build
    
    if %ERRORLEVEL% NEQ 0 (
        echo.
        echo Error building ParkingOut. Please check the output above.
        pause
        goto :eof
    )
    
    echo.
    echo Build successful. Starting ParkingOut...
    start cmd /k "cd /d D:\21maret\clean_code\ParkingOut && dotnet run"
    
    echo ParkingOut started successfully.
    pause
    goto :eof

:check_status
    echo.
    echo Checking Parking System Status...
    echo.
    
    set server_running=0
    set parkingin_running=0
    set parkingout_running=0
    set postgres_running=0
    
    for /f "tokens=1" %%p in ('tasklist /fi "IMAGENAME eq postgres.exe" ^| find /i "postgres.exe"') do (
        set postgres_running=1
    )
    
    for /f "tokens=1" %%p in ('tasklist /fi "IMAGENAME eq dotnet.exe" ^| find /c /i "dotnet.exe"') do (
        if %%p GTR 0 set server_running=1
    )
    
    for /f "tokens=1" %%p in ('tasklist /fi "IMAGENAME eq ParkingIN.exe" ^| find /i "ParkingIN.exe"') do (
        set parkingin_running=1
    )
    
    for /f "tokens=1" %%p in ('tasklist /fi "IMAGENAME eq ParkingOut.exe" ^| find /i "ParkingOut.exe"') do (
        set parkingout_running=1
    )
    
    echo Status:
    echo.
    if !postgres_running!==1 (
        echo PostgreSQL: RUNNING [OK]
    ) else (
        echo PostgreSQL: STOPPED [ERROR]
    )
    
    if !server_running!==1 (
        echo ParkingServer: RUNNING [OK]
    ) else (
        echo ParkingServer: STOPPED [ERROR]
    )
    
    if !parkingin_running!==1 (
        echo ParkingIN: RUNNING [OK]
    ) else (
        echo ParkingIN: STOPPED [ERROR]
    )
    
    if !parkingout_running!==1 (
        echo ParkingOut: RUNNING [OK]
    ) else (
        echo ParkingOut: STOPPED [ERROR]
    )
    
    echo.
    pause
    goto :eof

:check_postgres
    echo.
    echo Checking PostgreSQL service...
    echo.
    
    cd /d D:\21maret\clean_code\ParkingIN\Database
    call StartPostgreSQL.bat
    
    echo.
    pause
    goto :eof

:test_database
    echo.
    echo Testing database connection...
    echo.
    
    cd /d D:\21maret\clean_code\ParkingIN
    dotnet run -- --test-connection
    
    echo.
    pause
    goto :eof

:restart_services
    echo.
    echo Restarting all services...
    echo.
    
    echo Stopping running processes...
    taskkill /F /IM ParkingIN.exe 2>nul
    taskkill /F /IM ParkingOut.exe 2>nul
    for /f "tokens=2" %%p in ('tasklist /fi "WINDOWTITLE eq Parking System Server" /fo csv ^| find /i "cmd.exe"') do (
        taskkill /F /PID %%p 2>nul
    )
    
    echo Restarting PostgreSQL...
    cd /d D:\21maret\clean_code\ParkingIN\Database
    call StartPostgreSQL.bat
    
    echo All services restarted successfully.
    echo.
    pause
    goto :eof
