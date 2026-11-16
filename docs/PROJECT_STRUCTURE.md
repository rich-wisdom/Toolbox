# Project Structure

## Directory Layout

```
windows-toolbox/
├── .github/
│   └── workflows/
│       └── build.yml                    # GitHub Actions CI/CD workflow
├── docs/
│   ├── ARCHITECTURE.md                  # Architecture documentation
│   ├── PROJECT_STRUCTURE.md            # This file
│   └── TOOLS.md                         # Complete list of all 102 tools
├── src/
│   ├── WindowsToolbox.App/
│   │   ├── Assets/                      # App assets (icons, images)
│   │   ├── Services/
│   │   │   └── IGlobalHotKeyService.cs  # Global hotkey registration
│   │   ├── ViewModels/
│   │   │   ├── MainViewModel.cs
│   │   │   └── QuickLauncherViewModel.cs
│   │   ├── Views/
│   │   │   ├── CategoryPage.xaml/cs     # Category tools page
│   │   │   ├── HomePage.xaml/cs         # Home page
│   │   │   ├── MainWindow.xaml/cs       # Main application window
│   │   │   ├── QuickLauncherWindow.xaml/cs  # Quick launcher UI
│   │   │   └── SettingsPage.xaml/cs     # Settings page
│   │   ├── App.xaml/cs                  # Application entry point
│   │   ├── app.manifest                 # Application manifest
│   │   └── WindowsToolbox.App.csproj    # WinUI 3 project file
│   ├── WindowsToolbox.Core/
│   │   ├── Interfaces/
│   │   │   └── ITool.cs                 # Core tool interface
│   │   ├── Models/
│   │   │   └── ToolCategory.cs          # Tool categories
│   │   ├── Services/
│   │   │   ├── IRecentToolsService.cs   # Recent tools tracking
│   │   │   ├── ISettingsService.cs      # Settings persistence
│   │   │   └── IToolRegistry.cs         # Tool registration & search
│   │   └── WindowsToolbox.Core.csproj
│   ├── WindowsToolbox.Tools/
│   │   ├── Audio/
│   │   │   └── CleanUpAudioTool.cs
│   │   ├── Developer/
│   │   │   └── JsonFormatterTool.cs     # ✅ Implemented
│   │   ├── Document/
│   │   │   └── MergePdfsTool.cs
│   │   ├── File/
│   │   │   └── QRCodeTool.cs            # ✅ Implemented
│   │   ├── Image/
│   │   │   └── ScreenshotTool.cs
│   │   ├── System/
│   │   │   └── StorageExplorerTool.cs
│   │   ├── TextAndSpeech/
│   │   │   └── TranscribeTool.cs
│   │   ├── Video/
│   │   │   └── CompressVideoTool.cs
│   │   ├── BaseTool.cs                  # Base class for all tools
│   │   └── WindowsToolbox.Tools.csproj
│   └── WindowsToolbox.MCP/
│       ├── Program.cs                   # MCP server implementation
│       └── WindowsToolbox.MCP.csproj
├── tests/                               # Test projects (to be added)
├── .gitignore
├── README.md                            # Main documentation
├── SETUP.md                             # Repository setup instructions
└── WindowsToolbox.sln                   # Visual Studio solution

```

## Project Dependencies

### WindowsToolbox.App (WinUI 3 Application)
- Microsoft.WindowsAppSDK
- Microsoft.Windows.SDK.BuildTools
- CommunityToolkit.Mvvm
- CommunityToolkit.WinUI.UI.Controls
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- WinUIEx
- **References**: WindowsToolbox.Core, WindowsToolbox.Tools

### WindowsToolbox.Core (Class Library)
- CommunityToolkit.Mvvm
- Microsoft.Extensions.DependencyInjection.Abstractions
- Microsoft.Extensions.Logging.Abstractions
- System.Text.Json

### WindowsToolbox.Tools (Class Library)
- FFMpegCore (video/audio processing)
- SixLabors.ImageSharp (image processing)
- itext7 (PDF manipulation)
- QRCoder (QR code generation)
- SharpCompress (archive handling)
- **References**: WindowsToolbox.Core

### WindowsToolbox.MCP (Console Application)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Hosting
- System.Text.Json
- **References**: WindowsToolbox.Core, WindowsToolbox.Tools

## Build Configurations

- **Debug**: Development build with debugging symbols
- **Release**: Optimized production build

## Supported Platforms

- x64 (64-bit Intel/AMD)
- x86 (32-bit Intel/AMD)
- ARM64 (64-bit ARM)

## Key Files

- **WindowsToolbox.sln**: Visual Studio solution file
- **app.manifest**: Windows application manifest for DPI awareness
- **.gitignore**: Comprehensive .NET/Visual Studio ignore rules
- **build.yml**: GitHub Actions workflow for CI/CD

## Tool Organization

Tools are organized by category in separate folders:

1. **Video/** - 15 video processing tools
2. **Audio/** - 5 audio processing tools
3. **Image/** - 13 image processing tools
4. **Document/** - 23 document processing tools
5. **TextAndSpeech/** - 9 text and speech tools
6. **File/** - 5 file utility tools
7. **System/** - 18 Windows system tools
8. **Developer/** - 14 developer utility tools

Each tool:
- Inherits from `BaseTool`
- Implements `ITool` interface
- Lives in its category folder
- Registered in `App.xaml.cs`

## Implementation Status

✅ Implemented: 2 tools
🚧 In Progress: 0 tools
⏳ Planned: 100 tools

Total: 102 tools across 7 categories
