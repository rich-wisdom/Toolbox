# Building Windows Toolbox Installer

This guide explains how to build the Windows Toolbox MSIX installer package.

## Prerequisites

### Required Software

1. **Visual Studio 2022** (version 17.0 or later)
   - Install the following workloads:
     - .NET Desktop Development
     - Universal Windows Platform development
   - Individual components:
     - Windows 11 SDK (10.0.22621.0 or later)
     - MSIX Packaging Tools

2. **.NET 8 SDK**
   - Download from: https://dotnet.microsoft.com/download/dotnet/8.0

3. **Windows SDK** (10.0.17763.0 or later)
   - Included with Visual Studio 2022

### Optional Software

- **Windows Terminal** - For better PowerShell experience
- **Git** - For version control operations

## Building from Visual Studio

### Step 1: Open the Solution

1. Open `WindowsToolbox.sln` in Visual Studio 2022
2. Wait for NuGet packages to restore automatically

### Step 2: Select Configuration and Platform

1. Choose configuration: **Release** or **Debug**
2. Choose platform: **x64**, **x86**, or **ARM64**
   - For most users: **x64** is recommended
   - For Surface devices: **ARM64**

### Step 3: Build the Package

1. Right-click on `WindowsToolbox.Package` project
2. Select **Build** or press `Ctrl+B`
3. Wait for the build to complete (first build may take several minutes)

### Step 4: Locate the Output

The MSIX package will be created in:
```
src\WindowsToolbox.Package\AppPackages\
```

Look for files ending in `.msix` or `.msixbundle`.

## Building from Command Line

### Using the Build Script (Recommended)

Open PowerShell and navigate to the repository root, then run:

```powershell
# Build Release x64 (default)
.\build\Build-Installer.ps1

# Build Debug x86
.\build\Build-Installer.ps1 -Configuration Debug -Platform x86

# Build for all platforms
.\build\Build-Installer.ps1 -Platform All

# Build with custom version
.\build\Build-Installer.ps1 -Version 1.2.3.0

# Custom output directory
.\build\Build-Installer.ps1 -OutputPath "C:\Builds\WindowsToolbox"
```

The installer will be output to the `dist` directory by default.

### Manual Command Line Build

```powershell
# Restore packages
dotnet restore WindowsToolbox.sln

# Build the solution
msbuild WindowsToolbox.sln `
  /p:Configuration=Release `
  /p:Platform=x64 `
  /p:AppxBundle=Always `
  /p:AppxBundlePlatforms="x64" `
  /p:UapAppxPackageBuildMode=SideloadOnly
```

## Build Configurations

### Release vs Debug

- **Release**: Optimized for distribution, smaller size, no debugging symbols
- **Debug**: Includes debugging symbols, larger size, easier to debug

For distribution, always use **Release**.

### Platform Options

- **x64**: 64-bit Intel/AMD processors (most common)
- **x86**: 32-bit processors (legacy systems)
- **ARM64**: ARM-based processors (Surface Pro X, etc.)
- **All**: Creates a bundle for all platforms

## Code Signing

Before distributing your installer, you should sign it with a code signing certificate.

### Using the Signing Script

```powershell
# Sign with PFX file
.\build\Sign-Package.ps1 `
  -PackagePath ".\dist\WindowsToolbox.msix" `
  -CertificatePath ".\MyCert.pfx" `
  -CertificatePassword "MyPassword"

# Sign with certificate from store
.\build\Sign-Package.ps1 `
  -PackagePath ".\dist\WindowsToolbox.msix" `
  -CertificateThumbprint "1234567890ABCDEF"
```

### Creating a Test Certificate

For testing purposes only (not for distribution):

```powershell
# Generate a test certificate
$cert = New-SelfSignedCertificate `
  -Type Custom `
  -Subject "CN=Rich Wisdom" `
  -KeyUsage DigitalSignature `
  -FriendlyName "Windows Toolbox Test Certificate" `
  -CertStoreLocation "Cert:\CurrentUser\My" `
  -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

# Export to PFX
$password = ConvertTo-SecureString -String "password" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath "TestCert.pfx" -Password $password
```

### Getting a Production Certificate

For production distribution, obtain a code signing certificate from a trusted Certificate Authority:

- **DigiCert** - https://www.digicert.com/
- **Sectigo** (formerly Comodo) - https://www.sectigo.com/
- **GlobalSign** - https://www.globalsign.com/

Cost: Typically $100-$500/year

## Troubleshooting

### Build Errors

**Error: Windows SDK not found**
- Install Windows 11 SDK via Visual Studio Installer
- Modify Visual Studio → Individual Components → Windows 11 SDK

**Error: MSBuild not found**
- Ensure Visual Studio 2022 is installed with .NET Desktop Development workload

**Error: NuGet package restore failed**
- Clear NuGet cache: `dotnet nuget locals all --clear`
- Restore packages: `dotnet restore WindowsToolbox.sln`

### Package Errors

**Error: Package creation failed**
- Check that all projects build successfully first
- Ensure you have write permissions to the output directory
- Try cleaning the solution: `msbuild /t:Clean`

**Error: Certificate not trusted**
- For test certificates, install the certificate to Trusted Root Certification Authorities
- For production, use a certificate from a trusted CA

## Build Output

After a successful build, you will find:

```
dist/
├── WindowsToolbox_1.0.0.0_x64.msix           # Single-platform package
├── WindowsToolbox_1.0.0.0_x64_bundle.msixbundle  # Multi-platform bundle
└── Dependencies/                              # Required framework packages
    └── x64/
        ├── Microsoft.NET.CoreRuntime.8.0.msix
        └── Microsoft.WindowsAppRuntime.1.5.msix
```

## Next Steps

After building:

1. **Test the installer** on a clean Windows machine (see INSTALL.md)
2. **Sign the package** with your code signing certificate
3. **Distribute** via GitHub Releases or Microsoft Store (see DISTRIBUTION.md)

## Continuous Integration

See `.github/workflows/build-release.yml` for automated building with GitHub Actions.

## Additional Resources

- [MSIX Packaging Overview](https://docs.microsoft.com/windows/msix/)
- [Windows App SDK](https://docs.microsoft.com/windows/apps/windows-app-sdk/)
- [Code Signing Best Practices](https://docs.microsoft.com/windows/msix/package/signing-package-overview)
