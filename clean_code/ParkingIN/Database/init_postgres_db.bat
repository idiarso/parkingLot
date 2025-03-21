@echo off
echo =================================================
echo Initializing PostgreSQL Database for ParkingIN
echo =================================================
echo.

REM Check if PostgreSQL is installed
SET PGBIN=
FOR /F "tokens=*" %%A IN ('where /r "C:\Program Files\PostgreSQL" psql.exe 2^>NUL') DO (
    SET PGBIN=%%~dpA
    goto :found
)
FOR /F "tokens=*" %%A IN ('where /r "C:\Program Files (x86)\PostgreSQL" psql.exe 2^>NUL') DO (
    SET PGBIN=%%~dpA
    goto :found
)

echo PostgreSQL psql.exe not found in standard locations, please install PostgreSQL or update PATH.
echo Trying with default PostgreSQL commands...
:found

echo 1. Checking PostgreSQL Service...
net start postgresql-x64-14
timeout /t 0 /nobreak
echo.

REM Set postgres password as environment variable for this session
SET PGPASSWORD=root@rsi

echo 2. Creating Database if not exists...
psql -h localhost -U postgres -p 5432 -c "SELECT 1 FROM pg_database WHERE datname='parkirdb'" | findstr "1" > nul
IF %ERRORLEVEL% NEQ 0 (
    echo Creating parkirdb database...
    psql -h localhost -U postgres -p 5432 -c "CREATE DATABASE parkirdb WITH ENCODING='UTF8';"
) ELSE (
    echo Database parkirdb already exists.
)
echo.

echo 3. Initializing Tables...
psql -h localhost -U postgres -p 5432 -d parkirdb -f "%~dp0postgresql_schema.sql"
echo.

echo 4. Verifying Database...
psql -h localhost -U postgres -p 5432 -d parkirdb -c "\dt"
psql -h localhost -U postgres -p 5432 -d parkirdb -c "SELECT * FROM t_user WHERE username='admin';"
echo.

REM Clear password from environment
SET PGPASSWORD=

echo.
echo Database initialization completed.
echo.
echo Now you can:
echo 1. Start ParkingIN application
echo 2. Login with:
echo    Username: admin
echo    Password: admin123
echo.
pause
