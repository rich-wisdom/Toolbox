# Windows Toolbox MCP Server Setup Guide

The Windows Toolbox MCP (Model Context Protocol) server allows AI assistants like Claude, Cursor, and VS Code Copilot to use Windows Toolbox tools programmatically.

## Features

- **36 Tools** exposed to AI assistants
- **Real-time progress** notifications
- **File path validation** and security
- **Automatic error handling**
- **JSON-RPC 2.0** protocol support

## Installation

### Option 1: Automatic Setup (Recommended)

1. Open Windows Toolbox
2. Go to **Settings** → **AI Integration**
3. Click **"Setup MCP Server"**
4. Select your AI app(s):
   - Cursor
   - Claude Desktop
   - VS Code
5. Click **"Install"**

The app will automatically:
- Detect installed AI apps
- Generate configuration files
- Install them in the correct locations
- Provide restart instructions

### Option 2: Manual Setup

#### For Cursor

1. Open Cursor settings (File → Preferences → Settings)
2. Search for "MCP"
3. Edit the MCP configuration file
4. Add this configuration:

```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "C:\\Program Files\\Windows Toolbox\\WindowsToolbox.MCP.exe",
      "args": []
    }
  }
}
```

#### For Claude Desktop

1. Locate the config file:
   - Windows: `%APPDATA%\Claude\claude_desktop_config.json`
2. Create or edit the file with:

```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "C:\\Program Files\\Windows Toolbox\\WindowsToolbox.MCP.exe",
      "args": []
    }
  }
}
```

#### For VS Code

1. Install an MCP-compatible extension (e.g., Copilot)
2. Open VS Code settings
3. Search for "MCP"
4. Add the configuration similar to Cursor

## Available Tools

### Video Tools (6)
- `compress-video` - Compress video files
- `convert-video` - Convert video formats
- `trim-video` - Trim videos
- `merge-videos` - Merge multiple videos
- `burn-subtitles` - Add subtitles to video
- `download-video` - Download videos from URLs

### Audio Tools (3)
- `clean-audio` - Remove background noise
- `trim-audio` - Trim audio files
- `convert-audio` - Convert audio formats

### Image Tools (7)
- `compress-images` - Compress images
- `resize-images` - Resize images
- `remove-background` - Remove image backgrounds
- `beautify-screenshot` - Beautify screenshots
- `convert-images` - Convert image formats
- `blur-faces` - Blur faces in images
- `watermark` - Add watermarks

### Document Tools (8)
- `merge-pdfs` - Merge PDF files
- `compress-pdf` - Compress PDFs
- `sign-pdf` - Sign PDFs
- `split-pdf` - Split PDFs
- `pdf-to-word` - Convert PDF to Word
- `pdf-to-images` - Convert PDF to images
- `ocr-pdf` - Add OCR layer to PDFs
- `redact-pdf` - Redact PDF content

### File Tools (3)
- `batch-rename` - Rename multiple files
- `generate-qr` - Generate QR codes
- `zip-files` - Create ZIP archives

### Text & Speech Tools (5)
- `transcribe` - Transcribe audio/video
- `translate` - Translate text/documents
- `extract-text` - Extract text from images/PDFs
- `summarize` - Summarize text
- `ocr-image` - OCR on images

### Developer Tools (4)
- `format-json` - Format JSON
- `encode-base64` - Encode to Base64
- `decode-base64` - Decode Base64
- `generate-hash` - Generate hashes (MD5, SHA1, SHA256, SHA512)

## Example Prompts

### Video Processing
```
"Compress the video on my Desktop to reduce its size"
"Convert video.mp4 to WebM format"
"Trim the first 30 seconds from my recording"
"Merge all MP4 files in the Videos folder"
```

### Document Processing
```
"Merge all PDFs in the Documents folder"
"Compress this PDF to reduce its size"
"Extract text from this scanned PDF"
"Split this PDF into separate pages"
```

### Image Processing
```
"Compress all images in the Screenshots folder"
"Remove the background from logo.png"
"Resize all images to 1920x1080"
"Add my watermark to all product photos"
```

### Text & Developer Tools
```
"Transcribe this video file"
"Translate this document to Spanish"
"Generate SHA256 hash of this file"
"Format this JSON data"
```

## Troubleshooting

### Server Not Responding

1. Check if Windows Toolbox is installed:
   ```
   C:\Program Files\Windows Toolbox\WindowsToolbox.MCP.exe
   ```

2. Test the server manually:
   ```cmd
   "C:\Program Files\Windows Toolbox\WindowsToolbox.MCP.exe"
   ```

3. Check the log files:
   ```
   %LOCALAPPDATA%\WindowsToolbox\Logs\
   ```

### Tools Not Found

1. Restart your AI app after configuration
2. Verify the config file is in the correct location
3. Check that the executable path is correct
4. Ensure Windows Toolbox is up to date

### Permission Errors

1. Run your AI app as Administrator (if needed)
2. Check file permissions on:
   - Installation directory
   - Config file location
   - Tool output directories

### File Path Issues

- Always use **absolute paths** (e.g., `C:\Users\...\file.mp4`)
- Escape backslashes in JSON: `C:\\Users\\...\\file.mp4`
- Use forward slashes when possible: `C:/Users/.../file.mp4`

## Security

The MCP server includes security features:
- **File path validation** - Prevents directory traversal
- **Sandboxed execution** - Tools run in isolated processes
- **Rate limiting** - Prevents abuse
- **Local-only** - Server only accepts local connections

## Updates

The MCP server updates automatically with Windows Toolbox updates. Check for updates:
1. Open Windows Toolbox
2. Go to Settings → About
3. Click "Check for Updates"

## Support

- **Documentation**: https://github.com/rich-wisdom/Toolbox/docs
- **Issues**: https://github.com/rich-wisdom/Toolbox/issues
- **Email**: support@windowstoolbox.com

## Advanced Configuration

### Environment Variables

You can customize behavior with environment variables:

```json
{
  "mcpServers": {
    "windows-toolbox": {
      "command": "C:\\Program Files\\Windows Toolbox\\WindowsToolbox.MCP.exe",
      "args": [],
      "env": {
        "TOOLBOX_OUTPUT_DIR": "C:\\MyOutputs",
        "TOOLBOX_LOG_LEVEL": "debug"
      }
    }
  }
}
```

### Output Directory

By default, tool outputs go to:
```
C:\Users\<YourName>\Downloads\Toolbox\<YYYY-MM-DD_HH-MM-SS>\
```

To change this, set `TOOLBOX_OUTPUT_DIR` environment variable.

## Protocol Details

The server implements MCP (Model Context Protocol) with:
- **Protocol Version**: 2024-11-05
- **Transport**: stdio (standard input/output)
- **Message Format**: JSON-RPC 2.0
- **Progress**: Real-time progress notifications
- **Error Handling**: Detailed error messages with codes

For more technical details, see the [MCP Protocol Documentation](https://modelcontextprotocol.io).
