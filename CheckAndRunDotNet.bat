@echo off
echo Checking .NET installation...
echo.

REM Check if dotnet is available
where dotnet >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [ERROR] .NET SDK is not installed or not in PATH
    echo.
    echo Please install .NET 8.0 SDK from:
    echo https://dotnet.microsoft.com/download/dotnet/8.0
    echo.
    echo After installation, restart your terminal and try again.
    goto :end
)

echo [SUCCESS] .NET SDK found!
echo.

echo Checking .NET version...
dotnet --version
echo.

echo Checking installed workloads...
dotnet workload list
echo.

echo Checking if MAUI workload is installed...
dotnet workload list | findstr "maui" >nul 2>&1
if %ERRORLEVEL% NEQ 0 (
    echo [WARNING] MAUI workload not found
    echo Installing MAUI workload...
    dotnet workload install maui
    if %ERRORLEVEL% EQU 0 (
        echo [SUCCESS] MAUI workload installed!
    ) else (
        echo [ERROR] Failed to install MAUI workload
    )
) else (
    echo [SUCCESS] MAUI workload is installed!
)

echo.
echo Now trying to run the application...
cd "c:\Users\Yixin Zhang\Desktop\knew_fate"
dotnet run -f net8.0-windows10.0.19041.0

:end
pause
