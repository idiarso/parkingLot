@echo off
echo ===================================================
echo Testing Database Connection and Initialization
echo ===================================================

set PGHOME=C:\Program Files\PostgreSQL\14
set PGBIN="%PGHOME%\bin"
set PGUSER=postgres
set PGPASSWORD=root@rsi
set PGDATABASE=parkirdb

echo.
echo 1. Testing PostgreSQL Connection...
%PGBIN%\psql -U postgres -c "\conninfo"

if %ERRORLEVEL% NEQ 0 (
    echo Failed to connect to PostgreSQL.
    echo Please run ConfigurePostgreSQL.bat first.
    goto :error
)

echo.
echo 2. Creating Database...
%PGBIN%\psql -U postgres -c "CREATE DATABASE parkirdb WITH ENCODING='UTF8' OWNER=postgres;" 2>nul
if %ERRORLEVEL% NEQ 0 (
    echo Note: Database may already exist.
)

echo.
echo 3. Creating Tables...
%PGBIN%\psql -U postgres -d parkirdb -f InitDatabase.sql

echo.
echo 4. Verifying Database Objects...
echo.
echo Tables:
%PGBIN%\psql -U postgres -d parkirdb -c "\dt"
echo.
echo Users:
%PGBIN%\psql -U postgres -d parkirdb -c "SELECT id, username, role, status FROM t_user;"
echo.
echo Settings:
%PGBIN%\psql -U postgres -d parkirdb -c "SELECT * FROM t_setting;"

goto :end

:error
echo.
echo ===================================================
echo Error occurred during database setup
echo ===================================================
exit /b 1

:end
echo.
echo Database setup completed successfully.
echo You can now start the ParkingIN application.
pause 