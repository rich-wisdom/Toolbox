using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.MCP.Services;
using WindowsToolbox.Tools.FileUtilities;
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
            // File Tools (3)
            new BatchRenameTool(),
            new QRCodeTool(),
            new ZipFilesTool(),

            // Developer Tools (4)
            new JsonFormatterTool(),
            new Base64EncodeTool(),
            new Base64DecodeTool(),
            new HashGeneratorTool()
        };
    }
}
