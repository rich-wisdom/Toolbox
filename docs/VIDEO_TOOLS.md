# Video Processing Tools

This document describes the 15 video processing tools implemented for Windows Toolbox.

## Overview

All video tools use **FFMpegCore** library for video processing and **yt-dlp** for video downloads. The tools support:
- Async/await operations
- Progress reporting
- Cancellation support
- Hardware acceleration detection
- Comprehensive error handling

## Tools List

### 1. Download Video
**ID**: `download-video`  
**Description**: Download videos from YouTube, X/Twitter, TikTok, Instagram, Vimeo, etc.  
**Parameters**:
- `url` (string, required): Video URL
- `quality` (string): best, 1080p, 720p, 480p, 360p (default: best)
- `audioOnly` (bool): Extract audio only (default: false)
- `outputDir` (string): Output directory (default: Downloads folder)

**Requirements**: yt-dlp.exe must be placed in `yt-dlp/` folder in application directory

---

### 2. Compress Video
**ID**: `compress-video`  
**Description**: Reduce video file size while maintaining quality  
**Parameters**:
- `inputFile` (string, required): Input video file
- `outputFile` (string): Output file path
- `quality` (string): high, medium, low (default: medium)
- `targetSizeMB` (int): Target file size in MB (optional)

**Features**:
- CRF-based compression (20-28)
- H.264 encoding with AAC audio
- Two-pass encoding support
- Shows size reduction percentage

---

### 3. Convert Video
**ID**: `convert-video`  
**Description**: Convert videos between different formats  
**Parameters**:
- `inputFile` (string, required): Input video file
- `outputFormat` (string): mp4, mov, avi, mkv, webm (default: mp4)
- `videoCodec` (string): h264, h265, vp9, copy (default: copy)
- `audioCodec` (string): aac, mp3, opus, copy (default: copy)

**Supported Formats**: MP4, MOV, AVI, MKV, WebM, M4V, FLV, WMV

---

### 4. Trim Video
**ID**: `trim-video`  
**Description**: Cut video segments by start and end time  
**Parameters**:
- `inputFile` (string, required): Input video file
- `startTime` (TimeSpan): Start time (default: 0)
- `endTime` (TimeSpan): End time (default: video duration)
- `fastMode` (bool): Stream copy vs re-encode (default: true)

**Features**:
- Fast mode: no re-encoding (stream copy)
- Precise mode: frame-accurate trimming

---

### 5. Merge Videos
**ID**: `merge-videos`  
**Description**: Concatenate multiple videos into one  
**Parameters**:
- `inputFiles` (List<string>, required): List of input video files (min 2)
- `outputFile` (string): Output file path

**Features**:
- Automatic resolution/codec matching
- Maintains video quality
- Fast concatenation

---

### 6. Crop & Rotate Video
**ID**: `crop-rotate-video`  
**Description**: Crop and rotate videos  
**Parameters**:
- `inputFile` (string, required): Input video file
- `cropX`, `cropY`, `cropWidth`, `cropHeight` (int): Crop dimensions
- `rotation` (int): 0, 90, 180, 270 degrees
- `flipHorizontal` (bool): Flip horizontally
- `flipVertical` (bool): Flip vertically

**Features**:
- Visual crop selection support
- Multiple transformations in one pass

---

### 7. Resize Video
**ID**: `resize-video`  
**Description**: Change video resolution  
**Parameters**:
- `inputFile` (string, required): Input video file
- `preset` (string): 4K, 1080p, 720p, 480p, 360p
- `customWidth`, `customHeight` (int): Custom dimensions
- `maintainAspectRatio` (bool): Maintain aspect ratio (default: true)
- `scalingAlgorithm` (string): bilinear, bicubic, lanczos (default: lanczos)

**Presets**:
- 4K: 3840x2160
- 1080p: 1920x1080
- 720p: 1280x720
- 480p: 854x480
- 360p: 640x360

---

### 8. Change Speed
**ID**: `change-video-speed`  
**Description**: Speed up or slow down videos  
**Parameters**:
- `inputFile` (string, required): Input video file
- `speed` (double): 0.25x to 4x (default: 1.0)
- `maintainPitch` (bool): Prevent chipmunk effect (default: true)

**Features**:
- Maintains audio pitch
- Supports extreme speeds with filter chaining
- Smooth playback

---

### 9. Mute Video
**ID**: `mute-video`  
**Description**: Remove audio from videos  
**Parameters**:
- `inputFile` (string, required): Input video file

**Features**:
- Fast operation (stream copy)
- Batch processing support
- Removes all audio tracks

---

