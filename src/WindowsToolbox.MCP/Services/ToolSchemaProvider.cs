using WindowsToolbox.MCP.Models;

namespace WindowsToolbox.MCP.Services;

public class ToolSchemaProvider
{
    public static List<McpTool> GetAllToolSchemas()
    {
        return new List<McpTool>
        {
            // Video Tools (6)
            new McpTool
            {
                Name = "compress-video",
                Description = "Compress video files to reduce size while maintaining quality",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the video file" },
                        ["quality"] = new SchemaProperty { Type = "string", Description = "Compression quality level", Enum = new List<string> { "high", "medium", "low" } },
                        ["target_size_mb"] = new SchemaProperty { Type = "number", Description = "Target file size in MB (optional)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "convert-video",
                Description = "Convert video files to different formats",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the video file" },
                        ["output_format"] = new SchemaProperty { Type = "string", Description = "Output format", Enum = new List<string> { "mp4", "mov", "webm", "avi", "mkv" } }
                    },
                    Required = new List<string> { "file_path", "output_format" }
                }
            },
            new McpTool
            {
                Name = "trim-video",
                Description = "Trim video files to a specific time range",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the video file" },
                        ["start_time"] = new SchemaProperty { Type = "string", Description = "Start time (HH:MM:SS or seconds)" },
                        ["end_time"] = new SchemaProperty { Type = "string", Description = "End time (HH:MM:SS or seconds)" }
                    },
                    Required = new List<string> { "file_path", "start_time", "end_time" }
                }
            },
            new McpTool
            {
                Name = "merge-videos",
                Description = "Merge multiple video files into one",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of video file paths to merge", Items = new SchemaProperty { Type = "string" } }
                    },
                    Required = new List<string> { "file_paths" }
                }
            },
            new McpTool
            {
                Name = "burn-subtitles",
                Description = "Burn subtitles into video file",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the video file" },
                        ["subtitle_file"] = new SchemaProperty { Type = "string", Description = "Path to subtitle file (SRT, VTT)" },
                        ["auto_transcribe"] = new SchemaProperty { Type = "boolean", Description = "Auto-generate subtitles via transcription" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "download-video",
                Description = "Download video from URL (YouTube, etc.)",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["url"] = new SchemaProperty { Type = "string", Description = "Video URL" },
                        ["quality"] = new SchemaProperty { Type = "string", Description = "Video quality", Enum = new List<string> { "best", "1080p", "720p", "480p", "audio" } }
                    },
                    Required = new List<string> { "url" }
                }
            },

            // Audio Tools (3)
            new McpTool
            {
                Name = "clean-audio",
                Description = "Clean and enhance audio by removing background noise",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the audio file" },
                        ["noise_reduction"] = new SchemaProperty { Type = "string", Description = "Noise reduction level", Enum = new List<string> { "low", "medium", "high" } }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "trim-audio",
                Description = "Trim audio files to a specific time range",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the audio file" },
                        ["start_time"] = new SchemaProperty { Type = "string", Description = "Start time (HH:MM:SS or seconds)" },
                        ["end_time"] = new SchemaProperty { Type = "string", Description = "End time (HH:MM:SS or seconds)" }
                    },
                    Required = new List<string> { "file_path", "start_time", "end_time" }
                }
            },
            new McpTool
            {
                Name = "convert-audio",
                Description = "Convert audio files to different formats",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the audio file" },
                        ["output_format"] = new SchemaProperty { Type = "string", Description = "Output format", Enum = new List<string> { "mp3", "wav", "aac", "flac", "ogg" } }
                    },
                    Required = new List<string> { "file_path", "output_format" }
                }
            },

            // Image Tools (7)
            new McpTool
            {
                Name = "compress-images",
                Description = "Compress image files to reduce size",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of image file paths", Items = new SchemaProperty { Type = "string" } },
                        ["quality"] = new SchemaProperty { Type = "number", Description = "Quality level (1-100)" }
                    },
                    Required = new List<string> { "file_paths" }
                }
            },
            new McpTool
            {
                Name = "resize-images",
                Description = "Resize image files to specific dimensions",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of image file paths", Items = new SchemaProperty { Type = "string" } },
                        ["width"] = new SchemaProperty { Type = "number", Description = "Width in pixels" },
                        ["height"] = new SchemaProperty { Type = "number", Description = "Height in pixels" },
                        ["percentage"] = new SchemaProperty { Type = "number", Description = "Resize by percentage" }
                    },
                    Required = new List<string> { "file_paths" }
                }
            },
            new McpTool
            {
                Name = "remove-background",
                Description = "Remove background from images",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the image file" },
                        ["background_color"] = new SchemaProperty { Type = "string", Description = "Background color (transparent/white/hex)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "beautify-screenshot",
                Description = "Beautify screenshots with backgrounds, padding, shadows",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the screenshot file" },
                        ["background"] = new SchemaProperty { Type = "string", Description = "Background type (gradient/solid/image)" },
                        ["padding"] = new SchemaProperty { Type = "number", Description = "Padding in pixels" },
                        ["corner_radius"] = new SchemaProperty { Type = "number", Description = "Corner radius in pixels" },
                        ["shadow"] = new SchemaProperty { Type = "boolean", Description = "Add shadow effect" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "convert-images",
                Description = "Convert image files to different formats",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of image file paths", Items = new SchemaProperty { Type = "string" } },
                        ["output_format"] = new SchemaProperty { Type = "string", Description = "Output format", Enum = new List<string> { "png", "jpg", "bmp", "webp" } }
                    },
                    Required = new List<string> { "file_paths", "output_format" }
                }
            },
            new McpTool
            {
                Name = "blur-faces",
                Description = "Automatically detect and blur faces in images",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the image file" },
                        ["blur_type"] = new SchemaProperty { Type = "string", Description = "Type of blur", Enum = new List<string> { "blur", "pixelate", "black" } }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "watermark",
                Description = "Add watermark to images",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of image file paths", Items = new SchemaProperty { Type = "string" } },
                        ["watermark_text"] = new SchemaProperty { Type = "string", Description = "Watermark text" },
                        ["watermark_image"] = new SchemaProperty { Type = "string", Description = "Path to watermark image" },
                        ["position"] = new SchemaProperty { Type = "string", Description = "Position (top-left/top-right/bottom-left/bottom-right/center)" }
                    },
                    Required = new List<string> { "file_paths" }
                }
            },

            // Document Tools (8)
            new McpTool
            {
                Name = "merge-pdfs",
                Description = "Merge multiple PDF files into one",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of PDF file paths to merge", Items = new SchemaProperty { Type = "string" } }
                    },
                    Required = new List<string> { "file_paths" }
                }
            },
            new McpTool
            {
                Name = "compress-pdf",
                Description = "Compress PDF files to reduce size",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" },
                        ["quality"] = new SchemaProperty { Type = "string", Description = "Compression quality", Enum = new List<string> { "low", "medium", "high" } }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "sign-pdf",
                Description = "Add signature to PDF files",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" },
                        ["signature_image"] = new SchemaProperty { Type = "string", Description = "Path to signature image" },
                        ["signature_text"] = new SchemaProperty { Type = "string", Description = "Signature text" },
                        ["position"] = new SchemaProperty { Type = "string", Description = "Position (bottom-left/bottom-right/custom)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "split-pdf",
                Description = "Split PDF files into multiple documents",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" },
                        ["split_mode"] = new SchemaProperty { Type = "string", Description = "Split mode", Enum = new List<string> { "pages", "page_ranges" } },
                        ["pages"] = new SchemaProperty { Type = "string", Description = "Page numbers or ranges (e.g., '1,3,5-10')" }
                    },
                    Required = new List<string> { "file_path", "split_mode" }
                }
            },
            new McpTool
            {
                Name = "pdf-to-word",
                Description = "Convert PDF files to Word documents",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "pdf-to-images",
                Description = "Convert PDF pages to image files",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" },
                        ["output_format"] = new SchemaProperty { Type = "string", Description = "Output format", Enum = new List<string> { "png", "jpg" } },
                        ["dpi"] = new SchemaProperty { Type = "number", Description = "DPI quality (72-300)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "ocr-pdf",
                Description = "Add OCR text layer to scanned PDF files",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" },
                        ["language"] = new SchemaProperty { Type = "string", Description = "OCR language (en/es/fr/de/etc.)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "redact-pdf",
                Description = "Redact sensitive information from PDF files",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the PDF file" },
                        ["search_terms"] = new SchemaProperty { Type = "array", Description = "Terms to search and redact", Items = new SchemaProperty { Type = "string" } },
                        ["coordinates"] = new SchemaProperty { Type = "array", Description = "Coordinates to redact", Items = new SchemaProperty { Type = "object" } }
                    },
                    Required = new List<string> { "file_path" }
                }
            },

            // File Tools (3)
            new McpTool
            {
                Name = "batch-rename",
                Description = "Rename multiple files at once",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of file paths to rename", Items = new SchemaProperty { Type = "string" } },
                        ["operation"] = new SchemaProperty { Type = "string", Description = "Rename operation", Enum = new List<string> { "find_replace", "add_prefix", "add_suffix", "numbering" } },
                        ["find"] = new SchemaProperty { Type = "string", Description = "Text to find (for find_replace)" },
                        ["replace"] = new SchemaProperty { Type = "string", Description = "Text to replace with" },
                        ["prefix"] = new SchemaProperty { Type = "string", Description = "Prefix to add" },
                        ["suffix"] = new SchemaProperty { Type = "string", Description = "Suffix to add" }
                    },
                    Required = new List<string> { "file_paths", "operation" }
                }
            },
            new McpTool
            {
                Name = "generate-qr",
                Description = "Generate QR codes",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["content"] = new SchemaProperty { Type = "string", Description = "Content to encode in QR code" },
                        ["type"] = new SchemaProperty { Type = "string", Description = "QR code type", Enum = new List<string> { "text", "url", "wifi" } },
                        ["output_format"] = new SchemaProperty { Type = "string", Description = "Output format", Enum = new List<string> { "png", "svg" } }
                    },
                    Required = new List<string> { "content" }
                }
            },
            new McpTool
            {
                Name = "zip-files",
                Description = "Create ZIP archives from files",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_paths"] = new SchemaProperty { Type = "array", Description = "Array of file paths to zip", Items = new SchemaProperty { Type = "string" } },
                        ["output_name"] = new SchemaProperty { Type = "string", Description = "Name for the ZIP file" },
                        ["password"] = new SchemaProperty { Type = "string", Description = "Password for the ZIP file (optional)" }
                    },
                    Required = new List<string> { "file_paths" }
                }
            },

            // Text & Speech Tools (5)
            new McpTool
            {
                Name = "transcribe",
                Description = "Transcribe audio/video to text",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to audio/video file" },
                        ["language"] = new SchemaProperty { Type = "string", Description = "Language code (en/es/fr/etc.)" },
                        ["output_format"] = new SchemaProperty { Type = "string", Description = "Output format", Enum = new List<string> { "text", "srt", "vtt", "json" } }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "translate",
                Description = "Translate text or documents",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["text"] = new SchemaProperty { Type = "string", Description = "Text to translate" },
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to file to translate" },
                        ["source_language"] = new SchemaProperty { Type = "string", Description = "Source language code" },
                        ["target_language"] = new SchemaProperty { Type = "string", Description = "Target language code" }
                    },
                    Required = new List<string> { "target_language" }
                }
            },
            new McpTool
            {
                Name = "extract-text",
                Description = "Extract text from images or PDFs",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to image or PDF file" },
                        ["language"] = new SchemaProperty { Type = "string", Description = "OCR language (en/es/fr/etc.)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },
            new McpTool
            {
                Name = "summarize",
                Description = "Summarize text or documents",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["text"] = new SchemaProperty { Type = "string", Description = "Text to summarize" },
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to file to summarize" },
                        ["length"] = new SchemaProperty { Type = "string", Description = "Summary length", Enum = new List<string> { "brief", "medium", "detailed" } }
                    },
                    Required = new List<string>()
                }
            },
            new McpTool
            {
                Name = "ocr-image",
                Description = "Extract text from images using OCR",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to the image file" },
                        ["language"] = new SchemaProperty { Type = "string", Description = "OCR language (en/es/fr/etc.)" }
                    },
                    Required = new List<string> { "file_path" }
                }
            },

            // Developer Tools (4)
            new McpTool
            {
                Name = "format-json",
                Description = "Format and validate JSON data",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["json_string"] = new SchemaProperty { Type = "string", Description = "JSON string to format" }
                    },
                    Required = new List<string> { "json_string" }
                }
            },
            new McpTool
            {
                Name = "encode-base64",
                Description = "Encode text or files to Base64",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["text"] = new SchemaProperty { Type = "string", Description = "Text to encode" },
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to file to encode" }
                    },
                    Required = new List<string>()
                }
            },
            new McpTool
            {
                Name = "decode-base64",
                Description = "Decode Base64 to text or file",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["base64_string"] = new SchemaProperty { Type = "string", Description = "Base64 string to decode" },
                        ["output_type"] = new SchemaProperty { Type = "string", Description = "Output type", Enum = new List<string> { "text", "file" } }
                    },
                    Required = new List<string> { "base64_string" }
                }
            },
            new McpTool
            {
                Name = "generate-hash",
                Description = "Generate cryptographic hashes",
                InputSchema = new ToolInputSchema
                {
                    Properties = new Dictionary<string, SchemaProperty>
                    {
                        ["text"] = new SchemaProperty { Type = "string", Description = "Text to hash" },
                        ["file_path"] = new SchemaProperty { Type = "string", Description = "Path to file to hash" },
                        ["algorithms"] = new SchemaProperty { Type = "array", Description = "Hash algorithms", Items = new SchemaProperty { Type = "string", Enum = new List<string> { "md5", "sha1", "sha256", "sha512" } } }
                    },
                    Required = new List<string>()
                }
            }
        };
    }
}
