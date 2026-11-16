# Example Prompts for Windows Toolbox MCP

Use these prompts with your AI assistant (Cursor, Claude Desktop, VS Code) to leverage Windows Toolbox tools.

## Video Processing

### Compress Videos
```
"Compress the video file C:\Users\Me\Videos\presentation.mp4 to reduce its size"
"Make this video smaller: C:\Users\Me\Desktop\recording.mp4 with high quality"
"Compress all videos in C:\Users\Me\Videos to medium quality"
```

### Convert Videos
```
"Convert video.mp4 to WebM format"
"Change this video to MOV format: C:\Users\Me\Videos\demo.mp4"
"Convert my AVI files to MP4"
```

### Trim Videos
```
"Trim the first 30 seconds from C:\Users\Me\Videos\interview.mp4"
"Cut the video from 1:30 to 3:45"
"Remove the last minute from this video"
```

### Merge Videos
```
"Merge these three videos into one: clip1.mp4, clip2.mp4, clip3.mp4"
"Combine all MP4 files in the Videos folder"
"Join these video files in order"
```

### Add Subtitles
```
"Burn subtitles from subtitles.srt into video.mp4"
"Add subtitles to this video by auto-transcribing it"
"Embed English subtitles into my presentation video"
```

### Download Videos
```
"Download this YouTube video: https://youtube.com/watch?v=..."
"Download video at 1080p from this URL"
"Get the audio only from this video URL"
```

## Audio Processing

### Clean Audio
```
"Remove background noise from C:\Users\Me\Audio\podcast.mp3"
"Clean up this audio recording with medium noise reduction"
"Enhance the audio quality of my voice recording"
```

### Trim Audio
```
"Trim the audio from 0:30 to 2:15"
"Cut the first 10 seconds from this audio file"
"Extract the middle portion of the song"
```

### Convert Audio
```
"Convert this WAV file to MP3"
"Change the audio format to FLAC"
"Convert all my audio files to AAC"
```

## Image Processing

### Compress Images
```
"Compress all images in C:\Users\Me\Pictures\Photos at 85% quality"
"Reduce the size of these image files"
"Compress screenshot.png to make it smaller"
```

### Resize Images
```
"Resize all images to 1920x1080"
"Scale down these images to 50%"
"Make this image 800 pixels wide"
```

### Remove Background
```
"Remove the background from logo.png"
"Make the background transparent in this image"
"Extract the subject from this photo"
```

### Beautify Screenshots
```
"Beautify this screenshot with a gradient background and shadow"
"Make this screenshot look professional with rounded corners"
"Add padding and shadow to my app screenshot"
```

### Convert Images
```
"Convert all PNG files to JPG"
"Change this BMP to WebP format"
"Convert images to PNG format"
```

### Blur Faces
```
"Blur all faces in this photo"
"Pixelate faces in the group photo"
"Anonymize people in this image"
```

### Watermark
```
"Add 'Copyright 2026' watermark to all images in this folder"
"Watermark my photos with my logo in the bottom right"
"Add watermark to product images"
```

## Document Processing

### Merge PDFs
```
"Merge all PDFs in C:\Users\Me\Documents into one file"
"Combine these three PDF files: report1.pdf, report2.pdf, report3.pdf"
"Create a single PDF from multiple documents"
```

### Compress PDFs
```
"Compress this large PDF to reduce file size"
"Make this PDF smaller with high quality"
"Reduce the size of presentation.pdf"
```

### Sign PDFs
```
"Add my signature to this PDF at the bottom"
"Sign the contract PDF with my signature image"
"Add signature to last page of agreement.pdf"
```

### Split PDFs
```
"Split this PDF into separate pages"
"Extract pages 1-5 from document.pdf"
"Divide the PDF into multiple files"
```

### Convert PDFs
```
"Convert this PDF to a Word document"
"Turn PDF pages into PNG images"
"Extract all pages as JPG files"
```

### OCR PDFs
```
"Add text layer to this scanned PDF"
"Make this PDF searchable with OCR"
"Extract text from scanned document"
```

### Redact PDFs
```
"Redact all social security numbers from this PDF"
"Remove sensitive information from the document"
"Black out confidential data in the PDF"
```

## File Operations

### Batch Rename
```
"Rename all files in this folder by adding 'backup_' prefix"
"Replace 'draft' with 'final' in all filenames"
"Number all files in sequence"
```

### Generate QR Codes
```
"Create a QR code for this URL: https://example.com"
"Generate QR code for WiFi password"
"Make a QR code with this text: 'Contact: john@example.com'"
```

### Create ZIP Archives
```
"ZIP all files in the Documents folder"
"Create a password-protected ZIP of these files"
"Compress these folders into a ZIP archive"
```

## Text & Speech

### Transcribe
```
"Transcribe this audio file to text"
"Convert speech to text from meeting.mp4"
"Generate SRT subtitles from the video"
```

### Translate
```
"Translate this text to Spanish"
"Convert this document to French"
"Translate the contents to German"
```

### Extract Text
```
"Extract all text from this image"
"Get the text from this scanned document"
"OCR this screenshot and give me the text"
```

### Summarize
```
"Summarize this long document"
"Give me a brief summary of this text"
"Create a detailed summary of the article"
```

## Developer Tools

### Format JSON
```
"Format this JSON data: {\"name\":\"test\",\"value\":123}"
"Pretty print this JSON"
"Beautify this JSON string"
```

### Base64 Encoding/Decoding
```
"Encode this text to Base64: Hello World"
"Convert this file to Base64"
"Decode this Base64 string"
```

### Generate Hashes
```
"Generate SHA256 hash of this file"
"Calculate MD5 and SHA1 hashes for this text"
"Get the hash values for this document"
```

## Complex Workflows

### Video Production
```
"Process my video: trim first 10 seconds, compress to 50MB, and add subtitles"
"Download video from URL, convert to MP4, and compress"
```

### Image Batch Processing
```
"Resize all images in Photos folder to 1920x1080, compress to 85%, and watermark with my logo"
"Compress, convert to JPG, and add watermark to all screenshots"
```

### Document Workflow
```
"Merge all PDFs in folder, compress the result, and add OCR"
"Split the PDF, compress each page, and convert to images"
```

### Content Creation
```
"Transcribe the video, translate to Spanish, and create subtitles"
"Extract audio from video, clean it up, and transcribe to text"
```

## Tips for Better Prompts

1. **Use absolute paths**: `C:\Users\Me\file.mp4` instead of "file.mp4"
2. **Be specific**: Mention quality levels (high/medium/low)
3. **Specify output format**: "Convert to MP4" instead of "convert this video"
4. **Batch operations**: "All files in folder" for multiple files
5. **Chain operations**: Combine multiple tools in one request

## Common Patterns

### Pattern: Process and Save
```
"Compress the video and save it with '_compressed' suffix"
```

### Pattern: Batch with Filter
```
"Compress all PNG files over 1MB in the folder"
```

### Pattern: Multi-step
```
"First merge these PDFs, then compress the result to under 10MB"
```

### Pattern: Quality Control
```
"Compress the video but keep quality above 720p"
```
