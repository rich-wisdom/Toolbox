# MCP Server Troubleshooting Guide

Common issues and solutions for Windows Toolbox MCP Server.

## Installation Issues

### Server Executable Not Found

**Symptom**: AI app reports "command not found" or executable path error

**Solutions**:
1. Verify installation path:
   ```
   C:\Program Files\Windows Toolbox\WindowsToolbox.MCP.exe
   ```
2. Check if file exists at that location
3. Reinstall Windows Toolbox if missing
4. Update config file with correct path

### Configuration File Not Loaded

**Symptom**: Tools not appearing in AI assistant

**Solutions**:
1. **For Cursor**: Check `%APPDATA%\Cursor\User\mcp_settings.json`
2. **For Claude Desktop**: Check `%APPDATA%\Claude\claude_desktop_config.json`
3. **For VS Code**: Check `%APPDATA%\Code\User\mcp_settings.json`
4. Verify JSON syntax is valid
5. Restart the AI application

## Runtime Issues

### Server Not Responding

**Symptom**: Tools list empty or calls timeout

**Diagnosis**:
```powershell
# Test server manually
& "C:\Program Files\Windows Toolbox\WindowsToolbox.MCP.exe"

# Send test request
'{"jsonrpc":"2.0","method":"initialize","params":{},"id":1}' | & "C:\Program Files\Windows Toolbox\WindowsToolbox.MCP.exe"
```

**Solutions**:
1. Check server logs:
   ```
   %LOCALAPPDATA%\WindowsToolbox\Logs\mcp_*.log
   ```
2. Verify no antivirus blocking
3. Run AI app as Administrator
4. Check Windows Event Viewer for errors

### Tools Not Executing

**Symptom**: Tool calls return errors

**Common Error Messages**:

#### "File not found"
- Use **absolute paths**: `C:\Users\...\file.mp4`
- Escape backslashes in JSON: `C:\\Users\\...`
- Or use forward slashes: `C:/Users/.../file.mp4`
- Verify file exists before calling

#### "Permission denied"
- Run AI app as Administrator
- Check file/folder permissions
- Ensure file is not locked by another app
- Check antivirus isn't blocking

#### "Tool not found"
- Use exact tool name from list
- Tool names use kebab-case: `compress-video` not `CompressVideo`
- Check tool list: `{"method":"tools/list"}`

#### "Invalid parameters"
- Check required parameters in tool schema
- Verify parameter types (string, number, array)
- Use correct enum values for options

## Progress Issues

### No Progress Updates

**Symptom**: Long operations appear frozen

**Explanation**: Progress notifications are sent via separate channel

**Solution**: Check if your AI app supports progress notifications
- Most modern MCP clients display progress
- Operations still complete even without visual progress

## File Path Issues

### Windows Path Separators

**Problem**: Backslashes in paths

**Solutions**:
```json
// Option 1: Escape backslashes
{"file_path": "C:\\Users\\Me\\video.mp4"}

// Option 2: Use forward slashes
{"file_path": "C:/Users/Me/video.mp4"}

// Option 3: Raw string (if supported)
{"file_path": r"C:\Users\Me\video.mp4"}
```

### Relative Paths

**Problem**: Relative paths not resolving

**Solution**: Always use absolute paths
```
❌ "video.mp4"
❌ ".\Downloads\video.mp4"
✅ "C:\Users\Me\Downloads\video.mp4"
```

### Special Characters

**Problem**: Spaces or special characters in paths

**Solution**: Paths with spaces work fine, just use quotes
```json
{"file_path": "C:/Users/John Doe/My Videos/video.mp4"}
```

## Performance Issues

### Slow Tool Execution

**Causes**:
- Large file processing (expected)
- Insufficient disk space
- Antivirus scanning
- Background processes

**Solutions**:
1. Free up disk space
2. Temporarily disable antivirus for testing
3. Close unnecessary applications
4. Check Task Manager for resource usage

### Memory Issues

**Symptom**: High memory usage or crashes

**Solutions**:
1. Process files in smaller batches
2. Close other applications
3. Increase system RAM
4. Use quality/size parameters to reduce processing load

## Connection Issues

### Server Keeps Restarting

**Symptom**: Server logs show repeated starts

**Causes**:
- Server crashing
- AI app repeatedly retrying
- Configuration error

**Solutions**:
1. Check server logs for crash reason
2. Verify configuration JSON is valid
3. Update Windows Toolbox to latest version
4. Report bug if persistent

### Timeout Errors

**Symptom**: Operations timeout before completion

**Solutions**:
1. Increase timeout in AI app settings (if available)
2. Use smaller files for testing
3. Check network/disk I/O speed
4. Process files locally, not on network drives

