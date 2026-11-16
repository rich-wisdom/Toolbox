# Build Scripts

This directory contains scripts for building, signing, and packaging Windows Toolbox.

## Scripts

### Build-Installer.ps1

Builds the MSIX installer package.

```powershell
# Basic usage
.\Build-Installer.ps1

# With options
.\Build-Installer.ps1 -Configuration Release -Platform x64 -Version 1.0.0.0
```

**Parameters:**
- `-Configuration` - Debug or Release (default: Release)
- `-Platform` - x86, x64, ARM64, or All (default: x64)
- `-Version` - Version number (optional)
- `-SkipBuild` - Skip build, only package
- `-OutputPath` - Custom output directory (default: ./dist)

### Sign-Package.ps1

Signs the MSIX package with a code signing certificate.

```powershell
# Sign with PFX file
.\Sign-Package.ps1 `
  -PackagePath ".\dist\WindowsToolbox.msix" `
  -CertificatePath ".\cert.pfx" `
  -CertificatePassword "password"

# Sign with certificate from store
.\Sign-Package.ps1 `
  -PackagePath ".\dist\WindowsToolbox.msix" `
  -CertificateThumbprint "ABC123..."
```

**Parameters:**
- `-PackagePath` - Path to MSIX package (required)
- `-CertificatePath` - Path to PFX certificate
- `-CertificatePassword` - PFX password
- `-CertificateThumbprint` - Certificate thumbprint from store
- `-TimestampServer` - Timestamp server URL

### Create-Placeholder-Images.ps1

Creates placeholder images for development/testing.

```powershell
# Create default blue placeholders
.\Create-Placeholder-Images.ps1

# Custom color
.\Create-Placeholder-Images.ps1 -Color "#FF6B6B"

# Custom output path
.\Create-Placeholder-Images.ps1 -OutputPath "C:\Images"
```

**Parameters:**
- `-OutputPath` - Where to save images
- `-Color` - Hex color code (default: #0078D7)

## Workflow

### Development Build

1. Create placeholder images (first time only):
   ```powershell
   .\Create-Placeholder-Images.ps1
   ```

2. Build the installer:
   ```powershell
   .\Build-Installer.ps1 -Configuration Debug
   ```

3. Test installation on your machine

### Release Build

1. Ensure you have proper branding images in `src/WindowsToolbox.Package/Images/`

2. Build for all platforms:
   ```powershell
   .\Build-Installer.ps1 -Configuration Release -Platform All -Version 1.0.0.0
   ```

3. Sign the packages:
   ```powershell
   Get-ChildItem .\dist -Filter "*.msix" | ForEach-Object {
       .\Sign-Package.ps1 `
         -PackagePath $_.FullName `
         -CertificatePath ".\cert.pfx" `
         -CertificatePassword $env:CERT_PASSWORD
   }
   ```

4. Test signed packages

5. Upload to GitHub Releases

## Continuous Integration

The GitHub Actions workflow (`.github/workflows/build-release.yml`) automatically:

1. Builds all platforms
2. Creates MSIX bundle
3. Creates GitHub Release with packages
4. Runs on version tags (e.g., `v1.0.0`)

### Trigger a Release

```bash
# Tag a version
git tag -a v1.0.0 -m "Release version 1.0.0"

# Push the tag
git push origin v1.0.0

# GitHub Actions will automatically build and create release
```

## Prerequisites

- Visual Studio 2022 with:
  - .NET Desktop Development
  - Universal Windows Platform development
  - Windows 11 SDK
- .NET 8 SDK
- PowerShell 5.1 or later

## Troubleshooting

### "MSBuild not found"

Install Visual Studio 2022 with .NET Desktop Development workload.

### "SignTool not found"

Install Windows SDK via Visual Studio Installer.

### "Package creation failed"

1. Clean the solution: `msbuild /t:Clean`
2. Delete `bin` and `obj` folders
3. Rebuild

### "Certificate not trusted"

For test certificates:
1. Double-click the certificate
2. Install to "Trusted Root Certification Authorities"

For production, use a certificate from a trusted CA.

## Additional Resources

- [BUILD.md](../docs/BUILD.md) - Detailed build instructions
- [DISTRIBUTION.md](../docs/DISTRIBUTION.md) - Distribution guide
- [INSTALL.md](../docs/INSTALL.md) - Installation instructions
