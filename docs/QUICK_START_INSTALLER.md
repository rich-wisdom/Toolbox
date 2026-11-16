# Quick Start: Windows Toolbox Installer

Get the Windows Toolbox installer up and running in minutes.

## For Developers: First Build

### 1. Generate Placeholder Images

```powershell
cd build
.\Create-Placeholder-Images.ps1
```

This creates temporary app icons for development. Replace with your branding before release.

### 2. Build the Installer

```powershell
.\Build-Installer.ps1
```

This builds a Release x64 installer by default. Find it in `dist/`.

### 3. Test It

```powershell
.\Test-Installer.ps1 -KeepInstalled
```

This validates and installs the package for testing.

## For End Users: Installation

### Option 1: Download and Install

1. Go to [Releases](https://github.com/rich-wisdom/Toolbox/releases/latest)
2. Download `WindowsToolbox_<version>_x64.msixbundle`
3. Double-click to install
4. Click "Install" in the dialog

### Option 2: Via Command Line

```powershell
Add-AppxPackage -Path WindowsToolbox_1.0.0_x64.msix
```

## For Distributors: Release Process

### 1. Update Version

Edit `src/WindowsToolbox.Package/Package.appxmanifest`:

```xml
<Identity Version="1.0.0.0" ... />
```

### 2. Build for All Platforms

```powershell
.\build\Build-Installer.ps1 -Platform All -Configuration Release
```

### 3. Sign the Packages

```powershell
Get-ChildItem .\dist -Filter "*.msix*" | ForEach-Object {
    .\build\Sign-Package.ps1 -PackagePath $_.FullName -CertificatePath ".\cert.pfx" -CertificatePassword $env:CERT_PASSWORD
}
```

### 4. Create GitHub Release

```bash
# Tag the version
git tag -a v1.0.0 -m "Release v1.0.0"

# Push the tag (triggers GitHub Actions)
git push origin v1.0.0
```

GitHub Actions will automatically:
- Build all platforms
- Create MSIX bundle
- Publish to GitHub Releases with release notes

## Common Tasks

### Build Debug Version

```powershell
.\build\Build-Installer.ps1 -Configuration Debug
```

### Build Specific Platform

```powershell
# For ARM64 devices (Surface Pro X)
.\build\Build-Installer.ps1 -Platform ARM64

# For 32-bit systems
.\build\Build-Installer.ps1 -Platform x86
```

### Test Without Installing

```powershell
.\build\Test-Installer.ps1 -SkipInstallTest
```

### Create Custom Color Icons

```powershell
# Red icons
.\build\Create-Placeholder-Images.ps1 -Color "#FF0000"

# Green icons
.\build\Create-Placeholder-Images.ps1 -Color "#00FF00"
```

## Troubleshooting

### "MSBuild not found"

Install Visual Studio 2022 with .NET Desktop Development workload.

### "Package validation failed"

1. Clean the solution: `msbuild /t:Clean`
2. Delete `bin` and `obj` folders
3. Try again

### "Certificate not trusted"

For development certificates:

```powershell
# Install certificate to Trusted Root
$cert = Get-AuthenticodeSignature -FilePath "WindowsToolbox.msix"
Export-Certificate -Cert $cert.SignerCertificate -FilePath "temp.cer"
Import-Certificate -FilePath "temp.cer" -CertStoreLocation Cert:\LocalMachine\Root
```

### "Installation failed"

- Check Windows version: Win+R → `winver` (need 1809+)
- Install .NET 8 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
- Run PowerShell as Administrator

## Next Steps

- **New to MSIX?** Read [BUILD.md](BUILD.md)
- **Ready to distribute?** Read [DISTRIBUTION.md](DISTRIBUTION.md)
- **End user guide**: [INSTALL.md](INSTALL.md)
- **Full details**: [INSTALLER_SUMMARY.md](INSTALLER_SUMMARY.md)

## Get Help

- [Build Documentation](BUILD.md) - Comprehensive build guide
- [GitHub Issues](https://github.com/rich-wisdom/Toolbox/issues) - Report problems
- [Discussions](https://github.com/rich-wisdom/Toolbox/discussions) - Ask questions

## Checklist: First Release

Before your first public release:

- [ ] Replace placeholder images with branded icons
- [ ] Obtain code signing certificate
- [ ] Test on clean Windows 10 machine
- [ ] Test on clean Windows 11 machine
- [ ] Update version to 1.0.0
- [ ] Write release notes
- [ ] Create privacy policy
- [ ] Set up support channel
- [ ] Test update mechanism
- [ ] Review all documentation

## What's Included

The installer system includes:

✅ MSIX packaging project  
✅ Auto-update with GitHub Releases  
✅ Multi-platform builds (x64, x86, ARM64)  
✅ Build scripts (PowerShell)  
✅ Code signing support  
✅ GitHub Actions CI/CD  
✅ Comprehensive documentation  
✅ Testing scripts  
✅ Image generation tools  
✅ Production-ready workflow  

Everything is documented and ready to use!
