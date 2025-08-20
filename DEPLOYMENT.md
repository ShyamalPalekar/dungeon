# 🎮 Dungeon Game - Deployment Guide

> **Developer:** GALIH RIDHO UTOMO  
> **Repository:** <https://github.com/galihru/dungeon>  
> **License:** MIT  

## 📋 Table of Contents

- [🎯 Overview](#-overview)
- [🔧 Prerequisites](#-prerequisites)
- [🚀 Quick Deployment](#-quick-deployment)
- [🏗️ Manual Build Process](#️-manual-build-process)
- [📦 Distribution Methods](#-distribution-methods)
- [🔄 Auto-Update System](#-auto-update-system)
- [🌐 CI/CD Pipeline](#-cicd-pipeline)
- [🔒 Code Signing](#-code-signing)
- [🐛 Troubleshooting](#-troubleshooting)

## 🎯 Overview

This document provides comprehensive instructions for building, packaging, and deploying the Dungeon Game application. The game features an AI-powered strategic combat system with automatic updates and professional distribution methods.

### ⚡ Key Features

- **Q-Learning AI System** - Adaptive opponent with multiple difficulty levels
- **Auto-Update System** - GitHub integration with automatic version detection
- **Professional Installer** - MSI package with proper registration
- **Cross-Platform Build** - CI/CD pipeline for automated releases
- **Modern UI** - WPF with custom theming and animations

## 🔧 Prerequisites

### Required Software

- **[.NET 9 SDK](https://dotnet.microsoft.com/download)** - Latest version
- **[Git](https://git-scm.com/)** - For version control
- **[PowerShell 5.0+](https://docs.microsoft.com/powershell/)** - For advanced scripts

### Optional Tools

- **[WiX Toolset v3.11+](https://github.com/wixtoolset/wix3/releases)** - For MSI installer creation
- **[GitHub CLI](https://cli.github.com/)** - For automated releases
- **[Visual Studio 2022](https://visualstudio.microsoft.com/)** - For development

### System Requirements

- **OS:** Windows 10/11 (x64)
- **RAM:** 4GB minimum, 8GB recommended
- **Storage:** 500MB free space for build tools
- **Network:** Internet connection for NuGet packages and GitHub API

## 🚀 Quick Deployment

### Method 1: Simple Batch Script

```batch
# Run the automated build script
.\build-installer.bat

# Follow the prompts to:
# 1. Enter version number (e.g., 1.0.0)
# 2. Wait for build completion
# 3. Find generated files in output folder
```

### Method 2: Advanced PowerShell Script

```powershell
# Basic build
.\build-deploy.ps1 -Version "1.0.0"

# Build and create GitHub release
.\build-deploy.ps1 -Version "1.0.0" -CreateRelease -GitHubToken "your_token"

# Clean only
.\build-deploy.ps1 -CleanOnly

# Show all options
.\build-deploy.ps1 -Help
```

## 🏗️ Manual Build Process

### 1. Clean Previous Builds

```batch
rmdir /s /q bin obj publish 2>nul
del *.zip *.msi 2>nul
```

### 2. Update Version Information

Edit `DungeonGame.csproj` and update:

```xml
<AssemblyVersion>1.0.0.0</AssemblyVersion>
<FileVersion>1.0.0.0</FileVersion>
<ProductVersion>1.0.0</ProductVersion>
```

### 3. Build Application

```batch
dotnet restore
dotnet build --configuration Release
```

### 4. Publish Self-Contained

```batch
dotnet publish DungeonGame.csproj ^
    --configuration Release ^
    --runtime win-x64 ^
    --self-contained true ^
    --output publish
```

### 5. Create Distribution Packages

#### ZIP Package (Portable)

```powershell
Compress-Archive -Path "publish\*" -DestinationPath "DungeonGame-v1.0.0-Portable.zip"
```

#### MSI Installer (Requires WiX)

```batch
# Compile WiX source
candle.exe -out "Installer\obj\DungeonGame.wixobj" "Installer\DungeonGame.wxs"

# Create MSI
light.exe -ext WixUIExtension -out "DungeonGame-v1.0.0-Setup.msi" "Installer\obj\DungeonGame.wixobj"
```

## 📦 Distribution Methods

### GitHub Releases (Recommended)

1. **Create Release Tag**

   ```bash
   git tag -a v1.0.0 -m "Release version 1.0.0"
   git push origin v1.0.0
   ```

2. **Upload Assets**
   - Go to <https://github.com/galihru/dungeon/releases/new>
   - Select tag: `v1.0.0`
   - Upload ZIP and MSI files
   - Write release notes

3. **Automatic Distribution**
   - Auto-update system detects new releases
   - Users get notified automatically
   - One-click download and install

### Direct Distribution

- **Corporate:** Deploy MSI via Group Policy
- **Personal:** Share ZIP file directly
- **Web:** Host on download server

## 🔄 Auto-Update System

### How It Works

1. **Version Check:** App checks GitHub API for latest release
2. **User Notification:** Update dialog shows new version details
3. **Download:** Secure download from GitHub releases
4. **Installation:** Automatic installer launch with admin privileges
5. **App Restart:** Seamless transition to new version

### Configuration

Update behavior is controlled in `Services/UpdateService.cs`:

```csharp
private readonly string _repositoryOwner = "galihru";
private readonly string _repositoryName = "dungeon";
```

### User Settings

- **Auto-update enabled:** `Properties.Settings.Default.AutoUpdateEnabled`
- **Check interval:** `Properties.Settings.Default.UpdateCheckInterval` (hours)
- **Skipped versions:** `Properties.Settings.Default.SkippedVersion`

## 🌐 CI/CD Pipeline

### GitHub Actions Workflow

File: `.github/workflows/build-release.yml`

**Triggers:**

- Push to `main` branch (pre-release)
- Git tags `v*` (stable release)
- Manual dispatch with version input

**Process:**

1. **Setup Environment** - .NET 9, restore packages
2. **Version Management** - Auto-increment or manual
3. **Build & Test** - Full compilation and test suite
4. **Package Creation** - ZIP and MSI generation
5. **Release Creation** - GitHub release with assets
6. **Notification** - Auto-update system activation

### Manual Release

```bash
# Create and push release tag
git tag -a v1.0.0 -m "Release v1.0.0"
git push origin v1.0.0

# GitHub Actions will automatically:
# - Build the application
# - Create MSI and ZIP packages
# - Create GitHub release
# - Upload all assets
```

## 🔒 Code Signing

### Why Sign Code?

- **Security:** Prevents tampering warnings
- **Trust:** Users trust signed applications
- **Distribution:** Required for some platforms

### Signing Process (Optional)

1. **Obtain Certificate**
   - Code signing certificate from CA
   - Azure Key Vault integration
   - Self-signed for testing

2. **Sign Executables**

   ```batch
   signtool.exe sign /f "certificate.pfx" /p "password" /t "http://timestamp.digicert.com" "DungeonGame.exe"
   ```

3. **Verify Signature**

   ```batch
   signtool.exe verify /pa "DungeonGame.exe"
   ```

## 🐛 Troubleshooting

### Common Build Issues

#### .NET Version Mismatch

**Error:** Project targets .NET 9 but .NET 8 found  
**Solution:** Install .NET 9 SDK from official Microsoft site

#### Missing Dependencies

**Error:** Package restore failed  
**Solution:**

```batch
dotnet nuget locals all --clear
dotnet restore --force
```

#### WiX Compilation Errors

**Error:** WiX toolset not found  
**Solution:**

1. Download WiX v3.11+ from GitHub releases
2. Install with PATH environment variable
3. Restart command prompt

### Runtime Issues

#### App Won't Start

**Symptoms:** No window appears, process exits immediately  
**Diagnosis:**

```batch
# Run from command line to see errors
DungeonGame.exe --debug
```

**Common Fixes:**

- Install .NET 9 Runtime on target machine
- Check for missing assets in installation folder
- Run as administrator if registry issues occur

#### Update System Fails

**Symptoms:** "Update check failed" or connection errors  
**Solutions:**

- Check internet connection
- Verify GitHub repository is public
- Update GitHub API rate limits

#### Theme/Resource Errors

**Error:** Cannot locate resource 'ModernStyles.xaml'  
**Solution:** Ensure all XAML files are included in build:

```xml
<ItemGroup>
  <Resource Include="Theme\ModernStyles.xaml" />
  <Resource Include="Theme\Styles.xaml" />
</ItemGroup>
```

### Performance Issues

#### Slow AI Response

**Symptoms:** Long delays during AI turns  
**Solutions:**

- Reduce Q-Learning exploration rate
- Limit decision tree depth
- Optimize game state evaluation

#### Memory Leaks

**Symptoms:** Increasing memory usage over time  
**Diagnosis:** Use Visual Studio Diagnostic Tools  
**Solutions:**

- Dispose MediaPlayer instances properly
- Unsubscribe from events in window close handlers

## 🎯 Release Checklist

### Pre-Release Testing

- [ ] Build completes without warnings
- [ ] All game features working (AI, difficulty levels, etc.)
- [ ] Auto-update system functional
- [ ] Theme system loading correctly
- [ ] Audio system working on different Windows versions
- [ ] Installer creates proper shortcuts and registry entries
- [ ] Uninstaller removes all components cleanly

### Release Process

- [ ] Version number updated in all files
- [ ] Release notes written and reviewed
- [ ] All assets included in distribution packages
- [ ] Code signed (if applicable)
- [ ] GitHub release created with proper tags
- [ ] Auto-update system tested with new version

### Post-Release

- [ ] Monitor user feedback and bug reports
- [ ] Verify auto-update notifications working
- [ ] Check download statistics and adoption rates
- [ ] Plan next version based on user feedback

---

## 🆘 Support

For build issues, deployment problems, or questions:

- **GitHub Issues:** <https://github.com/galihru/dungeon/issues>
- **Developer:** GALIH RIDHO UTOMO
- **Repository:** <https://github.com/galihru/dungeon>

---

*This deployment guide covers the complete process from source code to end-user installation. The auto-update system ensures users always have the latest features and security updates.*
