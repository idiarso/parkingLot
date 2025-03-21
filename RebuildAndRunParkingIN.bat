@echo off
echo ===================================================
echo Rebuilding and Testing ParkingIN Application
echo ===================================================

set PROJECT_DIR=D:\21maret\clean_code\ParkingIN
set CONFIG_FILE=%PROJECT_DIR%\bin\Release\net6.0-windows\ParkingIN.dll.config

echo.
echo Step 1: Changing to project directory...
cd /d "%PROJECT_DIR%"
if %ERRORLEVEL% NEQ 0 (
    echo Error: Could not change to project directory
    goto :error
)

echo.
echo Step 2: Cleaning previous build...
dotnet clean
if %ERRORLEVEL% NEQ 0 (
    echo Error: Clean failed
    goto :error
)
echo Previous build files cleaned.

echo.
echo Step 3: Building ParkingIN project...
dotnet build --configuration Release
if %ERRORLEVEL% NEQ 0 (
    echo Error: Build failed
    goto :error
)
echo Build completed successfully.

echo.
echo Step 4: Creating/Updating config file...
(
    echo ^<?xml version="1.0" encoding="utf-8" ?^>
    echo ^<configuration^>
    echo     ^<connectionStrings^>
    echo         ^<add name="ParkingDBConnection" connectionString="Host=localhost;Port=5432;Database=parkirdb;Username=postgres;Password=root@rsi" providerName="Npgsql" /^>
    echo     ^</connectionStrings^>
    echo ^</configuration^>
) > "%CONFIG_FILE%"
echo Config file updated successfully.

echo.
echo Step 5: Starting application...
start "" "%PROJECT_DIR%\bin\Release\net6.0-windows\ParkingIN.exe"
if %ERRORLEVEL% NEQ 0 (
    echo Error: Could not start application
    goto :error
)
echo Application started.

goto :end

:error
echo.
echo ===================================================
echo An error occurred. Please check the messages above.
echo ===================================================
exit /b 1

:end
echo.
echo ===================================================
echo Build and configuration completed successfully.
echo ===================================================
pause 