# 🎮 Dungeon Game - Advanced Build & Deploy Script
# Developer: GALIH RIDHO UTOMO
# Repository: https://github.com/galihru/dungeon

param(
    [string]$Version = "",
    [switch]$SkipBuild,
    [switch]$CreateRelease,
    [switch]$UploadToGitHub,
    [string]$GitHubToken = "",
    [switch]$CleanOnly,
    [switch]$Help
)

# Configuration
$ProjectName = "DungeonGame"
$Developer = "GALIH RIDHO UTOMO"
$Repository = "galihru/dungeon"
$ProjectFile = "$ProjectName.csproj"

function Show-Help {
    Write-Host ""
    Write-Host "🎮 Dungeon Game - Advanced Build & Deploy Script" -ForegroundColor Cyan
    Write-Host "Developer: $Developer" -ForegroundColor Green
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "  .\build-deploy.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "  -Version <string>      Specify version (e.g., '1.2.0')"
    Write-Host "  -SkipBuild            Skip build process, use existing publish folder"
    Write-Host "  -CreateRelease        Create GitHub release after build"
    Write-Host "  -UploadToGitHub       Upload artifacts to GitHub release"
    Write-Host "  -GitHubToken <string> GitHub personal access token for API"
    Write-Host "  -CleanOnly            Only clean build artifacts and exit"
    Write-Host "  -Help                 Show this help message"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "  .\build-deploy.ps1 -Version '1.0.0'"
    Write-Host "  .\build-deploy.ps1 -Version '1.1.0' -CreateRelease -UploadToGitHub"
    Write-Host "  .\build-deploy.ps1 -CleanOnly"
    Write-Host ""
    exit 0
}

function Write-Banner {
    param([string]$Message)
    Write-Host ""
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host "🎮 $Message" -ForegroundColor Yellow
    Write-Host "=" * 60 -ForegroundColor Cyan
    Write-Host ""
}

function Write-Step {
    param([string]$Message)
    Write-Host "🔸 $Message" -ForegroundColor Green
}

function Write-Error-Step {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor Red
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor Green
}

function Test-Prerequisites {
    Write-Step "Checking prerequisites..."
    
    # Check .NET 9
    try {
        $dotnetVersion = dotnet --version
        if ($dotnetVersion -notmatch "^9\.") {
            Write-Error-Step ".NET 9 SDK required, found: $dotnetVersion"
            return $false
        }
        Write-Success ".NET 9 SDK found: $dotnetVersion"
    }
    catch {
        Write-Error-Step ".NET 9 SDK not found. Please install .NET 9 SDK."
        return $false
    }
    
    # Check Git
    try {
        git --version | Out-Null
        Write-Success "Git found"
    }
    catch {
        Write-Error-Step "Git not found. Please install Git."
        return $false
    }
    
    # Check PowerShell version
    if ($PSVersionTable.PSVersion.Major -lt 5) {
        Write-Error-Step "PowerShell 5.0+ required"
        return $false
    }
    Write-Success "PowerShell $($PSVersionTable.PSVersion) found"
    
    return $true
}

function Get-ProjectVersion {
    if ($Version) {
        return $Version
    }
    
    # Try to get from git tags
    try {
        $gitTag = git describe --tags --abbrev=0 2>$null
        if ($gitTag) {
            $suggestedVersion = $gitTag -replace '^v', ''
            $newVersion = Read-Host "Enter version (suggested: $suggestedVersion)"
            if (-not $newVersion) { return $suggestedVersion }
            return $newVersion
        }
    }
    catch {}
    
    # Fallback to manual input
    do {
        $inputVersion = Read-Host "Enter version (e.g., 1.0.0)"
    } while (-not $inputVersion -or $inputVersion -notmatch '^\d+\.\d+\.\d+')
    
    return $inputVersion
}

function Update-ProjectVersion {
    param([string]$NewVersion)
    
    Write-Step "Updating project version to $NewVersion..."
    
    $content = Get-Content $ProjectFile -Raw
    $content = $content -replace '<AssemblyVersion>.*?</AssemblyVersion>', "<AssemblyVersion>$NewVersion.0</AssemblyVersion>"
    $content = $content -replace '<FileVersion>.*?</FileVersion>', "<FileVersion>$NewVersion.0</FileVersion>"
    $content = $content -replace '<ProductVersion>.*?</ProductVersion>', "<ProductVersion>$NewVersion</ProductVersion>"
    
    $content | Set-Content $ProjectFile -NoNewline
    Write-Success "Project version updated to $NewVersion"
}

