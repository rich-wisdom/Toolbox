# Architecture Documentation

## Overview

Windows Toolbox is built using a modular, plugin-based architecture that makes it easy to add new tools while maintaining clean separation of concerns.

## Project Structure

### WindowsToolbox.App
The main WinUI 3 application containing:
- **Views/**: XAML pages and windows (MainWindow, QuickLauncherWindow, etc.)
- **ViewModels/**: MVVM view models using CommunityToolkit.Mvvm
- **Services/**: Application services (GlobalHotKeyService, etc.)

### WindowsToolbox.Core
Core interfaces and services shared across all projects:
- **Interfaces/**: ITool and related interfaces
- **Models/**: Data models (ToolCategory, etc.)
- **Services/**: Core services (IToolRegistry, IRecentToolsService, ISettingsService)

### WindowsToolbox.Tools
Tool implementations organized by category:
- **Video/**: Video processing tools (15 tools)
- **Audio/**: Audio processing tools (5 tools)
- **Image/**: Image processing tools (13 tools)
- **Document/**: Document processing tools (23 tools)
- **TextAndSpeech/**: OCR, transcription, translation (9 tools)
- **File/**: File utilities (5 tools)
- **System/**: Windows system tools (18 tools)
- **Developer/**: Developer utilities (14 tools)

### WindowsToolbox.MCP
MCP (Model Context Protocol) server for AI agent integration.

## Key Design Patterns

### MVVM (Model-View-ViewModel)
- Views are XAML-based WinUI 3 pages
- ViewModels use CommunityToolkit.Mvvm for property change notification
- Models are in WindowsToolbox.Core.Models

### Dependency Injection
- Microsoft.Extensions.DependencyInjection for IoC
- Services registered in App.xaml.cs
- ViewModels and services are injected into views

### Plugin Architecture
- All tools implement ITool interface
- Tools are discovered and registered via IToolRegistry
- New tools can be added by simply implementing ITool

### Repository Pattern
- IToolRegistry acts as a repository for tools
- Provides search, filtering, and lookup capabilities

## Tool Lifecycle

1. **Registration**: Tools are registered in App.xaml.cs on startup
2. **Discovery**: IToolRegistry manages all registered tools
3. **Search**: Tools can be searched by name, description, or category
4. **Execution**: Tools execute asynchronously with progress reporting
5. **Results**: Tools return ToolResult with success/failure and data

## Quick Launcher System

The quick launcher uses:
- Global hotkey registration (Win32 API via P/Invoke)
- Fuzzy search for tool discovery
- Recent tools tracking for quick access
- Keyboard navigation for efficiency

## Settings Management

Settings are persisted using:
- JSON serialization
- File-based storage in LocalApplicationData
- ISettingsService interface for abstraction

## MCP Integration

The MCP server:
- Runs as a separate console application
- Communicates via stdio using JSON-RPC
- Exposes tools to AI agents (Claude, Cursor, VS Code)
- Supports tool discovery and execution
- Reports progress and results back to agents

## Future Enhancements

- Background job queue for long-running operations
- Notification system for completed jobs
- Plugin discovery from external assemblies
- Cloud sync for settings and recent tools
- Telemetry and crash reporting
