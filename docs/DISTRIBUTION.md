# Distributing Windows Toolbox

This guide explains how to distribute the Windows Toolbox installer to end users.

## Distribution Options

There are several ways to distribute Windows Toolbox:

1. **GitHub Releases** (Recommended) - Free, automatic updates
2. **Microsoft Store** - Maximum reach, built-in updates
3. **Direct Download** - Simple, but manual updates
4. **Enterprise Deployment** - Group Policy, SCCM, Intune

## Option 1: GitHub Releases (Recommended)

### Advantages
- ✅ Free hosting
- ✅ Built-in update mechanism
- ✅ Version control integration
- ✅ Easy for developers
- ✅ Automatic updates via UpdateService

### Setup

1. **Build and sign your package**
   ```powershell
   .\build\Build-Installer.ps1 -Configuration Release -Platform All
   .\build\Sign-Package.ps1 -PackagePath ".\dist\WindowsToolbox.msixbundle" -CertificatePath ".\cert.pfx"
   ```

2. **Create a new release on GitHub**
   - Go to https://github.com/rich-wisdom/Toolbox/releases
   - Click "Draft a new release"
   - Choose a tag (e.g., `v1.0.0`)
   - Write release notes
   - Upload the signed MSIX package
   - Click "Publish release"

3. **Users install from release**
   - Users download the `.msixbundle` file
   - Double-click to install
   - Updates are checked automatically on app startup

### Automated Releases with GitHub Actions

The repository includes a GitHub Actions workflow for automated releases:

```yaml
# Triggered on version tags (e.g., v1.0.0)
# Builds, signs (if configured), and publishes to GitHub Releases
```

See `.github/workflows/build-release.yml` for details.

## Option 2: Microsoft Store

### Advantages
- ✅ Maximum visibility and reach
- ✅ Built-in updates and licensing
- ✅ User trust (verified publisher)
- ✅ No code signing certificate needed

### Disadvantages
- ❌ Review process (1-3 business days)
- ❌ Store fee ($19 one-time)
- ❌ Store policies and restrictions

### Setup

1. **Create a Microsoft Partner Center account**
   - Go to https://partner.microsoft.com/dashboard
   - Register as an individual or company ($19 one-time fee)

2. **Reserve your app name**
   - Create a new app submission
   - Reserve "Windows Toolbox"

3. **Prepare Store submission**
   - Build MSIX package with Store configuration
   - Update Package.appxmanifest with Store identity
   - Prepare screenshots, descriptions, privacy policy

4. **Submit for review**
   - Upload the MSIX package
   - Fill out Store listing details
   - Submit for certification

5. **Publish**
   - After approval (1-3 days), publish to Store
   - Updates are pushed automatically

## Option 3: Direct Download

### Advantages
- ✅ Full control over distribution
- ✅ No third-party dependencies
- ✅ Can host on your own website

### Disadvantages
- ❌ Requires hosting
- ❌ Manual update notification
- ❌ Less user trust

### Setup

1. **Host the signed MSIX package**
   - Upload to your website or CDN
   - Ensure HTTPS for security

2. **Create download page**
   - Provide clear installation instructions
   - Include system requirements
   - Link to troubleshooting guide

3. **Update mechanism**
   - The app checks GitHub Releases for updates by default
   - Or implement custom update server

## Option 4: Enterprise Deployment

### Group Policy (GPO)

1. **Prepare the package**
   ```powershell
   .\build\Build-Installer.ps1 -Configuration Release
   .\build\Sign-Package.ps1 -PackagePath ".\dist\WindowsToolbox.msix" -CertificatePath ".\cert.pfx"
   ```

2. **Deploy via GPO**
   - Copy MSIX to network share
   - Create new GPO: Computer Configuration → Policies → Software Settings → Software Installation
   - Add new package, select MSIX file
   - Assign to target computers

### Microsoft Intune

1. **Sign in to Intune**
   - Go to https://endpoint.microsoft.com

2. **Add app**
   - Apps → All apps → Add
   - Select "Line-of-business app"
   - Upload MSIX package

3. **Assign to groups**
   - Select target user/device groups
   - Configure deployment settings
   - Save and deploy

### SCCM (Configuration Manager)

1. **Create application**
   - Software Library → Applications → Create Application
   - Select MSIX file

2. **Configure deployment type**
   - Detection method: MSIX package identity
   - Installation command: automatic for MSIX

