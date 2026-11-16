# Windows Toolbox

An early-stage native Windows utility application built with WinUI 3 and .NET 8.

> The repository contains a large candidate tool catalog, but it has not been verified as 102 functional tools. The current build registers 37 developer, file, and system candidates. Media, document, image, and text/speech sources are retained but excluded from compilation because their merged implementations still have unresolved API and platform-integration errors.

## Features

- **37 registered candidate tools** across developer, file, and system categories
- **Quick Launcher** - Press `Ctrl+Alt+Space` anywhere to search and launch tools
- **Local processing where implemented** - Some media features require FFmpeg, yt-dlp, Whisper models, or other local dependencies
- **MCP Integration** - Expose tools to AI agents (Claude, Cursor, VS Code)
- **Modern UI** - Native Windows 11 design with WinUI 3
- **MVVM Architecture** - Clean, maintainable code structure

## Tool Categories

The source catalog covers video, audio, image, document, text/speech, file, system, and developer utilities. Only the 37 currently registered candidates are compiled into the app, and their presence is not a claim of end-to-end verification. The experimental MCP server advertises seven developer/file candidates.

## Requirements

- Windows 10 version 1809 (build 17763) or later
- Windows 11 recommended
- .NET 8 Runtime
- Visual Studio 2022 (for building)

## Building from Source

### Prerequisites

1. Install [Visual Studio 2022](https://visualstudio.microsoft.com/) with:
   - .NET Desktop Development workload
   - Windows App SDK C# Templates
   - Windows 11 SDK (10.0.22621.0 or later)

2. Clone the repository:
```bash
git clone https://github.com/rich-wisdom/Toolbox.git
cd Toolbox
```

3. Open `WindowsToolbox.sln` in Visual Studio 2022

4. Restore NuGet packages:
   - Right-click solution → Restore NuGet Packages

5. Build the solution:
   - Select `Debug` or `Release` configuration
   - Choose platform: `x64`, `x86`, or `ARM64`
   - Press F5 to build and run

### Build from Command Line

```powershell
# Build the unpackaged x64 app
dotnet restore WindowsToolbox.sln
dotnet build src/WindowsToolbox.App/WindowsToolbox.App.csproj -c Release -p:Platform=x64
```

The WinUI application and MSIX packaging project require Windows. They are not expected to build on Linux or macOS.

### Build an MSIX

From Windows PowerShell with Visual Studio 2022, the Windows Application Packaging workload, and the Windows 10/11 SDK installed:

```powershell
.\build\Build-Installer.ps1 -Configuration Release -Platform x64
```

This creates an **unsigned** development MSIX in `dist\`. Sign it with a certificate trusted by the target machine before installation:

```powershell
.\build\Sign-Package.ps1 -PackagePath .\dist\<package>.msix -CertificatePath .\certificate.pfx
Add-AppxPackage .\dist\<signed-package>.msix
```

## Project Structure

```
windows-toolbox/
├── src/
│   ├── WindowsToolbox.App/          # Main WinUI 3 application
│   │   ├── Views/                   # XAML pages and windows
│   │   ├── ViewModels/              # MVVM view models
│   │   └── Services/                # App-specific services
│   ├── WindowsToolbox.Core/         # Core interfaces and services
│   │   ├── Interfaces/              # ITool, abstractions
│   │   ├── Models/                  # Data models
│   │   └── Services/                # Tool registry, settings
│   ├── WindowsToolbox.Tools/        # Tool implementations
│   │   ├── Video/                   # Video processing tools
│   │   ├── Audio/                   # Audio processing tools
│   │   ├── Image/                   # Image processing tools
│   │   ├── Document/                # Document processing tools
│   │   ├── TextAndSpeech/           # OCR, transcription, etc.
│   │   ├── File/                    # File utilities
│   │   ├── System/                  # System utilities
│   │   └── Developer/               # Developer tools
│   └── WindowsToolbox.MCP/          # MCP server for AI integration
├── tests/
├── docs/
└── WindowsToolbox.sln               # Visual Studio solution
```

## Architecture

### Tool System

All tools implement the `ITool` interface:

```csharp
public interface ITool
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Category { get; }
    bool IsAvailable { get; }
    bool IsPremium { get; }
    
    Task<ToolResult> ExecuteAsync(ToolParameters parameters, 
                                   IProgress<ToolProgress>? progress = null, 
                                   CancellationToken cancellationToken = default);
}
```

### Adding New Tools

1. Create a new class in the appropriate category folder under `WindowsToolbox.Tools`
2. Inherit from `BaseTool`
3. Implement the required properties and `ExecuteAsync` method
4. Register the tool in `App.xaml.cs`

Example:

```csharp
public class MyNewTool : BaseTool
{
    public override string Id => "my-new-tool";
    public override string Name => "My New Tool";
    public override string Description => "Does something awesome";
    public override string Category => "developer";

    public override async Task<ToolResult> ExecuteAsync(
        ToolParameters parameters, 
        IProgress<ToolProgress>? progress = null, 
        CancellationToken cancellationToken = default)
    {
        // Implementation here
        return Success("Operation completed");
    }
}
```

## MCP Integration

The Windows Toolbox includes an MCP (Model Context Protocol) server that exposes tools to AI agents like Claude, Cursor, and VS Code.

### Running the MCP Server

```powershell
cd src/WindowsToolbox.MCP
dotnet run
```

### Configuring for Cursor

Add to your Cursor settings:

```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "dotnet",
      "args": ["run", "--project", "path/to/WindowsToolbox.MCP/WindowsToolbox.MCP.csproj"],
      "env": {}
    }
  }
}
```

## Dependencies

Key dependencies used:

- **Microsoft.WindowsAppSDK** - WinUI 3 framework
- **CommunityToolkit.Mvvm** - MVVM helpers
- **FFMpegCore** - Video/audio processing
- **SixLabors.ImageSharp** - Image processing
- **itext7** - PDF manipulation
- **QRCoder** - QR code generation
- **SharpCompress** - Archive handling

## Verification status

The integrated source has been statically audited and is built by Windows CI. Functional, end-to-end verification of every catalog entry is still required. In particular, tools that invoke FFmpeg, yt-dlp, Tesseract, Whisper, Office/LibreOffice, Windows APIs, or hardware-specific features need dependency-specific tests on Windows.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[License TBD]

## Credits

Inspired by Mac Toolbox, reimagined for Windows with AI integration.
