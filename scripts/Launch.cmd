@echo off
cd /d "%~dp0"
if not exist "Captovix.exe" (
 echo Extract the full portable ZIP before running.
 pause
 exit /b 1
)
reg query "HKLM\SOFTWARE\Microsoft\VisualStudio\14.0\VC\Runtimes\x64" /v Installed >nul 2>&1
if errorlevel 1 (
 echo Visual C++ Runtime may be missing. Run Install.cmd or Setup.exe first.
 pause
)
start "" "%~dp0Captovix.exe"
