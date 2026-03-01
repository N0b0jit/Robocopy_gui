@echo off
echo ========================================
echo Nexus Copy Diagnostic Tool
echo ========================================
echo.
echo Checking system information...
echo.

echo [1] Checking Windows Version:
ver
echo.

echo [2] Checking .NET Runtimes:
echo Installed .NET runtimes:
dotnet --list-runtimes 2>nul
if errorlevel 1 (
    echo .NET CLI not found. This is OK for self-contained version.
) else (
    echo .NET CLI found.
)
echo.

echo [3] Checking if NexusCopy.exe exists:
if exist "NexusCopy.exe" (
    echo NexusCopy.exe found - Size:
    dir "NexusCopy.exe" | find "NexusCopy.exe"
) else (
    echo ERROR: NexusCopy.exe not found!
)
echo.

echo [4] Checking available memory:
wmic computersystem get TotalPhysicalMemory /value | find "="
echo.

echo [5] Checking disk space:
wmic logicaldisk get size,freespace,caption
echo.

echo [6] Attempting to run NexusCopy...
echo.
echo If the application doesn't start, try these solutions:
echo 1. Run as Administrator
echo 2. Check Windows Event Viewer for errors
echo 3. Temporarily disable antivirus software
echo 4. Try running in compatibility mode
echo.
echo Press any key to try launching NexusCopy...
pause > nul

echo Launching NexusCopy...
start "" "NexusCopy.exe"

echo.
echo If NexusCopy still doesn't start, check:
echo 1. Windows Event Viewer - Windows Logs - Application
echo 2. Task Manager for background processes
echo 3. Antivirus quarantine logs
echo.
pause
