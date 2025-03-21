@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo Complete PostgreSQL Setup and Test for ParkingIN
echo ===================================================

echo.
echo This script will:
echo 1. Set up the PostgreSQL database
echo 2. Build the ParkingIN application
echo 3. Run the ParkingIN application
echo.
echo Press any key to start...
pause > nul

echo.
echo ===================================================
echo STEP 1: Setting up PostgreSQL Database
echo ===================================================

:: Set variables
set PGHOST=localhost
set PGPORT=5432
set PGUSER=postgres
set PGPASSWORD=root@rsi
set PGDATABASE=parkirdb

echo Current database settings:
echo Host: %PGHOST%
echo Port: %PGPORT%
echo User: %PGUSER%
echo Password: [hidden]
echo Database: %PGDATABASE%
echo.

:: Check if psql is in PATH or in Program Files
set PSQL_PATH=
for %%p in (
    "C:\Program Files\PostgreSQL\15\bin\psql.exe"
    "C:\Program Files\PostgreSQL\14\bin\psql.exe"
    "C:\Program Files\PostgreSQL\13\bin\psql.exe"
) do (
    if exist %%p (
        set "PSQL_PATH=%%p"
        goto :found_psql
    )
)

:check_path
where psql >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    set "PSQL_PATH=psql"
    goto :found_psql
)

echo PostgreSQL client tools (psql) not found.
echo Please ensure PostgreSQL is installed and added to PATH.
echo Skipping database setup...
goto :build_app

:found_psql
echo Found PostgreSQL at: !PSQL_PATH!
echo.

echo Creating database if it doesn't exist...
"!PSQL_PATH!" -h %PGHOST% -p %PGPORT% -U %PGUSER% -d postgres -c "CREATE DATABASE %PGDATABASE% WITH ENCODING='UTF8' OWNER=%PGUSER%;" 2>nul

echo.
echo Running SQL script to create tables...
if not exist create_tables.sql (
    echo Error: create_tables.sql file not found.
    echo Please make sure the SQL script is in the current directory.
    goto :error
)

"!PSQL_PATH!" -h %PGHOST% -p %PGPORT% -U %PGUSER% -d %PGDATABASE% -f create_tables.sql
if %ERRORLEVEL% NEQ 0 (
    echo Error running SQL script. Please check error messages above.
    goto :error
)

:build_app
echo ===================================================
echo STEP 2: Building ParkingIN Application
echo ===================================================

echo.
echo Changing to project directory...
cd /d "D:\21maret\clean_code\ParkingIN"
if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to change directory to D:\21maret\clean_code\ParkingIN
    goto :error
)

echo.
echo Current directory: %CD%
echo Available project files:
dir /b *.csproj

echo.
echo Cleaning previous build and logs...
rmdir /s /q bin 2>nul
rmdir /s /q obj 2>nul
echo Previous build files cleaned.

echo.
echo Building ParkingIN project...
dotnet build ParkingIN.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Error: Build failed
    goto :error
)

echo.
echo ===================================================
echo STEP 3: Running ParkingIN Application
echo ===================================================

echo.
echo Creating logs directory...
mkdir "bin\Release\net6.0-windows\logs" 2>nul

echo.
echo Starting ParkingIN.exe...
echo.
echo Please test the following:
echo - Login with username "admin" and password "admin"
echo.
echo If the login fails, check the logs folder for database.log
echo.

cd bin\Release\net6.0-windows
start ParkingIN.exe
if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to start ParkingIN.exe
    goto :error
)

echo.
echo Application started successfully.
goto :end

:error
echo.
echo ===================================================
echo An error occurred. Please check the messages above.
echo ===================================================
exit /b 1

:end
echo.
echo Process completed. Press any key to exit...
pause > nul
exit /b 0 