function Invoke-Clean {
    Write-Step "Cleaning previous builds..."
    
    $foldersToClean = @("bin", "obj", "publish", "Installer\bin", "Installer\obj")
    foreach ($folder in $foldersToClean) {
        if (Test-Path $folder) {
            Remove-Item $folder -Recurse -Force
            Write-Host "  • Removed $folder" -ForegroundColor DarkGray
        }
    }
    
    # Clean generated files
    $filesToClean = @("*.zip", "*.msi")
    foreach ($pattern in $filesToClean) {
        Get-ChildItem $pattern -ErrorAction SilentlyContinue | Remove-Item -Force
    }
    
    Write-Success "Cleanup completed"
}

function Invoke-Build {
    param([string]$Version)
    
    Write-Step "Building application..."
    
    # Build
    $buildResult = dotnet build $ProjectFile --configuration Release --verbosity minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Step "Build failed"
        return $false
    }
    
    # Publish
    Write-Step "Publishing self-contained application..."
    $publishResult = dotnet publish $ProjectFile `
        --configuration Release `
        --runtime win-x64 `
        --self-contained true `
        --output publish `
        --verbosity minimal `
        -p:PublishSingleFile=false `
        -p:IncludeNativeLibrariesForSelfExtract=true
        
    if ($LASTEXITCODE -ne 0) {
        Write-Error-Step "Publish failed"
        return $false
    }
    
    # Copy additional files
    Write-Step "Copying additional files..."
    if (Test-Path "README.md") { Copy-Item "README.md" "publish\" }
    if (Test-Path "LICENSE") { Copy-Item "LICENSE" "publish\" }
    
    # Create version info
    $versionInfo = @{
        version = $Version
        buildDate = (Get-Date).ToString("yyyy-MM-dd HH:mm:ss")
        developer = $Developer
        repository = "https://github.com/$Repository"
        commitHash = $(git rev-parse HEAD 2>$null)
        description = "Strategic AI-powered dungeon exploration game"
        features = @(
            "Q-Learning AI system",
            "Multiple difficulty levels",
            "Turn-based strategic combat", 
            "Auto-update functionality",
            "Modern WPF interface"
        )
    }
    
    $versionInfo | ConvertTo-Json -Depth 3 | Out-File "publish\version.json" -Encoding UTF8
    
    Write-Success "Build completed successfully"
    return $true
}

function New-Packages {
    param([string]$Version)
    
    Write-Step "Creating distribution packages..."
    
    # Create ZIP
    $zipName = "$ProjectName-v$Version-Portable.zip"
    if (Test-Path $zipName) { Remove-Item $zipName -Force }
    
    Compress-Archive -Path "publish\*" -DestinationPath $zipName
    Write-Success "Created portable package: $zipName"
    
    # TODO: Create MSI installer if WiX is available
    try {
        candle.exe 2>$null | Out-Null
        Write-Step "WiX found, building MSI installer..."
        # Add WiX build logic here
        Write-Success "MSI installer created (placeholder)"
    }
    catch {
        Write-Host "  ⚠️ WiX not found, skipping MSI creation" -ForegroundColor Yellow
    }
    
    return @($zipName)
}

function New-GitHubRelease {
    param(
        [string]$Version,
        [string[]]$Assets,
        [string]$Token
    )
    
    if (-not $Token) {
        Write-Host "⚠️ No GitHub token provided, skipping release creation" -ForegroundColor Yellow
        return
    }
    
    Write-Step "Creating GitHub release..."
    
    # Create release notes
    $releaseNotes = @"
# 🎮 Dungeon Game v$Version

## 🆕 What's New
- Enhanced AI gameplay with Q-Learning algorithms
- Multiple difficulty levels (Beginner → Master)
- Turn-based strategic combat system
- Modern WPF interface with dark theme
- Auto-update system with GitHub integration

## 🎯 Features
- **Smart AI Opponent**: Uses Q-Learning for adaptive gameplay
- **Difficulty Scaling**: From beginner-friendly to master level challenge
- **Visual Polish**: Modern dark theme with smooth animations
- **Auto-Updates**: Automatic notification and installation of new versions
- **Game Rules**: Built-in tutorial and help system

## 📦 Installation
1. Download the ZIP file below
2. Extract to your preferred location
3. Run DungeonGame.exe
4. Enjoy strategic dungeon battles!

## 🔧 System Requirements
- Windows 10/11 (x64)
- .NET 9 Runtime (included)
- 50MB free disk space

## 👨‍💻 Developer
**$Developer**
- Repository: https://github.com/$Repository
- Built with: .NET 9, WPF, Q-Learning AI

---
*Built on $(Get-Date -Format "MMMM dd, yyyy") | Auto-update enabled*
"@
    
    # Use GitHub CLI if available
    try {
        gh --version | Out-Null
        Write-Step "Using GitHub CLI to create release..."
        
        $assetsParam = $Assets | ForEach-Object { "--attach `"$_`"" } | Join-String " "
        $cmd = "gh release create v$Version --title `"🎮 Dungeon Game v$Version`" --notes `"$releaseNotes`" $assetsParam"
        
        Invoke-Expression $cmd
        Write-Success "GitHub release created successfully"
    }
    catch {
        Write-Host "  ⚠️ GitHub CLI not available. Please create release manually:" -ForegroundColor Yellow
        Write-Host "    https://github.com/$Repository/releases/new" -ForegroundColor Gray
        Write-Host "    Tag: v$Version" -ForegroundColor Gray
    }
}

