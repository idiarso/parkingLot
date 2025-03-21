@echo off
echo Closing Setup Process...
taskkill /F /FI "WINDOWTITLE eq CompleteSetupAndTest.bat" /T
taskkill /F /IM cmd.exe /FI "WINDOWTITLE eq CompleteSetupAndTest.bat" /T

echo.
echo Setup process closed.
timeout /t 2 >nul 