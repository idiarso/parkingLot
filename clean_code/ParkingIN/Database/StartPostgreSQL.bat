@echo off
echo Checking PostgreSQL service status...

REM Check if running with admin privileges
net session >nul 2>&1
if %errorLevel% NEQ 0 (
    echo This script requires administrator privileges.
    echo Please right-click and select "Run as administrator".
    pause
    exit /b 1
)

REM Try to find the PostgreSQL service by name (supports different versions)
for /f "tokens=*" %%a in ('sc query state^=all ^| findstr /i "postgresql"') do (
    for /f "tokens=1,2 delims=: " %%b in ('sc query %%a ^| findstr /i "SERVICE_NAME"') do (
        set SERVICE_NAME=%%c
    )
)

REM If service name found, check its status
if defined SERVICE_NAME (
    echo Found PostgreSQL service: %SERVICE_NAME%
    
    REM Check service status
    sc query %SERVICE_NAME% | findstr "RUNNING" > nul
    if %errorlevel% equ 0 (
        echo PostgreSQL service is already running.
    ) else (
        echo Starting PostgreSQL service...
        net start %SERVICE_NAME%
        
        if %errorlevel% equ 0 (
            echo PostgreSQL service started successfully.
        ) else (
            echo Failed to start PostgreSQL service. Error code: %errorlevel%
        )
    )
) else (
    echo No PostgreSQL service found. Please ensure PostgreSQL is installed correctly.
    exit /b 1
)

REM Test the database connection
echo Testing database connection...
set PGPASSWORD=root@rsi
psql -h localhost -U postgres -p 5432 -c "SELECT 1 AS connection_test;" > nul 2>&1

if %errorlevel% equ 0 (
    echo Database connection successful!
) else (
    echo Failed to connect to the database.
    echo Please check your PostgreSQL installation and credentials.
)

REM Clear password from environment
set PGPASSWORD=

echo.
echo PostgreSQL service check complete.
echo.
pause
