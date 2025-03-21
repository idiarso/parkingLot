@echo off
echo ===================================================
echo Testing Simplified Database Approach
echo ===================================================

echo.
echo Step 1: Changing to project directory...
cd D:\21maret\clean_code\ParkingIN
if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to change directory to D:\21maret\clean_code\ParkingIN
    goto :error
)

echo.
echo Current directory: %CD%
echo Available project files:
dir /b *.csproj

echo.
echo Step 2: Cleaning previous build and logs...
rmdir /s /q bin 2>nul
rmdir /s /q obj 2>nul
echo Previous build files cleaned.

echo.
echo Step 3: Building ParkingIN project...
dotnet build ParkingIN.csproj -c Release
if %ERRORLEVEL% NEQ 0 (
    echo Error: Build failed
    goto :error
)

echo.
echo Step 4: Copying config files...
echo Creating new logs directory...
if not exist bin\Release\net6.0-windows\logs mkdir bin\Release\net6.0-windows\logs

echo.
echo Step 5: Running ParkingIN application...
echo Please test the following:
echo - Login with username "admin" and password "admin"
echo.
echo Navigate to bin\Release\net6.0-windows folder...
cd bin\Release\net6.0-windows
if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to change directory to bin\Release\net6.0-windows
    goto :error
)

echo Starting ParkingIN.exe...
start ParkingIN.exe
if %ERRORLEVEL% NEQ 0 (
    echo Error: Failed to start ParkingIN.exe
    goto :error
)

echo.
echo Application started. 
echo.
echo If the login still fails, check the logs folder for database.log file
echo to see detailed error messages about the database connection.
goto :end

:error
echo.
echo ===================================================
echo An error occurred. Please check the messages above.
echo ===================================================

:end
pause 