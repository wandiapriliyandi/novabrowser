@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo     Mengemas dan Mengompilasi NovaBrowser Installer
echo ===================================================

set CSC_PATH=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe

if not exist "%CSC_PATH%" (
    echo [ERROR] Compiler csc.exe tidak ditemukan di %CSC_PATH%
    exit /b 1
)

echo.
echo [1/3] Mengemas file aplikasi ke NovaBrowserPayload.zip...
if exist "installer\NovaBrowserPayload.zip" del "installer\NovaBrowserPayload.zip"

powershell -Command "Compress-Archive -Path 'NovaBrowser.exe', 'NovaBrowser.exe.config', 'app.ico', 'lib' -DestinationPath 'installer\NovaBrowserPayload.zip' -Force"

if not exist "installer\NovaBrowserPayload.zip" (
    echo [ERROR] Gagal membuat file payload zip!
    exit /b 1
)

echo [2/3] Mengompilasi SetupProgram.cs menjadi NovaBrowserSetup.exe...
"%CSC_PATH%" /target:winexe /optimize+ /platform:anycpu /out:NovaBrowserSetup.exe /win32icon:app.ico /resource:installer\NovaBrowserPayload.zip,NovaBrowserPayload.zip /r:System.Windows.Forms.dll,System.Drawing.dll,System.IO.Compression.dll,System.IO.Compression.FileSystem.dll,Microsoft.CSharp.dll installer\SetupProgram.cs

if %ERRORLEVEL% equ 0 (
    echo.
    echo ===================================================
    echo [SUKSES] NovaBrowserSetup.exe berhasil dibuat!
    echo Ukuran file Installer mandiri siap dibagikan ke pengguna.
    echo ===================================================
) else (
    echo [ERROR] Kompilasi NovaBrowserSetup.exe gagal!
    exit /b 1
)

echo.
