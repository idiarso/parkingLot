@echo off
echo ===== Hardware Testing Utility =====
echo.

:: Check for compiled DLL first
if exist "D:\21maret\Hardware\bin\Debug\net6.0\HardwareTester.dll" (
    echo Using existing compiled version...
    cd /d "D:\21maret\Hardware\bin\Debug\net6.0\"
    dotnet HardwareTester.dll
    pause
    exit /b
)

:: If not compiled, create a temporary project and compile
echo Compiled version not found. Creating temporary project...
cd /d "D:\21maret\"

:: Create temporary directory for project
if not exist "TempHardwareTest" mkdir TempHardwareTest
cd TempHardwareTest

:: Create a new console application
echo Creating new console application...
dotnet new console

:: Copy hardware test files
echo Copying hardware test files...
copy /Y "..\Hardware\HardwareTester.cs" "Program.cs" >nul
if not exist "config" mkdir config
copy /Y "..\clean_code\ParkingIN\config\*.*" "config\" >nul
if not exist "test_images" mkdir test_images

:: Create csproj file with required references
echo Creating project file with required references...
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

:: Build the application
echo Building hardware tester...
dotnet build -c Debug

:: Run the application
echo.
echo Running hardware tester...
echo.
dotnet run

:: Clean up
echo.
echo Press any key to exit...
pause > nul
cd ..
echo Cleaning up...
:: Uncomment this line if you want to remove the temporary directory after testing
:: rmdir /s /q TempHardwareTest
