@echo off
cd /d "%~dp0"
if not exist "NovaBrowser.exe" (
    echo [INFO] NovaBrowser.exe belum terkompilasi. Memulai kompilasi otomatis...
    call build.bat
)
start "" "NovaBrowser.exe"

