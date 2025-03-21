@echo off
echo ===================================================
echo Initializing ParkingIN Database
echo ===================================================

set PGPASSWORD=root@rsi

echo.
echo 1. Creating and initializing database...
"C:\Program Files\PostgreSQL\14\bin\psql.exe" -U postgres -h 127.0.0.1 -f InitDatabase.sql

echo.
echo 2. Verifying database objects...
"C:\Program Files\PostgreSQL\14\bin\psql.exe" -U postgres -h 127.0.0.1 -d parkirdb -c "\dt"
"C:\Program Files\PostgreSQL\14\bin\psql.exe" -U postgres -h 127.0.0.1 -d parkirdb -c "SELECT id, username, role, status FROM t_user;"
"C:\Program Files\PostgreSQL\14\bin\psql.exe" -U postgres -h 127.0.0.1 -d parkirdb -c "SELECT * FROM t_setting;"

echo.
echo Press any key to exit...
pause 