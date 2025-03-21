@echo off
echo ===================================================
echo Restarting PostgreSQL Server
echo ===================================================

set PGHOME=C:\Program Files\PostgreSQL\14
set PGBIN=%PGHOME%\bin
set PGDATA=%PGHOME%\data

echo.
echo 1. Stopping all PostgreSQL processes...
taskkill /F /IM postgres.exe /T 2>nul
taskkill /F /IM pg_ctl.exe /T 2>nul
net stop postgresql-x64-14 2>nul
timeout /t 5

echo.
echo 2. Verifying port 5432 is free...
netstat -ano | findstr :5432
if %ERRORLEVEL% EQU 0 (
    echo Port 5432 is still in use. Please check running processes.
    goto :error
)

echo.
echo 3. Starting PostgreSQL service...
net start postgresql-x64-14
timeout /t 5

echo.
echo 4. Testing connection...
set PGUSER=postgres
set PGPASSWORD=root@rsi
"%PGBIN%\psql.exe" -U postgres -c "\conninfo"

if %ERRORLEVEL% EQU 0 (
    echo PostgreSQL server is now running and accepting connections.
) else (
    echo Failed to connect to PostgreSQL.
    echo Please check the configuration and try again.
    goto :error
)

goto :end

:error
echo.
echo ===================================================
echo Error occurred during restart
echo ===================================================
exit /b 1

:end
echo.
echo Restart completed. Press any key to exit...
pause 