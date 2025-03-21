@echo off
setlocal enabledelayedexpansion
color 0A
title .NET Desktop Runtime Installer

echo ================================================================
echo        .NET Desktop Runtime Installation Helper
echo ================================================================
echo.
echo This script will help you install the required .NET Desktop Runtime
echo to run the Modern Parking System application.
echo.

:: Check if .NET Desktop Runtime is already installed
dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 8." > nul
if %ERRORLEVEL% EQU 0 (
    echo [INFO] .NET Desktop Runtime 8.0 is already installed.
    echo.
    pause
    exit /b 0
)

echo [WARN] .NET Desktop Runtime 8.0 was not found on your system.
echo.
echo The Modern Parking System requires .NET Desktop Runtime 8.0 or newer.
echo.
echo Options:
echo 1. Download and install automatically (Recommended)
echo 2. Open download page in browser
echo 3. Cancel
echo.

set /p choice="Enter your choice (1-3): "

if "%choice%"=="1" (
    echo.
    echo Downloading .NET Desktop Runtime 8.0.14...
    
    :: Create temp directory if it doesn't exist
    if not exist "temp" mkdir temp
    
    :: Download the .NET Desktop Runtime installer - updated to 8.0.14
    curl -L -o temp\dotnet-runtime-installer.exe https://download.visualstudio.microsoft.com/download/pr/cab8c5b5-9c4c-42de-a1fe-89ab613250d5/4d7ba17bb7e0332c4fe7e3e4e3856745/windowsdesktop-runtime-8.0.14-win-x64.exe
    
    echo.
    if exist "temp\dotnet-runtime-installer.exe" (
        echo Download completed successfully!
        echo.
        echo Installing .NET Desktop Runtime 8.0.14...
        echo This may take a few minutes. Please wait...
        echo.
        
        :: Run the installer
        start /wait temp\dotnet-runtime-installer.exe /quiet /norestart
        
        echo.
        echo Installation completed!
        echo.
        echo Please restart the application now.
    ) else (
        echo Failed to download the installer.
        echo Please try option 2 to download manually.
    )
) else if "%choice%"=="2" (
    echo.
    echo Opening download page in your default browser...
    start https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-desktop-8.0.14-windows-x64-installer
) else (
    echo.
    echo Installation cancelled.
)

echo.
pause 