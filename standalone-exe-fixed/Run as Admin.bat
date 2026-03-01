@echo off
echo Requesting administrator privileges...
powershell -Command "Start-Process NexusCopy.exe -Verb RunAs"
if errorlevel 1 (
    echo.
    echo Failed to run as administrator.
    echo Please right-click NexusCopy.exe and select "Run as administrator"
    pause
)
