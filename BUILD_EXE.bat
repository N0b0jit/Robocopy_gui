@echo off
echo ========================================
echo Nexus Copy Build Script
echo ========================================
echo.
echo This script will build the standalone executable
echo that includes all dependencies.
echo.
echo Building standalone executable...
echo.

dotnet publish src/NexusCopy.App/NexusCopy.App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o "NexusCopy-Standalone"

if errorlevel 1 (
    echo.
    echo BUILD FAILED!
    echo Check the error messages above.
    pause
    exit /b 1
)

echo.
echo ========================================
echo BUILD SUCCESSFUL!
echo ========================================
echo.
echo Your standalone executable is ready at:
echo %cd%\NexusCopy-Standalone\NexusCopy.exe
echo.
echo File size:
dir "NexusCopy-Standalone\NexusCopy.exe" | find "NexusCopy.exe"
echo.
echo You can now run NexusCopy.exe - no installation required!
echo.
echo For troubleshooting, run:
echo - NexusCopy-Standalone\Diagnose.bat
echo - NexusCopy-Standalone\Run as Admin.bat
echo.
pause
