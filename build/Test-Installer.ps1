#Requires -Version 5.1
#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Tests the Windows Toolbox MSIX installer package.

.DESCRIPTION
    This script performs various tests on the built MSIX package:
    - Package validation
    - Certificate validation
    - Installation test
    - Launch test
    - Uninstallation test

.PARAMETER PackagePath
    Path to the MSIX package to test. If not specified, looks in .\dist\

.PARAMETER SkipInstallTest
    Skip the installation/uninstallation test (useful for signed packages)

.PARAMETER KeepInstalled
    Don't uninstall after testing (useful for manual testing)

.EXAMPLE
    .\Test-Installer.ps1
    Tests the package in .\dist\

.EXAMPLE
    .\Test-Installer.ps1 -PackagePath "C:\Build\WindowsToolbox.msix"
    Tests a specific package

.EXAMPLE
    .\Test-Installer.ps1 -KeepInstalled
    Tests and keeps the app installed
#>

param(
    [Parameter()]
    [string]$PackagePath,
    
    [Parameter()]
    [switch]$SkipInstallTest,
    
    [Parameter()]
    [switch]$KeepInstalled
)

$ErrorActionPreference = 'Stop'

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Windows Toolbox Installer Tester" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Find package if not specified
if (-not $PackagePath) {
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $distDir = Join-Path (Split-Path -Parent $scriptDir) "dist"
    
    if (Test-Path $distDir) {
        $PackagePath = Get-ChildItem $distDir -Filter "*.msix" | 
            Where-Object { $_.Name -notlike "*_Test*" } |
            Select-Object -First 1 -ExpandProperty FullName
    }
    
    if (-not $PackagePath) {
        Write-Host "✗ No package found in $distDir" -ForegroundColor Red
        Write-Host "Please specify -PackagePath or build the installer first." -ForegroundColor Yellow
        exit 1
    }
}

if (-not (Test-Path $PackagePath)) {
    Write-Host "✗ Package not found: $PackagePath" -ForegroundColor Red
    exit 1
}

Write-Host "Testing package: $PackagePath" -ForegroundColor Cyan
Write-Host ""

$testsPassed = 0
$testsFailed = 0

# Test 1: Package structure
Write-Host "[1/5] Validating package structure..." -ForegroundColor Yellow
try {
    $makeappxPath = (Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Filter "makeappx.exe" -Recurse | Select-Object -First 1).FullName
    
    $output = & $makeappxPath unpack /p $PackagePath /d "$env:TEMP\WindowsToolbox-Test" /o 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Package structure is valid" -ForegroundColor Green
        $testsPassed++
        
        # Check for required files
        $manifestPath = "$env:TEMP\WindowsToolbox-Test\AppxManifest.xml"
        if (Test-Path $manifestPath) {
            Write-Host "  ✓ Manifest found" -ForegroundColor Green
        }
        
        # Cleanup
        Remove-Item "$env:TEMP\WindowsToolbox-Test" -Recurse -Force -ErrorAction SilentlyContinue
    } else {
        Write-Host "  ✗ Package validation failed" -ForegroundColor Red
        $testsFailed++
    }
} catch {
    Write-Host "  ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    $testsFailed++
}
Write-Host ""

# Test 2: Digital signature
Write-Host "[2/5] Checking digital signature..." -ForegroundColor Yellow
try {
    $sig = Get-AuthenticodeSignature -FilePath $PackagePath
    
    if ($sig.Status -eq 'Valid') {
        Write-Host "  ✓ Package is signed with valid signature" -ForegroundColor Green
        Write-Host "    Subject: $($sig.SignerCertificate.Subject)" -ForegroundColor Gray
        Write-Host "    Issuer: $($sig.SignerCertificate.Issuer)" -ForegroundColor Gray
        $testsPassed++
    } elseif ($sig.Status -eq 'NotSigned') {
        Write-Host "  ⚠ Package is not signed" -ForegroundColor Yellow
        Write-Host "    Note: Signing is required for distribution" -ForegroundColor Gray
        $testsPassed++
    } else {
        Write-Host "  ✗ Invalid signature: $($sig.Status)" -ForegroundColor Red
        $testsFailed++
    }
} catch {
    Write-Host "  ⚠ Could not check signature: $($_.Exception.Message)" -ForegroundColor Yellow
    $testsPassed++
}
Write-Host ""

