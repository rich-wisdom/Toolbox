# Windows Toolbox MCP Server

Model Context Protocol (MCP) server that exposes 36 Windows Toolbox tools to AI assistants.

## Architecture

```
WindowsToolbox.MCP.exe (Console App)
├── McpProtocolHandler (JSON-RPC 2.0 over stdio)
├── ToolExecutor (Executes tools)
├── ToolSchemaProvider (36 tool schemas)
└── AIAppSetup (Auto-configuration utility)
```

## Features

- **36 Tools** across 6 categories
- **JSON-RPC 2.0** protocol
- **stdio transport** (standard input/output)
- **Progress notifications** for long-running operations
- **Error handling** with detailed messages
- **Auto-setup** for AI apps (Cursor, Claude Desktop, VS Code)

## Building

```bash
cd src/WindowsToolbox.MCP
dotnet build
```

## Running

```bash
dotnet run --project src/WindowsToolbox.MCP
```

Or directly:
```bash
WindowsToolbox.MCP.exe
```

## Testing

### Manual Test

1. Run the server:
   ```bash
   dotnet run --project src/WindowsToolbox.MCP
   ```

2. Send an initialize request:
   ```json
   {"jsonrpc":"2.0","method":"initialize","params":{},"id":1}
   ```

3. Send a tools/list request:
   ```json
   {"jsonrpc":"2.0","method":"tools/list","params":{},"id":2}
   ```

4. Call a tool:
   ```json
   {"jsonrpc":"2.0","method":"tools/call","params":{"name":"format-json","arguments":{"json_string":"{\"test\":true}"}},"id":3}
   ```

### PowerShell Test Script

```powershell
$mcpExe = ".\WindowsToolbox.MCP.exe"

# Test 1: Initialize
$init = '{"jsonrpc":"2.0","method":"initialize","params":{},"id":1}'
echo $init | & $mcpExe

# Test 2: List tools
$list = '{"jsonrpc":"2.0","method":"tools/list","params":{},"id":2}'
echo $list | & $mcpExe

# Test 3: Call tool
$call = '{"jsonrpc":"2.0","method":"tools/call","params":{"name":"format-json","arguments":{"json_string":"{\"test\":true}"}},"id":3}'
echo $call | & $mcpExe
```

## Logging

Logs are written to:
```
%LOCALAPPDATA%\WindowsToolbox\Logs\mcp_YYYYMMDD_HHMMSS.log
```

## Integration

See [MCP_SETUP.md](../../docs/MCP_SETUP.md) for integration instructions with:
- Cursor
- Claude Desktop
- VS Code

## Tool Categories

1. **Video** (6 tools) - Compress, convert, trim, merge, subtitles, download
2. **Audio** (3 tools) - Clean, trim, convert
3. **Image** (7 tools) - Compress, resize, background removal, beautify, convert, blur faces, watermark
4. **Document** (8 tools) - PDF operations (merge, compress, sign, split, convert, OCR, redact)
5. **File** (3 tools) - Batch rename, QR codes, ZIP
6. **Text & Speech** (5 tools) - Transcribe, translate, extract text, summarize, OCR
7. **Developer** (4 tools) - JSON format, Base64 encode/decode, hash generation

## Protocol Compliance

Implements MCP specification version 2024-11-05:
- ✅ initialize/initialized handshake
- ✅ tools/list - Tool discovery
- ✅ tools/call - Tool invocation
- ✅ Progress notifications
- ✅ Error handling (JSON-RPC error codes)
- ✅ Graceful shutdown on stdin close

## Security

- File path validation (prevents directory traversal)
- Input sanitization
- Tool execution timeout
- Rate limiting
- Local-only communication (stdio)

## Contributing

1. Add new tool to `WindowsToolbox.Tools`
2. Add tool schema to `ToolSchemaProvider.cs`
3. Register tool in `Program.GetAllTools()`
4. Update documentation
5. Test with MCP client

## License

See LICENSE file in repository root.
