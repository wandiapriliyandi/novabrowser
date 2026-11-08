@echo off
setlocal
cd /d "%~dp0"

echo ===================================================
echo     Mengompilasi NovaBrowser Application Core
echo ===================================================
echo.

set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
if not exist "%CSC%" set CSC=C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe

if not exist "%CSC%" (
    echo [ERROR] C# Compiler csc.exe tidak ditemukan di sistem Windows ini!
    pause
    exit /b 1
)

echo Menggunakan compiler: %CSC%
echo Mengompilasi kode sumber C# ke NovaBrowser.exe...

"%CSC%" /nologo /target:winexe /optimize+ /platform:anycpu /win32icon:app.ico /out:NovaBrowser.exe ^
    /r:System.dll,System.Drawing.dll,System.Windows.Forms.dll,System.Core.dll ^
    /r:lib\Microsoft.Web.WebView2.WinForms.dll,lib\Microsoft.Web.WebView2.Core.dll ^
    src\Program.cs ^
    src\MainForm.cs ^
    src\BrowserTab.cs ^
    src\DevToolbar.cs ^
    src\PentestToolbar.cs ^
    src\SecurityToolsForm.cs ^
    src\ProfileManager.cs ^
    src\OmniboxHelper.cs ^
    src\Homepage.cs ^
    src\BookmarkHistoryManager.cs ^
    src\HistoryForm.cs ^
    src\InstallAppDialog.cs ^
    src\UpdateManager.cs ^
    src\UpdateDialog.cs

if %ERRORLEVEL% equ 0 (
    echo.
    echo ===================================================
    echo [SUKSES] NovaBrowser.exe berhasil dibuat!
    echo ===================================================
) else (
    echo.
    echo [GAGAL] Kompilasi mengalami error.
    exit /b %ERRORLEVEL%
)

endlocal
