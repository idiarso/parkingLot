@echo off
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo This script requires administrator privileges
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo ===================================================
echo Direct PostgreSQL Startup
echo ===================================================

set PGHOME=C:\Program Files\PostgreSQL\14
set PGBIN=%PGHOME%\bin
set PGDATA=%PGHOME%\data
set PGLOG=%PGDATA%\pg_log\startup.log

echo.
echo 1. Stopping any existing PostgreSQL processes...
taskkill /F /IM postgres.exe /T 2>nul
taskkill /F /IM pg_ctl.exe /T 2>nul
net stop postgresql-x64-14 2>nul
timeout /t 5

echo.
echo 2. Updating postgresql.conf...
findstr /v /i "listen_addresses port" "%PGDATA%\postgresql.conf" > "%PGDATA%\postgresql.conf.tmp"
echo listen_addresses = '*' >> "%PGDATA%\postgresql.conf.tmp"
echo port = 5432 >> "%PGDATA%\postgresql.conf.tmp"
move /Y "%PGDATA%\postgresql.conf.tmp" "%PGDATA%\postgresql.conf"

echo.
echo 3. Updating pg_hba.conf...
echo # TYPE  DATABASE        USER            ADDRESS                 METHOD > "%PGDATA%\pg_hba.conf"
echo local   all            all                                     trust >> "%PGDATA%\pg_hba.conf"
echo host    all            all             127.0.0.1/32            trust >> "%PGDATA%\pg_hba.conf"
echo host    all            all             ::1/128                 trust >> "%PGDATA%\pg_hba.conf"
echo host    all            all             0.0.0.0/0               trust >> "%PGDATA%\pg_hba.conf"

echo.
echo 4. Setting permissions...
icacls "%PGDATA%" /grant "NT AUTHORITY\NetworkService":(OI)(CI)F
icacls "%PGDATA%\postgresql.conf" /grant "NT AUTHORITY\NetworkService":(F)
icacls "%PGDATA%\pg_hba.conf" /grant "NT AUTHORITY\NetworkService":(F)

echo.
echo 5. Starting PostgreSQL directly...
mkdir "%PGDATA%\pg_log" 2>nul
"%PGBIN%\pg_ctl.exe" start -D "%PGDATA%" -l "%PGLOG%" -w -t 60

echo.
echo 6. Testing connection...
timeout /t 5
set PGUSER=postgres
set PGPASSWORD=root@rsi
"%PGBIN%\psql.exe" -U postgres -c "\conninfo"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo PostgreSQL is now running and accepting connections.
    
    echo.
    echo 7. Updating authentication method to scram-sha-256...
    echo # TYPE  DATABASE        USER            ADDRESS                 METHOD > "%PGDATA%\pg_hba.conf"
    echo local   all            all                                     scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    echo host    all            all             127.0.0.1/32            scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    echo host    all            all             ::1/128                 scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    echo host    all            all             0.0.0.0/0               scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    
    "%PGBIN%\pg_ctl.exe" restart -D "%PGDATA%" -l "%PGLOG%" -w -t 60
    
    echo.
    echo Configuration completed successfully.
) else (
    echo Failed to connect to PostgreSQL.
    echo Checking log file for errors:
    type "%PGLOG%"
    goto :error
)

goto :end

:error
echo.
echo ===================================================
echo Error occurred during startup
echo ===================================================
exit /b 1

:end
echo.
echo Press any key to exit...
pause 