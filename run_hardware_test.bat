@echo off
echo ===== Hardware Test Utility =====
echo.

:: Move to hardware directory
cd /d "D:\21maret\TempHardwareTest"

:: Remove any existing files to start clean
rd /s /q "TempHardwareTest" 2>nul
mkdir "TempHardwareTest"
cd TempHardwareTest

:: Create a new console application
echo Creating console application...
dotnet new console --force

:: Create csproj file
echo ^<Project Sdk="Microsoft.NET.Sdk"^> > HardwareTester.csproj
echo   ^<PropertyGroup^> >> HardwareTester.csproj
echo     ^<OutputType^>Exe^</OutputType^> >> HardwareTester.csproj
echo     ^<TargetFramework^>net6.0^</TargetFramework^> >> HardwareTester.csproj
echo   ^</PropertyGroup^> >> HardwareTester.csproj
echo   ^<ItemGroup^> >> HardwareTester.csproj
echo     ^<PackageReference Include="AForge.Video" Version="2.2.5" /^> >> HardwareTester.csproj
echo     ^<PackageReference Include="AForge.Video.DirectShow" Version="2.2.5" /^> >> HardwareTester.csproj
echo     ^<PackageReference Include="System.IO.Ports" Version="6.0.0" /^> >> HardwareTester.csproj
echo     ^<PackageReference Include="System.Drawing.Common" Version="6.0.0" /^> >> HardwareTester.csproj
echo   ^</ItemGroup^> >> HardwareTester.csproj
echo ^</Project^> >> HardwareTester.csproj

:: Copy hardware tester file
echo Copying hardware test files...
copy /Y "..\..\Hardware\HardwareTester.cs" "Program.cs" >nul

:: Create directories
if not exist "config" mkdir config
if not exist "test_images" mkdir test_images

:: Copy config files if they exist
if exist "..\..\clean_code\ParkingIN\config\*.*" (
    copy /Y "..\..\clean_code\ParkingIN\config\*.*" "config\" >nul
)

:: Build the application
echo Building hardware tester...
dotnet restore
dotnet build

:: Run the application
echo Running hardware tester...
dotnet run

echo.
echo Press any key to exit...
pause > nul
cd ..
