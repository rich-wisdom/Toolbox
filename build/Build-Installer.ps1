#Requires -Version 5.1
<#
.SYNOPSIS
    Builds the Windows Toolbox MSIX installer package.

.DESCRIPTION
    This script builds the Windows Toolbox application and creates an MSIX package
    for distribution. It supports Debug and Release configurations and multiple platforms.

.PARAMETER Configuration
    Build configuration (Debug or Release). Default: Release

.PARAMETER Platform
    Target platform (x86, x64, ARM64, or All). Default: x64

.PARAMETER Version
    Version number for the package (e.g., 1.0.0.0). If not specified, reads from project file.

.PARAMETER SkipBuild
    Skip the build step and only package existing binaries.

.PARAMETER OutputPath
    Custom output path for the installer. Default: .\dist\

.EXAMPLE
    .\Build-Installer.ps1
    Builds Release x64 installer

.EXAMPLE
    .\Build-Installer.ps1 -Configuration Debug -Platform x86
    Builds Debug x86 installer

.EXAMPLE
    .\Build-Installer.ps1 -Platform All -Version 1.2.3.0
    Builds Release installer for all platforms with version 1.2.3.0
#>

param(
    [Parameter()]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',
    
    [Parameter()]
    [ValidateSet('x86', 'x64', 'ARM64', 'All')]
    [string]$Platform = 'x64',
    
    [Parameter()]
    [string]$Version,
    
    [Parameter()]
    [switch]$SkipBuild,
    
    [Parameter()]
    [string]$OutputPath = ".\dist"
)

$ErrorActionPreference = 'Stop'

# Script paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$solutionFile = Join-Path $rootDir "WindowsToolbox.sln"
$packageProject = Join-Path $rootDir "src\WindowsToolbox.Package\WindowsToolbox.Package.wapproj"
$manifestFile = Join-Path $rootDir "src\WindowsToolbox.Package\Package.appxmanifest"
$imagesDir = Join-Path $rootDir "src\WindowsToolbox.Package\Images"
$imageGenerator = Join-Path $scriptDir "Create-Placeholder-Images.ps1"

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Windows Toolbox Installer Builder" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check prerequisites
Write-Host "Checking prerequisites..." -ForegroundColor Yellow

# Check for .NET SDK
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET SDK not found. Please install .NET 8 SDK." -ForegroundColor Red
    exit 1
}

# Check for MSBuild (for WAP project)
$msbuildPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe `
    -prerelease | Select-Object -First 1

if (-not $msbuildPath) {
    Write-Host "✗ MSBuild not found. Please install Visual Studio 2022 with .NET Desktop Development workload." -ForegroundColor Red
    exit 1
}
Write-Host "✓ MSBuild: $msbuildPath" -ForegroundColor Green

# Check solution file
if (-not (Test-Path $solutionFile)) {
    Write-Host "✗ Solution file not found: $solutionFile" -ForegroundColor Red
    exit 1
}
Write-Host "✓ Solution file found" -ForegroundColor Green

if ($Version) {
    if ($Version -notmatch '^\d+\.\d+\.\d+\.\d+$') {
        Write-Host "✗ Version must have four numeric parts (for example, 1.2.3.0)." -ForegroundColor Red
        exit 1
    }

    [xml]$manifest = Get-Content $manifestFile
    $manifest.Package.Identity.Version = $Version
    $manifest.Save($manifestFile)
    Write-Host "✓ Package version set to $Version" -ForegroundColor Green
}

$requiredImages = @(
    'StoreLogo.png',
    'Square44x44Logo.png',
    'SmallTile.png',
    'Square150x150Logo.png',
    'LargeTile.png',
    'Wide310x150Logo.png',
    'SplashScreen.png'
)
$missingImages = $requiredImages | Where-Object { -not (Test-Path (Join-Path $imagesDir $_)) }
if ($missingImages.Count -gt 0) {
    Write-Host "Package images are missing; generating development placeholders." -ForegroundColor Yellow
    & $imageGenerator -OutputPath $imagesDir
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to generate package images" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""

# Restore NuGet packages, including the Windows Application Packaging project.
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
& $msbuildPath $solutionFile `
    /restore `
    /p:Platform=$(if ($Platform -eq 'All') { 'x64' } else { $Platform }) `
    /verbosity:minimal
if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to restore NuGet packages" -ForegroundColor Red
    exit 1
}
Write-Host "✓ NuGet packages restored" -ForegroundColor Green
Write-Host ""

# Build the application
if (-not $SkipBuild) {
    $platforms = if ($Platform -eq 'All') { @('x86', 'x64', 'ARM64') } else { @($Platform) }
    
    foreach ($plat in $platforms) {
        Write-Host "Building $Configuration|$plat..." -ForegroundColor Yellow
        
        & $msbuildPath $solutionFile `
            /p:Configuration=$Configuration `
            /p:Platform=$plat `
            /p:AppxBundle=Always `
            /p:AppxBundlePlatforms="$($platforms -join '|')" `
            /p:UapAppxPackageBuildMode=SideloadOnly `
            /maxcpucount `
            /verbosity:minimal
        
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Build failed for $plat" -ForegroundColor Red
            exit 1
        }
        Write-Host "✓ Build completed for $plat" -ForegroundColor Green
    }
} else {
    Write-Host "Skipping build (using existing binaries)" -ForegroundColor Yellow
}

Write-Host ""

# Create output directory
$outputDir = Join-Path $rootDir $OutputPath
if (-not (Test-Path $outputDir)) {
    New-Item -ItemType Directory -Path $outputDir | Out-Null
}

# Copy installer packages to output
Write-Host "Copying installer packages to $outputDir..." -ForegroundColor Yellow
$packageOutputDir = Join-Path $rootDir "src\WindowsToolbox.Package\AppPackages"

if (Test-Path $packageOutputDir) {
    $packages = @(Get-ChildItem -Path $packageOutputDir -Filter "*.msix*" -Recurse |
        Where-Object { $_.Name -notlike "*_Test*" } |
        Select-Object -ExpandProperty FullName)

    if ($packages.Count -eq 0) {
        Write-Host "✗ Build completed without producing an MSIX package." -ForegroundColor Red
        exit 1
    }

    foreach ($package in $packages) {
        $dest = Join-Path $outputDir (Split-Path $package -Leaf)
        Copy-Item $package -Destination $dest -Force
        Write-Host "  Copied: $(Split-Path $package -Leaf)" -ForegroundColor Gray
    }
    Write-Host "✓ Installer packages copied" -ForegroundColor Green
} else {
    Write-Host "✗ Package output directory not found: $packageOutputDir" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Unsigned installer build completed successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Output directory: $outputDir" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Sign the package with build\Sign-Package.ps1 and a trusted certificate"
Write-Host "  2. Install the signed package with Add-AppxPackage"
Write-Host "  3. Test it on a clean Windows machine"
Write-Host ""