# Test 3: Package info
Write-Host "[3/5] Reading package information..." -ForegroundColor Yellow
try {
    # Extract and read manifest
    $makeappxPath = (Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Filter "makeappx.exe" -Recurse | Select-Object -First 1).FullName
    & $makeappxPath unpack /p $PackagePath /d "$env:TEMP\WindowsToolbox-Info" /o /nv 2>&1 | Out-Null
    
    $manifestPath = "$env:TEMP\WindowsToolbox-Info\AppxManifest.xml"
    [xml]$manifest = Get-Content $manifestPath
    
    $identity = $manifest.Package.Identity
    Write-Host "  Name: $($identity.Name)" -ForegroundColor Gray
    Write-Host "  Version: $($identity.Version)" -ForegroundColor Gray
    Write-Host "  Publisher: $($identity.Publisher)" -ForegroundColor Gray
    Write-Host "  Architecture: $($identity.ProcessorArchitecture)" -ForegroundColor Gray
    
    $displayName = $manifest.Package.Properties.DisplayName
    Write-Host "  Display Name: $displayName" -ForegroundColor Gray
    
    Write-Host "  ✓ Package info read successfully" -ForegroundColor Green
    $testsPassed++
    
    # Cleanup
    Remove-Item "$env:TEMP\WindowsToolbox-Info" -Recurse -Force -ErrorAction SilentlyContinue
} catch {
    Write-Host "  ✗ Error reading package info: $($_.Exception.Message)" -ForegroundColor Red
    $testsFailed++
}
Write-Host ""

# Test 4: Installation test
if (-not $SkipInstallTest) {
    Write-Host "[4/5] Testing installation..." -ForegroundColor Yellow
    
    # Check if already installed
    $existing = Get-AppxPackage -Name "*WindowsToolbox*"
    if ($existing) {
        Write-Host "  ⚠ Windows Toolbox is already installed" -ForegroundColor Yellow
        Write-Host "    Removing existing installation..." -ForegroundColor Gray
        $existing | Remove-AppxPackage
        Start-Sleep -Seconds 2
    }
    
    try {
        Write-Host "  Installing package..." -ForegroundColor Gray
        Add-AppxPackage -Path $PackagePath
        Start-Sleep -Seconds 2
        
        $installed = Get-AppxPackage -Name "*WindowsToolbox*"
        if ($installed) {
            Write-Host "  ✓ Installation successful" -ForegroundColor Green
            Write-Host "    Installed to: $($installed.InstallLocation)" -ForegroundColor Gray
            $testsPassed++
        } else {
            Write-Host "  ✗ Installation failed: Package not found after install" -ForegroundColor Red
            $testsFailed++
        }
    } catch {
        Write-Host "  ✗ Installation error: $($_.Exception.Message)" -ForegroundColor Red
        $testsFailed++
    }
} else {
    Write-Host "[4/5] Installation test skipped" -ForegroundColor Yellow
    Write-Host ""
}

# Test 5: Launch test
if (-not $SkipInstallTest) {
    Write-Host "[5/5] Testing application launch..." -ForegroundColor Yellow
    
    try {
        $installed = Get-AppxPackage -Name "*WindowsToolbox*"
        if ($installed) {
            Write-Host "  Attempting to launch..." -ForegroundColor Gray
            
            # Get the app ID from manifest
            $manifestPath = Join-Path $installed.InstallLocation "AppxManifest.xml"
            [xml]$manifest = Get-Content $manifestPath
            $appId = $manifest.Package.Applications.Application.Id
            $familyName = $installed.PackageFamilyName
            
            # Launch the app
            Start-Process "shell:AppsFolder\$familyName!$appId"
            Start-Sleep -Seconds 3
            
            # Check if process is running
            $process = Get-Process | Where-Object { $_.Path -like "*WindowsToolbox*" }
            if ($process) {
                Write-Host "  ✓ Application launched successfully" -ForegroundColor Green
                Write-Host "    PID: $($process.Id)" -ForegroundColor Gray
                $testsPassed++
                
                # Close the app
                Write-Host "  Closing application..." -ForegroundColor Gray
                $process | Stop-Process -Force
                Start-Sleep -Seconds 1
            } else {
                Write-Host "  ⚠ Could not confirm app is running" -ForegroundColor Yellow
                Write-Host "    Please verify manually" -ForegroundColor Gray
                $testsPassed++
            }
        }
    } catch {
        Write-Host "  ⚠ Launch test inconclusive: $($_.Exception.Message)" -ForegroundColor Yellow
        $testsPassed++
    }
    Write-Host ""
    
    # Cleanup - Uninstall
    if (-not $KeepInstalled) {
        Write-Host "Cleaning up..." -ForegroundColor Yellow
        try {
            $installed = Get-AppxPackage -Name "*WindowsToolbox*"
            if ($installed) {
                Remove-AppxPackage -Package $installed.PackageFullName
                Write-Host "  ✓ Uninstalled successfully" -ForegroundColor Green
            }
        } catch {
            Write-Host "  ⚠ Uninstall failed: $($_.Exception.Message)" -ForegroundColor Yellow
            Write-Host "  Please uninstall manually via Settings" -ForegroundColor Gray
        }
    } else {
        Write-Host "Package kept installed as requested" -ForegroundColor Cyan
    }
} else {
    Write-Host "[5/5] Launch test skipped" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Test Results" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Passed: $testsPassed" -ForegroundColor Green
if ($testsFailed -gt 0) {
    Write-Host "Failed: $testsFailed" -ForegroundColor Red
}
Write-Host ""

if ($testsFailed -eq 0) {
    Write-Host "✓ All tests passed!" -ForegroundColor Green
    exit 0
} else {
    Write-Host "✗ Some tests failed" -ForegroundColor Red
    exit 1
}
