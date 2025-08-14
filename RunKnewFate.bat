@echo off
echo Starting KnewFate Application...
cd "c:\Users\Yixin Zhang\Desktop\knew_fate\bin\Debug\net8.0-windows10.0.19041.0\win10-x64"
echo Current directory: %CD%
echo.
echo Checking if exe exists:
if exist KnewFate.exe (
    echo KnewFate.exe found!
    echo.
    echo Running application...
    KnewFate.exe
    echo.
    echo Application exited with code: %ERRORLEVEL%
) else (
    echo KnewFate.exe not found!
)
echo.
pause
