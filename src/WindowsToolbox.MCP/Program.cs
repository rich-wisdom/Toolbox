using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.MCP.Services;
using WindowsToolbox.Tools.Video;
using WindowsToolbox.Tools.Audio;
using WindowsToolbox.Tools.Image;
using WindowsToolbox.Tools.Document;
using WindowsToolbox.Tools.File;
using WindowsToolbox.Tools.TextAndSpeech;
using WindowsToolbox.Tools.Developer;

namespace WindowsToolbox.MCP;

class Program
{
    static async Task<int> Main(string[] args)
    {
        try
        {
            var logPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WindowsToolbox",
                "Logs",
                $"mcp_{DateTime.Now:yyyyMMdd_HHmmss}.log"
            );

            Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);
            
            using var logFile = new StreamWriter(logPath, append: true) { AutoFlush = true };
            await logFile.WriteLineAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MCP Server starting...");

            var tools = GetAllTools();
            await logFile.WriteLineAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Loaded {tools.Count} tools");

            var toolExecutor = new ToolExecutor(tools, Console.Out);
            var protocolHandler = new McpProtocolHandler(
                toolExecutor,
                Console.In,
                Console.Out,
                logFile
            );

            await logFile.WriteLineAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Starting protocol handler...");
            await protocolHandler.RunAsync();

            await logFile.WriteLineAsync($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] MCP Server stopped");
            return 0;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Fatal error: {ex.Message}");
            return 1;
        }
    }

    private static List<ITool> GetAllTools()
    {
        return new List<ITool>
        {
            // Video Tools (6)
            new CompressVideoTool(),
            new ConvertVideoTool(),
            new TrimVideoTool(),
            new MergeVideosTool(),
            new BurnSubtitlesTool(),
            new DownloadVideoTool(),

            // Audio Tools (3)
            new CleanUpAudioTool(),
            new TrimAudioTool(),
            new ConvertAudioTool(),

            // Image Tools (7)
            new CompressImagesTool(),
            new ResizeImagesTool(),
            new RemoveBackgroundTool(),
            new BeautifyScreenshotTool(),
            new ConvertImagesTool(),
            new BlurFacesTool(),
            new WatermarkTool(),

            // Document Tools (8)
            new MergePdfsTool(),
            new CompressPdfTool(),
            new SignPdfTool(),
            new SplitPdfTool(),
            new PdfToWordTool(),
            new PdfToImagesTool(),
            new OcrPdfTool(),
            new RedactPdfTool(),

            // File Tools (3)
            new BatchRenameTool(),
            new QRCodeTool(),
            new ZipFilesTool(),

            // Text & Speech Tools (5)
            new TranscribeTool(),
            new TranslateTool(),
            new ExtractTextTool(),
            new SummarizeTool(),
            new OcrImageTool(),

            // Developer Tools (4)
            new JsonFormatterTool(),
            new Base64EncodeTool(),
            new Base64DecodeTool(),
            new HashGeneratorTool()
        };
    }
}
