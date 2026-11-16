#Requires -Version 5.1
<#
.SYNOPSIS
    Signs the Windows Toolbox MSIX package with a code signing certificate.

.DESCRIPTION
    This script signs the MSIX package with a code signing certificate.
    You can use a PFX file or a certificate from the certificate store.

.PARAMETER PackagePath
    Path to the MSIX package to sign. Required.

.PARAMETER CertificatePath
    Path to the PFX certificate file. Mutually exclusive with CertificateThumbprint.

.PARAMETER CertificatePassword
    Password for the PFX certificate. Only used with CertificatePath.

.PARAMETER CertificateThumbprint
    Thumbprint of the certificate in the certificate store. Mutually exclusive with CertificatePath.

.PARAMETER TimestampServer
    URL of the timestamp server. Default: http://timestamp.digicert.com

.EXAMPLE
    .\Sign-Package.ps1 -PackagePath .\dist\WindowsToolbox.msix -CertificatePath .\cert.pfx -CertificatePassword "password"
    Signs the package with a PFX file

.EXAMPLE
    .\Sign-Package.ps1 -PackagePath .\dist\WindowsToolbox.msix -CertificateThumbprint "1234567890ABCDEF"
    Signs the package with a certificate from the store
#>

param(
    [Parameter(Mandatory=$true)]
    [string]$PackagePath,
    
    [Parameter(ParameterSetName='PFX')]
    [string]$CertificatePath,
    
    [Parameter(ParameterSetName='PFX')]
    [string]$CertificatePassword,
    
    [Parameter(ParameterSetName='Store')]
    [string]$CertificateThumbprint,
    
    [Parameter()]
    [string]$TimestampServer = "http://timestamp.digicert.com"
)

$ErrorActionPreference = 'Stop'

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Windows Toolbox Package Signer" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if package exists
if (-not (Test-Path $PackagePath)) {
    Write-Host "✗ Package not found: $PackagePath" -ForegroundColor Red
    exit 1
}

# Find SignTool
$signToolPath = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" `
    -latest -find "**/signtool.exe" -prerelease | Select-Object -First 1

if (-not $signToolPath) {
    Write-Host "✗ SignTool not found. Please install Windows SDK." -ForegroundColor Red
    exit 1
}

Write-Host "✓ SignTool: $signToolPath" -ForegroundColor Green
Write-Host ""

# Build SignTool command
$signToolArgs = @(
    'sign'
    '/fd', 'SHA256'
    '/tr', $TimestampServer
    '/td', 'SHA256'
)

if ($PSCmdlet.ParameterSetName -eq 'PFX') {
    if (-not (Test-Path $CertificatePath)) {
        Write-Host "✗ Certificate not found: $CertificatePath" -ForegroundColor Red
        exit 1
    }
    
    $signToolArgs += @(
        '/f', $CertificatePath
    )
    
    if ($CertificatePassword) {
        $signToolArgs += @('/p', $CertificatePassword)
    }
} elseif ($PSCmdlet.ParameterSetName -eq 'Store') {
    $signToolArgs += @(
        '/sha1', $CertificateThumbprint
    )
}

$signToolArgs += $PackagePath

# Sign the package
Write-Host "Signing package..." -ForegroundColor Yellow
Write-Host "Package: $PackagePath" -ForegroundColor Gray
& $signToolPath $signToolArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to sign package" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Package signed successfully!" -ForegroundColor Green
Write-Host ""
