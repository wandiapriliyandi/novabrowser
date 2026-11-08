@echo off
setlocal
cd /d "%~dp0"

echo Membuat shortcut NovaLite Browser di Desktop...

set "TARGET_EXE=%~dp0NovaBrowser.exe"
set "SHORTCUT_PATH=%USERPROFILE%\Desktop\NovaLite Browser.lnk"

powershell -NoProfile -Command ^
    "$ws = New-Object -ComObject WScript.Shell; $s = $ws.CreateShortcut('%SHORTCUT_PATH%'); $s.TargetPath = '%TARGET_EXE%'; $s.WorkingDirectory = '%~dp0'; $s.IconLocation = '%~dp0app.ico,0'; $s.Description = 'NovaLite Browser (Ultra Lightweight & Portable Developer Edition)'; $s.Save()"

if exist "%SHORTCUT_PATH%" (
    echo.
    echo [SUKSES] Shortcut "NovaLite Browser" berhasil dibuat di Desktop Anda!
) else (
    echo [GAGAL] Tidak dapat membuat shortcut di Desktop.
)

pause
endlocal