function Show-Summary {
    param(
        [string]$Version,
        [string[]]$Assets,
        [timespan]$BuildTime
    )
    
    Write-Banner "BUILD COMPLETED SUCCESSFULLY!"
    
    Write-Host "📊 Build Information:" -ForegroundColor Yellow
    Write-Host "   • Version: $Version" -ForegroundColor White
    Write-Host "   • Target: Windows x64" -ForegroundColor White
    Write-Host "   • Runtime: Self-contained .NET 9" -ForegroundColor White
    Write-Host "   • Developer: $Developer" -ForegroundColor White
    Write-Host "   • Build Time: $($BuildTime.ToString('mm\:ss'))" -ForegroundColor White
    Write-Host ""
    
    Write-Host "📦 Generated Files:" -ForegroundColor Yellow
    foreach ($asset in $Assets) {
        Write-Host "   • $asset" -ForegroundColor White
    }
    Write-Host ""
    
    Write-Host "🚀 Next Steps:" -ForegroundColor Yellow
    Write-Host "   • Test the application thoroughly" -ForegroundColor White
    Write-Host "   • Upload to GitHub Releases" -ForegroundColor White
    Write-Host "   • Auto-update system will detect new version" -ForegroundColor White
    Write-Host "   • Users will be notified automatically" -ForegroundColor White
    Write-Host ""
}

# Main execution
if ($Help) { Show-Help }

$startTime = Get-Date
Write-Banner "DUNGEON GAME BUILD & DEPLOY"
Write-Host "Developer: $Developer" -ForegroundColor Green
Write-Host "Repository: https://github.com/$Repository" -ForegroundColor Green

if (-not (Test-Prerequisites)) {
    Write-Error-Step "Prerequisites check failed"
    exit 1
}

if ($CleanOnly) {
    Invoke-Clean
    Write-Success "Clean operation completed"
    exit 0
}

$buildVersion = Get-ProjectVersion

Write-Host ""
Write-Host "🎯 Building Dungeon Game v$buildVersion" -ForegroundColor Cyan

Invoke-Clean
Update-ProjectVersion -NewVersion $buildVersion

if (-not $SkipBuild) {
    if (-not (Invoke-Build -Version $buildVersion)) {
        Write-Error-Step "Build process failed"
        exit 1
    }
}

$assets = New-Packages -Version $buildVersion

if ($CreateRelease -or $UploadToGitHub) {
    New-GitHubRelease -Version $buildVersion -Assets $assets -Token $GitHubToken
}

$buildTime = (Get-Date) - $startTime
Show-Summary -Version $buildVersion -Assets $assets -BuildTime $buildTime

# Open output folder
if ($assets.Count -gt 0 -and (Test-Path $assets[0])) {
    Write-Host "📂 Opening output folder..." -ForegroundColor Cyan
    explorer /select,"$($assets[0])"
}