### 10. Replace Audio
**ID**: `replace-audio`  
**Description**: Replace video audio with a different audio file  
**Parameters**:
- `videoFile` (string, required): Input video file
- `audioFile` (string, required): Input audio file
- `audioOffset` (TimeSpan): Audio sync offset (default: 0)
- `volume` (double): Volume adjustment 0.0-2.0 (default: 1.0)

**Features**:
- Audio synchronization
- Volume control
- Automatic duration matching

---

### 11. Burn In Subtitles
**ID**: `burn-subtitles`  
**Description**: Embed subtitles permanently into video  
**Parameters**:
- `videoFile` (string, required): Input video file
- `subtitleFile` (string, required): Subtitle file (SRT, VTT, ASS)
- `fontSize` (int): Font size (default: 24)
- `fontColor` (string): Font color (default: white)
- `fontName` (string): Font name (default: Arial)

**Supported Formats**: SRT, VTT, ASS

---

### 12. Extract Frames
**ID**: `extract-frames`  
**Description**: Extract images from video  
**Parameters**:
- `inputFile` (string, required): Input video file
- `mode` (string): all, interval, fps (default: interval)
- `interval` (int): Extract every Nth frame (default: 1)
- `fps` (int): Frames per second to extract (default: 1)
- `outputFormat` (string): png, jpg, bmp (default: png)
- `startTime`, `endTime` (TimeSpan): Time range

**Modes**:
- all: Extract all frames
- interval: Extract every Nth frame
- fps: Extract at specific frame rate

---

### 13. Extract Audio
**ID**: `extract-audio`  
**Description**: Extract audio track from video files  
**Parameters**:
- `inputFile` (string, required): Input video file
- `outputFormat` (string): mp3, aac, wav, flac, ogg (default: mp3)
- `bitrate` (int): Bitrate in kbps (default: 192)

**Supported Formats**: MP3, AAC, WAV, FLAC, OGG

---

### 14. Media Info
**ID**: `media-info`  
**Description**: Display detailed information about video and audio files  
**Parameters**:
- `inputFile` (string, required): Input media file
- `exportJson` (bool): Export info as JSON (default: false)

**Information Displayed**:
- File: path, name, size
- Format: container, duration, bitrate
- Video: codec, resolution, frame rate, bitrate, aspect ratio
- Audio: codec, sample rate, channels, bitrate

---

### 15. Video to GIF
**ID**: `video-to-gif`  
**Description**: Convert videos to animated GIF  
**Parameters**:
- `inputFile` (string, required): Input video file
- `startTime` (TimeSpan): Start time for GIF
- `duration` (TimeSpan): Duration of GIF
- `fps` (int): Frames per second (default: 10)
- `width` (int): Width in pixels (default: 480)
- `quality` (int): Quality 1-100 (default: 80)

**Features**:
- High-quality palette generation
- Color optimization
- Infinite loop
- Customizable size and frame rate

---

## FFmpeg Service

The `FFmpegService` class provides core functionality:
- Automatic FFmpeg path detection
- Progress parsing from FFmpeg output
- Hardware acceleration detection (NVENC, QuickSync, AMF)
- Media analysis with FFProbe

## Installation Requirements

1. **FFMpegCore**: Already included via NuGet package
2. **FFmpeg binaries**: Place `ffmpeg.exe` and `ffprobe.exe` in `ffmpeg/` folder in application directory
3. **yt-dlp**: Place `yt-dlp.exe` in `yt-dlp/` folder for Download Video tool

## Usage Example

```csharp
// Example: Compress a video
var tool = new CompressVideoTool();
var parameters = new ToolParameters(new Dictionary<string, object>
{
    ["inputFile"] = "C:\\Videos\\input.mp4",
    ["quality"] = "medium"
});

var progress = new Progress<ToolProgress>(p => 
{
    Console.WriteLine($"{p.Percentage}% - {p.Status}");
});

var result = await tool.ExecuteAsync(parameters, progress);
if (result.Success)
{
    Console.WriteLine(result.Message);
    Console.WriteLine($"Output: {result.Data["outputFile"]}");
}
```

## Error Handling

All tools include comprehensive error handling for:
- Invalid file formats
- Corrupted files
- Insufficient disk space
- FFmpeg errors
- Cancellation requests

## Performance Considerations

- Hardware acceleration is automatically detected where available
- All operations support cancellation via CancellationToken
- Progress reporting provides real-time feedback
- Batch operations are optimized for efficiency

## Future Enhancements

Potential improvements:
- GUI for visual editing (crop, trim)
- Batch processing UI
- Preset management
- Hardware acceleration configuration
- Auto-update for yt-dlp
- Subtitle auto-transcription
- Video stabilization
- Transition effects for merging