3. **Distribute content**
   - Distribute to distribution points
   - Deploy to collections

## Code Signing

For distribution outside the Microsoft Store, you MUST sign your package with a trusted code signing certificate.

### Getting a Certificate

1. **Purchase from Certificate Authority**
   - DigiCert: ~$299/year
   - Sectigo: ~$199/year
   - GlobalSign: ~$249/year

2. **Certificate requirements**
   - Extended Validation (EV) or Organization Validation (OV)
   - Code Signing usage
   - Valid for Windows Platform

### Signing the Package

```powershell
.\build\Sign-Package.ps1 `
  -PackagePath ".\dist\WindowsToolbox.msixbundle" `
  -CertificatePath ".\MyCert.pfx" `
  -CertificatePassword $env:CERT_PASSWORD
```

## Auto-Update System

Windows Toolbox includes a built-in update checker that:

1. **Checks for updates on startup**
   - Queries GitHub Releases API
   - Compares latest version with installed version

2. **Notifies users of updates**
   - Shows update notification in app
   - Displays release notes

3. **Downloads and installs updates**
   - One-click update installation
   - Automatically restarts app after update

### Configuration

The update system is configured in `UpdateService.cs`:

```csharp
private const string GitHubApiUrl = "https://api.github.com/repos/rich-wisdom/Toolbox/releases/latest";
```

To use a different update source, modify this URL.

### Disabling Auto-Updates

Users can disable automatic update checks in Settings → General → Check for updates.

## Update Frequency

- **Automatic check**: Once per day (first app launch of the day)
- **Manual check**: Settings → About → Check for updates
- **Silent updates**: Not supported (user confirmation required)

## Telemetry and Analytics (Optional)

Consider adding telemetry to understand usage:

1. **Microsoft AppCenter** - Free analytics and crash reporting
2. **Google Analytics** - Web-style analytics
3. **Custom solution** - Log to your own server

⚠️ **Important**: Respect user privacy and comply with GDPR/CCPA. Always provide opt-out.

## Distribution Checklist

Before distributing your first release:

- [ ] Build in Release configuration
- [ ] Sign with trusted certificate
- [ ] Test installation on clean Windows 10 machine
- [ ] Test installation on clean Windows 11 machine
- [ ] Test update mechanism
- [ ] Prepare release notes
- [ ] Update version number in Package.appxmanifest
- [ ] Create privacy policy (required for Store)
- [ ] Create support page/email
- [ ] Test uninstallation process

## Support

Provide these resources for users:

- **GitHub Issues** - Bug reports and feature requests
- **Email Support** - Direct user support
- **Documentation** - Installation and usage guides
- **FAQ** - Common questions and troubleshooting

## Legal Requirements

### Privacy Policy

Required if you collect any user data. Even minimal telemetry requires disclosure.

Example: https://www.freeprivacypolicy.com/

### Terms of Service

Recommended to limit liability and define usage terms.

### Open Source License

Specify license for your code (MIT, GPL, Apache, etc.)

### Third-Party Notices

Include licenses for all dependencies (FFmpeg, libraries, etc.)

## Monetization (Optional)

Consider these models:

1. **Freemium** - Basic features free, premium features paid
2. **One-time purchase** - Pay once, lifetime license
3. **Subscription** - Monthly/yearly recurring
4. **Donations** - GitHub Sponsors, Buy Me a Coffee

Implement licensing in `ILicenseService.cs`.

## Marketing

Promote your app:

- **GitHub** - Good README with screenshots
- **Reddit** - r/Windows, r/software
- **Twitter/X** - Tech community
- **Product Hunt** - Launch announcement
- **YouTube** - Demo videos
- **Blog posts** - Feature tutorials

## Metrics to Track

Monitor these metrics post-launch:

- Downloads per day/week/month
- Active users (DAU/MAU)
- Update adoption rate
- Crash-free sessions
- Most used tools
- User retention

## Next Steps

1. Build and test your installer (see BUILD.md)
2. Set up distribution method (GitHub Releases recommended)
3. Create release with signed package
4. Announce to users
5. Monitor feedback and iterate

## Resources

- [MSIX Package Signing](https://docs.microsoft.com/windows/msix/package/signing-package-overview)
- [Microsoft Partner Center](https://partner.microsoft.com/)
- [GitHub Releases](https://docs.github.com/repositories/releasing-projects-on-github)
- [Windows App Deployment Guide](https://docs.microsoft.com/windows/application-management/)
