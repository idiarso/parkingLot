@echo off
echo Stopping PostgreSQL Services and Processes...

:: Stop the service using sc.exe
echo Stopping PostgreSQL service...
sc stop postgresql-x64-14
timeout /t 5

:: Forcefully terminate all postgres processes
echo.
echo Terminating all PostgreSQL processes...
taskkill /F /IM postgres.exe /T
taskkill /F /IM pg_ctl.exe /T
taskkill /F /IM pgbouncer.exe /T
taskkill /F /IM pgadmin4.exe /T

:: Double-check with process kill by PID
for /f "tokens=2" %%a in ('tasklist ^| findstr /i "postgres"') do (
    echo Stopping PID: %%a
    taskkill /F /PID %%a
)

echo.
echo Verifying processes...
tasklist | findstr /i "postgres"
if %ERRORLEVEL% EQU 0 (
    echo Some PostgreSQL processes are still running.
    echo You may need to restart your computer to fully stop PostgreSQL.
) else (
    echo All PostgreSQL processes have been stopped.
)

echo.
pause 