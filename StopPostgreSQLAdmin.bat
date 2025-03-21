@echo off
:: Check for admin privileges
net session >nul 2>&1
if %errorLevel% == 0 (
    goto :admin
) else (
    echo Requesting administrative privileges...
    powershell -Command "Start-Process '%~dpnx0' -Verb RunAs"
    exit /b
)

:admin
echo Stopping PostgreSQL Services and Processes...

:: Stop the service using sc.exe
echo Stopping PostgreSQL service...
net stop postgresql-x64-14
timeout /t 5

:: Forcefully terminate all postgres processes
echo.
echo Terminating all PostgreSQL processes...
taskkill /F /IM postgres.exe /T
taskkill /F /IM pg_ctl.exe /T

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