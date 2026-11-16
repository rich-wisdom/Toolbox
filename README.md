# Windows Toolbox

A native Windows application with 102 professional tools for video, audio, image, document processing, and more. Built with WinUI 3 and .NET 8.

## Features

- **102 Professional Tools** across 7 categories
- **Quick Launcher** - Press `Ctrl+Alt+Space` anywhere to search and launch tools
- **Offline-First** - All processing happens locally on your machine
- **MCP Integration** - Expose tools to AI agents (Claude, Cursor, VS Code)
- **Modern UI** - Native Windows 11 design with WinUI 3
- **MVVM Architecture** - Clean, maintainable code structure

## Tool Categories

- **Video Tools (15)** - Download, compress, convert, trim, merge, and edit videos
- **Audio Tools (5)** - Clean, record, trim, extract, and convert audio
- **Image Tools (13)** - Screenshot, beautify, annotate, remove backgrounds, and more
- **Document Tools (23)** - Scan, merge, sign, compress, and convert PDFs and documents
- **Text & Speech Tools (9)** - Transcribe, translate, summarize, OCR, and text utilities
- **File Tools (5)** - QR codes, batch rename, find duplicates, zip/unzip
- **System Tools (18)** - Storage explorer, battery health, network monitoring, and more
- **Developer Tools (14)** - JSON formatter, Base64, hashing, UUID, JWT, and more

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
git clone https://github.com/rich-wisdom/windows-toolbox.git
cd windows-toolbox
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
# Restore packages
dotnet restore

# Build the solution
dotnet build WindowsToolbox.sln -c Release

# Run the app
dotnet run --project src/WindowsToolbox.App/WindowsToolbox.App.csproj
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

## Development Roadmap

### Phase 1: Foundation ✅
- [x] Project structure
- [x] Main window and navigation
- [x] Quick launcher system
- [x] Tool architecture
- [x] Settings infrastructure

### Phase 2: Core Tools (In Progress)
- [ ] Developer tools (14 tools)
- [ ] File tools (5 tools)

### Phase 3-8: Tool Implementation
- [ ] Document tools (23 tools)
- [ ] Image tools (13 tools)
- [ ] Video tools (15 tools)
- [ ] Audio tools (5 tools)
- [ ] Text & Speech tools (9 tools)
- [ ] System tools (18 tools)

### Phase 9: MCP Server
- [ ] Complete MCP protocol implementation
- [ ] Tool discovery and invocation
- [ ] Progress streaming
- [ ] Testing with AI agents

### Phase 10: Polish
- [ ] UI refinement
- [ ] Performance optimization
- [ ] Comprehensive testing
- [ ] Documentation

### Phase 11: Distribution
- [ ] MSIX installer
- [ ] Auto-update system
- [ ] Licensing (freemium model)

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

[License TBD]

## Credits

Inspired by Mac Toolbox, reimagined for Windows with AI integration.
