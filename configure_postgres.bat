@echo off
echo ===================================================
echo Configuring PostgreSQL for TCP/IP Connections
echo ===================================================

set PGHOME=C:\Program Files\PostgreSQL\14
set PGBIN=%PGHOME%\bin
set PGDATA=%PGHOME%\data

echo.
echo 1. Checking PostgreSQL installation...
if not exist "%PGBIN%\pg_ctl.exe" (
    echo PostgreSQL binaries not found at %PGBIN%
    echo Please ensure PostgreSQL 14 is installed correctly.
    goto :error
)

echo.
echo 2. Stopping PostgreSQL service...
"%PGBIN%\pg_ctl.exe" stop -D "%PGDATA%" -m fast

echo.
echo 3. Backing up configuration files...
copy /Y "%PGDATA%\postgresql.conf" "%PGDATA%\postgresql.conf.backup"
copy /Y "%PGDATA%\pg_hba.conf" "%PGDATA%\pg_hba.conf.backup"

echo.
echo 4. Updating postgresql.conf...
echo listen_addresses = '*' > "%PGDATA%\postgresql.conf.tmp"
type "%PGDATA%\postgresql.conf" | findstr /v "listen_addresses" >> "%PGDATA%\postgresql.conf.tmp"
move /Y "%PGDATA%\postgresql.conf.tmp" "%PGDATA%\postgresql.conf"

echo.
echo 5. Updating pg_hba.conf...
(
echo # TYPE  DATABASE        USER            ADDRESS                 METHOD
echo local   all            all                                     scram-sha-256
echo host    all            all             127.0.0.1/32            scram-sha-256
echo host    all            all             ::1/128                 scram-sha-256
echo host    all            all             0.0.0.0/0               scram-sha-256
) > "%PGDATA%\pg_hba.conf"

echo.
echo 6. Starting PostgreSQL service...
"%PGBIN%\pg_ctl.exe" start -D "%PGDATA%" -w

echo.
echo 7. Testing connection...
set PGUSER=postgres
set PGPASSWORD=root@rsi
"%PGBIN%\psql.exe" -U postgres -c "\conninfo"

if %ERRORLEVEL% EQU 0 (
    echo PostgreSQL is now configured and accepting connections.
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
echo Configuration completed. Press any key to exit...
pause 