## Logging and Diagnostics

### Enable Debug Logging

Set environment variable in config:
```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "C:\\Program Files\\Windows Toolbox\\WindowsToolbox.MCP.exe",
      "args": [],
      "env": {
        "TOOLBOX_LOG_LEVEL": "debug"
      }
    }
  }
}
```

### Log Locations

- **MCP Server**: `%LOCALAPPDATA%\WindowsToolbox\Logs\mcp_*.log`
- **Tool Output**: `C:\Users\<You>\Downloads\Toolbox\<timestamp>\`
- **Windows Events**: Event Viewer → Windows Logs → Application

### Collect Diagnostics

For bug reports, collect:
1. MCP server logs
2. AI app logs (if available)
3. Steps to reproduce
4. Windows version
5. Windows Toolbox version

## Common Error Codes

### JSON-RPC Error Codes

- `-32700`: Parse error (invalid JSON)
- `-32600`: Invalid request
- `-32601`: Method not found
- `-32602`: Invalid params
- `-32603`: Internal error

### Tool-Specific Errors

Check `isError` field in response:
```json
{
  "content": [{"type": "text", "text": "Error message here"}],
  "isError": true
}
```

## Security Issues

### Antivirus Blocking

**Symptom**: Operations fail with cryptic errors

**Solutions**:
1. Add exception for:
   - `WindowsToolbox.MCP.exe`
   - Output directory: `%USERPROFILE%\Downloads\Toolbox\`
2. Temporarily disable for testing
3. Report false positive to antivirus vendor

### Firewall Issues

**Note**: MCP uses stdio, not network, so firewall shouldn't affect it

If network operations fail (e.g., download-video):
1. Check Windows Firewall settings
2. Allow Windows Toolbox through firewall

## Getting Help

### Self-Service

1. Check logs: `%LOCALAPPDATA%\WindowsToolbox\Logs\`
2. Review [MCP_SETUP.md](MCP_SETUP.md)
3. Try example prompts: [EXAMPLE_PROMPTS.md](EXAMPLE_PROMPTS.md)
4. Test with PowerShell script: `docs/examples/test_mcp_server.ps1`

### Community Support

- **GitHub Issues**: https://github.com/rich-wisdom/Toolbox/issues
- **Discussions**: https://github.com/rich-wisdom/Toolbox/discussions

### Bug Reports

Include:
1. Windows version
2. Windows Toolbox version
3. AI app and version
4. MCP server logs
5. Steps to reproduce
6. Expected vs actual behavior

### Feature Requests

Submit via GitHub Issues with:
- Use case description
- Proposed tool name and parameters
- Example prompts
- Similar tools in other apps

## Version Compatibility

### Minimum Versions

- **Windows**: Windows 10 or later
- **Windows Toolbox**: 1.0.0+
- **.NET Runtime**: 8.0+
- **Cursor**: Latest version
- **Claude Desktop**: 0.7.0+
- **VS Code**: 1.85.0+ (with MCP extension)

### Known Incompatibilities

- Windows 7/8: Not supported
- ARM Windows: Experimental support
- WSL: Not supported (use Windows native)

## Advanced Troubleshooting

### Process Monitor

Use Process Monitor to debug:
1. Download [Process Monitor](https://learn.microsoft.com/en-us/sysinternals/downloads/procmon)
2. Filter by `WindowsToolbox.MCP.exe`
3. Look for failed operations

### Event Tracing

Enable Windows Event Tracing:
```powershell
wevtutil set-log Application /enabled:true
```

View events:
```powershell
Get-EventLog -LogName Application -Source "Windows Toolbox" -Newest 10
```

### Network Trace

For download-video issues:
```powershell
netsh trace start capture=yes
# Run the operation
netsh trace stop
```

## FAQ

**Q: Can I run MCP server standalone?**
A: Yes, for testing. In production, let AI apps manage it.

**Q: Can multiple AI apps use the same server?**
A: Yes, each app launches its own instance.

**Q: How do I update the MCP server?**
A: Update Windows Toolbox. MCP server updates automatically.

**Q: Can I customize tool behavior?**
A: Not directly. Tools use Windows Toolbox defaults. Configure in main app.

**Q: Does it work offline?**
A: Yes, except download-video and cloud-dependent tools.

**Q: Is my data sent to the cloud?**
A: No. All processing is local. Only exceptions:
  - download-video (downloads from internet)
  - AI-powered features (if enabled in main app)

## Still Having Issues?

If you've tried everything:
1. Uninstall and reinstall Windows Toolbox
2. Delete config files and reconfigure
3. Check for Windows updates
4. Contact support: support@windowstoolbox.com
