@echo off
if exist "%~dp0Captovix.Recovery.exe" (
    start "" /d "%TEMP%" "%~dp0Captovix.Recovery.exe"
    exit /b
)
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Recovery.ps1"
