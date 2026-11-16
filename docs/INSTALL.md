# Installing Windows Toolbox

This guide will help you install Windows Toolbox on your Windows 10 or Windows 11 computer.

## System Requirements

### Minimum Requirements

- **Operating System**: Windows 10 version 1809 (build 17763) or later
- **Processor**: 1 GHz or faster
- **RAM**: 2 GB
- **Disk Space**: 500 MB free space
- **Architecture**: x64, x86, or ARM64

### Recommended Requirements

- **Operating System**: Windows 11 (latest version)
- **Processor**: Multi-core processor, 2 GHz or faster
- **RAM**: 4 GB or more
- **Disk Space**: 1 GB free space
- **Display**: 1920x1080 or higher resolution

## Installation Methods

### Method 1: MSIX Package (Recommended)

This is the easiest and most secure installation method.

#### Step 1: Download

1. Go to the [Releases page](https://github.com/rich-wisdom/Toolbox/releases/latest)
2. Download the latest `.msixbundle` or `.msix` file
3. Save it to your Downloads folder

#### Step 2: Install Certificate (First Time Only)

If this is your first time installing Windows Toolbox, you need to install the certificate:

1. Right-click the downloaded `.msix` file
2. Select **Properties**
3. Click the **Digital Signatures** tab
4. Select the signature and click **Details**
5. Click **View Certificate**
6. Click **Install Certificate**
7. Choose **Local Machine** (requires administrator)
8. Click **Next**
9. Select **Place all certificates in the following store**
10. Click **Browse** and select **Trusted Root Certification Authorities**
11. Click **OK**, then **Next**, then **Finish**

#### Step 3: Install the App

1. Double-click the downloaded `.msix` file
2. Click **Install**
3. Wait for installation to complete (usually 30 seconds)
4. Click **Launch** or find "Windows Toolbox" in Start Menu

### Method 2: Microsoft Store (Coming Soon)

Once available on Microsoft Store:

1. Open **Microsoft Store**
2. Search for "Windows Toolbox"
3. Click **Get** or **Install**
4. Launch from Start Menu

### Method 3: Winget (Coming Soon)

Using Windows Package Manager:

```powershell
winget install RichWisdom.WindowsToolbox
```

## First Launch

When you launch Windows Toolbox for the first time:

1. **Welcome Screen**: Brief introduction to the app
2. **Global Hotkey**: The default hotkey is `Ctrl+Alt+Space`
3. **Tool Categories**: Browse 102 tools across 7 categories

## Features Setup

### Quick Launcher

Press `Ctrl+Alt+Space` anywhere to open the quick launcher:
- Type to search for tools
- Use arrow keys to navigate
- Press Enter to launch a tool

To change the hotkey:
1. Open Settings (gear icon)
2. Go to **Keyboard**
3. Click on hotkey field
4. Press your desired key combination

### MCP Integration (Optional)

To use Windows Toolbox with AI agents like Claude or Cursor:

1. Open Windows Toolbox
2. Go to Settings → MCP Server
3. Copy the MCP server path
4. Add to your AI agent configuration:

For Cursor:
```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "C:\\Program Files\\WindowsApp\\WindowsToolbox\\WindowsToolbox.MCP.exe"
    }
  }
}
```

For Claude Desktop:
```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "C:\\Program Files\\WindowsApp\\WindowsToolbox\\WindowsToolbox.MCP.exe",
      "args": []
    }
  }
}
```

### FFmpeg Setup (For Video/Audio Tools)

Some tools require FFmpeg. Windows Toolbox can automatically download it:

1. Launch any video or audio tool
2. If FFmpeg is not installed, you'll see a prompt
3. Click **Download FFmpeg** to automatically install
4. Wait for download to complete (about 100MB)

Or install manually:
1. Download FFmpeg from https://ffmpeg.org/download.html
2. Extract to a folder (e.g., `C:\ffmpeg`)
3. Add to PATH or configure in Settings → Tools → FFmpeg Path

## Updates

Windows Toolbox automatically checks for updates on startup.

### Automatic Updates

1. When an update is available, you'll see a notification
2. Click **Update** to download and install
3. The app will restart after update

### Manual Update Check

1. Open Settings → About
2. Click **Check for updates**
3. If available, click **Download and Install**

### Update Settings

To configure update behavior:
1. Settings → General
2. Toggle **Check for updates automatically**
3. Choose **Update channel**: Stable or Beta

## Uninstallation

### Method 1: Windows Settings

1. Open **Settings** (Windows key + I)
2. Go to **Apps** → **Installed apps**
3. Find **Windows Toolbox**
4. Click **...** (three dots)
5. Click **Uninstall**
6. Confirm

### Method 2: Start Menu

1. Find **Windows Toolbox** in Start Menu
2. Right-click on it
3. Select **Uninstall**
4. Confirm

### Method 3: PowerShell

```powershell
Get-AppxPackage *WindowsToolbox* | Remove-AppxPackage
```

## Troubleshooting

### Installation Issues

#### "This app package is not signed"

**Solution**: Install the certificate first (see Step 2 above)

#### "The app didn't start"

**Causes**:
- Missing .NET 8 Runtime
- Corrupted installation
- Insufficient permissions

**Solution**:
1. Install .NET 8 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
2. Restart your computer
3. Try installing again
4. Run as Administrator

#### "Windows protected your PC" (SmartScreen)

**Solution**:
1. Click **More info**
2. Click **Run anyway**
3. (Only do this for the official Windows Toolbox installer)

### Runtime Issues

#### Tools not working

**Check**:
1. FFmpeg is installed (for video/audio tools)
2. Internet connection (for download tools)
3. Disk space available
4. File permissions

#### Quick Launcher not opening

**Check**:
1. Another app using the same hotkey
2. Change hotkey in Settings
3. Run Windows Toolbox as Administrator

#### App crashes on startup

**Solution**:
1. Check Windows Event Viewer for error details
2. Reinstall the app
3. Check antivirus isn't blocking it
4. Report issue on GitHub

### Performance Issues

#### Slow app startup

**Causes**:
- Slow disk
- Antivirus scanning
- Many background apps

**Solution**:
1. Exclude Windows Toolbox folder from antivirus
2. Close unnecessary background apps
3. Install on SSD if available

#### High memory usage

Some tools (especially video processing) require significant memory. This is normal during processing.

### Getting Help

If you encounter issues not covered here:

1. **Check FAQ**: See TROUBLESHOOTING.md
2. **GitHub Issues**: https://github.com/rich-wisdom/Toolbox/issues
3. **Discussions**: https://github.com/rich-wisdom/Toolbox/discussions
4. **Email Support**: support@windowstoolbox.com (if available)

When reporting issues, include:
- Windows version (Win+R, type `winver`)
- App version (Settings → About)
- Error message (screenshot)
- Steps to reproduce

## Data and Privacy

### What data is stored?

Windows Toolbox stores locally:
- User preferences and settings
- Recent tools history
- Tool configurations
- MCP server settings

Location: `%LocalAppData%\WindowsToolbox\`

### Is data sent online?

- **Update checks**: Version number sent to GitHub API
- **Tool usage**: Not tracked (fully offline)
- **Crash reports**: Optional (can opt-out in Settings)
- **Telemetry**: None by default

### Privacy Policy

See PRIVACY.md for full privacy policy.

## Licenses

### Windows Toolbox License

[To be determined - MIT/GPL/Proprietary]

### Third-Party Licenses

Windows Toolbox uses these open-source components:

- **FFmpeg** - LGPL/GPL license
- **ImageSharp** - Apache License 2.0
- **iText7** - AGPL license (or commercial)
- **CommunityToolkit** - MIT license
- **Windows App SDK** - Microsoft license

See LICENSES.md for full license texts.

## Comparison with Alternatives

| Feature | Windows Toolbox | Other Tools |
|---------|-----------------|-------------|
| Number of tools | 102 | Varies |
| Installation | Single MSIX | Multiple apps |
| Quick Launch | Built-in | Separate launcher |
| AI Integration | Native MCP | None |
| Offline Use | Full | Limited |
| Cost | Free/Freemium | Varies |
| Updates | Automatic | Manual |
| Privacy | Fully local | Cloud-based |

## Next Steps

After installation:

1. **Explore Tools**: Browse all 102 tools by category
2. **Set Hotkey**: Customize quick launcher hotkey
3. **Try Quick Launcher**: Press `Ctrl+Alt+Space`
4. **Configure MCP**: If using with AI agents
5. **Install FFmpeg**: For video/audio tools

## Feedback

We'd love to hear from you:

- ⭐ Star us on GitHub: https://github.com/rich-wisdom/Toolbox
- 💬 Join discussions: https://github.com/rich-wisdom/Toolbox/discussions
- 🐛 Report bugs: https://github.com/rich-wisdom/Toolbox/issues
- 📧 Email feedback: feedback@windowstoolbox.com

Thank you for using Windows Toolbox!
