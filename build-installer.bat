@echo off
setlocal enabledelayedexpansion

echo.
echo ========================================
echo 🎮 DUNGEON GAME INSTALLER BUILDER
echo ========================================
echo Developer: GALIH RIDHO UTOMO
echo Repository: https://github.com/galihru/dungeon
echo ========================================
echo.

REM Check prerequisites
echo 🔍 Checking prerequisites...

REM Check .NET 9
dotnet --version >nul 2>&1
if errorlevel 1 (
    echo ❌ .NET 9 SDK not found. Please install .NET 9 SDK first.
    pause
    exit /b 1
)

REM Check WiX Toolset
heat.exe >nul 2>&1
if errorlevel 1 (
    echo ❌ WiX Toolset not found. Please install WiX Toolset v3.11+ first.
    echo Download from: https://github.com/wixtoolset/wix3/releases
    pause
    exit /b 1
)

echo ✅ Prerequisites check passed!
echo.

REM Get version
set /p VERSION="Enter version (e.g., 1.0.0): "
if "%VERSION%"=="" set VERSION=1.0.0

echo 🎯 Building Dungeon Game v%VERSION%
echo.

REM Clean previous builds
echo 🧹 Cleaning previous builds...
if exist "bin\" rd /s /q "bin\"
if exist "obj\" rd /s /q "obj\"
if exist "publish\" rd /s /q "publish\"
if exist "Installer\bin\" rd /s /q "Installer\bin\"
if exist "Installer\obj\" rd /s /q "Installer\obj\"

REM Create directories
mkdir "publish" 2>nul
mkdir "Installer\bin" 2>nul
mkdir "Installer\obj" 2>nul

echo ✅ Cleanup completed!
echo.

REM Update project version
echo 🔄 Updating version information...
powershell -Command "
    $csprojPath = 'DungeonGame.csproj'
    $content = Get-Content $csprojPath -Raw
    $content = $content -replace '<AssemblyVersion>.*?</AssemblyVersion>', '<AssemblyVersion>%VERSION%.0</AssemblyVersion>'
    $content = $content -replace '<FileVersion>.*?</FileVersion>', '<FileVersion>%VERSION%.0</FileVersion>'
    $content = $content -replace '<ProductVersion>.*?</ProductVersion>', '<ProductVersion>%VERSION%</ProductVersion>'
    $content | Set-Content $csprojPath -NoNewline
    Write-Host '✅ Version updated to %VERSION%'
"

REM Build application
echo.
echo 🏗️ Building application...
dotnet build DungeonGame.csproj --configuration Release --verbosity minimal
if errorlevel 1 (
    echo ❌ Build failed!
    pause
    exit /b 1
)

REM Publish self-contained
echo.
echo 📦 Publishing self-contained application...
dotnet publish DungeonGame.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output publish ^
    --verbosity minimal ^
    -p:PublishSingleFile=false ^
    -p:IncludeNativeLibrariesForSelfExtract=true

if errorlevel 1 (
    echo ❌ Publish failed!
    pause
    exit /b 1
)

REM Copy additional files
echo.
echo 📋 Copying additional files...
if exist "README.md" copy "README.md" "publish\" >nul
if exist "LICENSE" copy "LICENSE" "publish\" >nul

REM Create version info
echo 📝 Creating version information...
powershell -Command "
    @{
        version = '%VERSION%'
        buildDate = (Get-Date).ToString('yyyy-MM-dd HH:mm:ss')
        developer = 'GALIH RIDHO UTOMO'
        repository = 'https://github.com/galihru/dungeon'
        description = 'Strategic AI-powered dungeon exploration game'
    } | ConvertTo-Json | Out-File 'publish\version.json' -Encoding UTF8
    Write-Host '✅ Version info created'
"

REM Build installer using WiX
echo.
echo 🔥 Building Windows Installer (MSI)...

REM Update WiX file with current version
powershell -Command "
    $wxsPath = 'Installer\DungeonGame.wxs'
    $content = Get-Content $wxsPath -Raw
    $content = $content -replace '<?define ProductVersion = \".*?\" ?>', '<?define ProductVersion = \"%VERSION%\" ?>'
    $content | Set-Content $wxsPath -NoNewline
    Write-Host '✅ WiX file updated with version %VERSION%'
"

REM Compile WiX
echo 🔧 Compiling installer...
candle.exe -nologo -out "Installer\obj\DungeonGame.wixobj" "Installer\DungeonGame.wxs"
if errorlevel 1 (
    echo ❌ WiX compilation failed!
    pause
    exit /b 1
)

REM Link installer
echo 🔗 Linking installer...
light.exe -nologo -ext WixUIExtension -out "Installer\bin\DungeonGame-v%VERSION%-Setup.msi" "Installer\obj\DungeonGame.wixobj"
if errorlevel 1 (
    echo ❌ WiX linking failed!
    pause
    exit /b 1
)

REM Create ZIP package
echo.
echo 📦 Creating portable ZIP package...
powershell -Command "
    $archiveName = 'DungeonGame-v%VERSION%-Portable.zip'
    Compress-Archive -Path 'publish\*' -DestinationPath $archiveName -Force
    Write-Host '✅ Created portable package: '$archiveName
"

REM Summary
echo.
echo ========================================
echo 🎉 BUILD COMPLETED SUCCESSFULLY!
echo ========================================
echo.
echo 📦 Generated Files:
echo    • DungeonGame-v%VERSION%-Setup.msi (Windows Installer)
echo    • DungeonGame-v%VERSION%-Portable.zip (Portable Package)
echo.
echo 📊 Build Information:
echo    • Version: %VERSION%
echo    • Target: Windows x64
echo    • Runtime: Self-contained .NET 9
echo    • Developer: GALIH RIDHO UTOMO
echo.
echo 🚀 Ready for distribution!
echo    • Upload to GitHub Releases
echo    • Auto-update system will detect new version
echo    • Users will be notified automatically
echo.

REM Open output folder
if exist "Installer\bin\DungeonGame-v%VERSION%-Setup.msi" (
    echo 📂 Opening output folder...
    explorer /select,"Installer\bin\DungeonGame-v%VERSION%-Setup.msi"
)

pause
