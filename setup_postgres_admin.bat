@echo off
NET SESSION >nul 2>&1
IF %ERRORLEVEL% NEQ 0 (
    echo This script requires administrator privileges
    echo Please right-click and select "Run as administrator"
    pause
    exit /b 1
)

echo ===================================================
echo PostgreSQL Configuration and Service Management
echo ===================================================

set PGHOME=C:\Program Files\PostgreSQL\14
set PGBIN=%PGHOME%\bin
set PGDATA=%PGHOME%\data

echo.
echo 1. Stopping PostgreSQL processes and service...
taskkill /F /IM postgres.exe /T 2>nul
taskkill /F /IM pg_ctl.exe /T 2>nul
net stop postgresql-x64-14 2>nul
timeout /t 5

echo.
echo 2. Backing up configuration...
copy /Y "%PGDATA%\postgresql.conf" "%PGDATA%\postgresql.conf.backup"
copy /Y "%PGDATA%\pg_hba.conf" "%PGDATA%\pg_hba.conf.backup"

echo.
echo 3. Updating postgresql.conf...
findstr /v /i "listen_addresses" "%PGDATA%\postgresql.conf" > "%PGDATA%\postgresql.conf.tmp"
echo listen_addresses = '*' >> "%PGDATA%\postgresql.conf.tmp"
move /Y "%PGDATA%\postgresql.conf.tmp" "%PGDATA%\postgresql.conf"

echo.
echo 4. Updating pg_hba.conf...
echo # TYPE  DATABASE        USER            ADDRESS                 METHOD > "%PGDATA%\pg_hba.conf"
echo local   all            all                                     trust >> "%PGDATA%\pg_hba.conf"
echo host    all            all             127.0.0.1/32            trust >> "%PGDATA%\pg_hba.conf"
echo host    all            all             ::1/128                 trust >> "%PGDATA%\pg_hba.conf"
echo host    all            all             0.0.0.0/0               trust >> "%PGDATA%\pg_hba.conf"

echo.
echo 5. Setting permissions...
icacls "%PGDATA%\postgresql.conf" /grant "NT AUTHORITY\NetworkService":(F)
icacls "%PGDATA%\pg_hba.conf" /grant "NT AUTHORITY\NetworkService":(F)

echo.
echo 6. Starting PostgreSQL service...
net start postgresql-x64-14
timeout /t 5

echo.
echo 7. Testing connection...
set PGUSER=postgres
set PGPASSWORD=root@rsi
"%PGBIN%\psql.exe" -U postgres -c "\conninfo"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo PostgreSQL is now configured and accepting connections.
    
    echo.
    echo 8. Updating authentication method to scram-sha-256...
    echo # TYPE  DATABASE        USER            ADDRESS                 METHOD > "%PGDATA%\pg_hba.conf"
    echo local   all            all                                     scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    echo host    all            all             127.0.0.1/32            scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    echo host    all            all             ::1/128                 scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    echo host    all            all             0.0.0.0/0               scram-sha-256 >> "%PGDATA%\pg_hba.conf"
    
    net stop postgresql-x64-14
    timeout /t 5
    net start postgresql-x64-14
    timeout /t 5
    
    echo.
    echo Configuration completed successfully.
) else (
    echo Failed to connect to PostgreSQL.
    echo Please check the configuration and try again.
    goto :error
)

goto :end

:error
echo.
echo ===================================================
echo Error occurred during configuration
echo ===================================================
exit /b 1

:end
echo.
echo Press any key to exit...
pause 