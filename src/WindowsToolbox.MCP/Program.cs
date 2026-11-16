using System.Text.Json;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Core.Services;
using WindowsToolbox.Tools.Developer;
using WindowsToolbox.Tools.File;

namespace WindowsToolbox.MCP;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new McpServer();
        await server.RunAsync();
    }
}

public class McpServer
{
    private readonly IToolRegistry _toolRegistry;
    private readonly TextReader _input;
    private readonly TextWriter _output;

    public McpServer()
    {
        var tools = new ITool[]
        {
            // Developer Tools
            new JsonFormatterTool(),
            new Base64Tool(),
            new HashGeneratorTool(),
            new UuidGeneratorTool(),
            new JwtDecoderTool(),
            new TimestampConverterTool(),
            new DiffCheckerTool(),
            new RegexTesterTool(),
            new CleanLinksTool(),
            new CoordinatePickerTool(),
            new UnicodeInspectorTool(),
            new LoremIpsumTool(),
            new PasswordGeneratorTool(),
            new CsvJsonConverterTool(),
            
            // File Tools
            new QRCodeTool(),
            new BatchRenameTool(),
            new FindDuplicatesTool(),
            new ZipUnzipTool(),
            new OrganizeDownloadsTool(),
        };
        
        _toolRegistry = new ToolRegistry(tools);
        _input = Console.In;
        _output = Console.Out;
    }

    public async Task RunAsync()
    {
        await _output.WriteLineAsync(JsonSerializer.Serialize(new
        {
            jsonrpc = "2.0",
            method = "server.initialized"
        }));
        await _output.FlushAsync();

        while (true)
        {
            var line = await _input.ReadLineAsync();
            if (line == null) break;

            try
            {
                var request = JsonSerializer.Deserialize<JsonElement>(line);
                var method = request.GetProperty("method").GetString();
                var id = request.TryGetProperty("id", out var idElement) ? idElement.GetInt32() : 0;

                object? result = method switch
                {
                    "tools/list" => ListTools(),
                    "tools/call" => await CallTool(request.GetProperty("params")),
                    _ => new { error = "Method not found" }
                };

                var response = new
                {
                    jsonrpc = "2.0",
                    id,
                    result
                };

                await _output.WriteLineAsync(JsonSerializer.Serialize(response));
                await _output.FlushAsync();
            }
            catch (Exception ex)
            {
                await _output.WriteLineAsync(JsonSerializer.Serialize(new
                {
                    jsonrpc = "2.0",
                    error = new { message = ex.Message }
                }));
                await _output.FlushAsync();
            }
        }
    }

    private object ListTools()
    {
        var tools = _toolRegistry.GetAllTools();
        return new
        {
            tools = tools.Select(t => new
            {
                name = t.Id,
                description = t.Description,
                inputSchema = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>(),
                    required = new string[] { }
                }
            })
        };
    }

    private async Task<object> CallTool(JsonElement parameters)
    {
        var toolId = parameters.GetProperty("name").GetString();
        var tool = _toolRegistry.GetToolById(toolId ?? "");
        
        if (tool == null)
        {
            return new { error = "Tool not found" };
        }

        var args = parameters.TryGetProperty("arguments", out var argsElement)
            ? JsonSerializer.Deserialize<Dictionary<string, object>>(argsElement.GetRawText())
            : new Dictionary<string, object>();

        var result = await tool.ExecuteAsync(new ToolParameters(args ?? new()));

        return new
        {
            success = result.Success,
            content = new[]
            {
                new
                {
                    type = "text",
                    text = result.Message
                }
            }
        };
    }
}
