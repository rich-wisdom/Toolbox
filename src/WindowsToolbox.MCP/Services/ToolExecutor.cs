using System.Text.Json;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.MCP.Models;

namespace WindowsToolbox.MCP.Services;

public class ToolExecutor
{
    private readonly Dictionary<string, ITool> _tools;
    private readonly TextWriter _output;
    private static readonly Dictionary<string, string> ToolAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["clean-audio"] = "clean-up-audio",
        ["generate-qr"] = "qr-code",
        ["format-json"] = "json-formatter",
        ["generate-hash"] = "hash-generator"
    };

    public ToolExecutor(IEnumerable<ITool> tools, TextWriter output)
    {
        _tools = tools
            .Where(t => t.IsAvailable)
            .ToDictionary(t => ConvertToolId(t.Id), t => t);
        _output = output;
    }

    public bool ContainsTool(string toolName) => ResolveTool(toolName) is not null;

    public async Task<McpToolCallResult> ExecuteToolAsync(string toolName, Dictionary<string, object>? arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            var tool = ResolveTool(toolName);
            if (tool is null)
            {
                return new McpToolCallResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent { Type = "text", Text = $"Tool not found: {toolName}" }
                    },
                    IsError = true
                };
            }

            var parameters = new ToolParameters(NormalizeArguments(arguments));
            
            if (!tool.CanExecute(parameters))
            {
                return new McpToolCallResult
                {
                    Content = new List<McpContent>
                    {
                        new McpContent { Type = "text", Text = $"Tool '{toolName}' cannot execute with provided parameters" }
                    },
                    IsError = true
                };
            }

            var progressToken = $"{toolName}-{Guid.NewGuid():N}";
            var progress = new Progress<ToolProgress>(p => SendProgressNotification(progressToken, p));

            var result = await tool.ExecuteAsync(parameters, progress, cancellationToken);

            return new McpToolCallResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = result.Message }
                },
                IsError = !result.Success,
                Meta = result.Data
            };
        }
        catch (Exception ex)
        {
            return new McpToolCallResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = $"Error executing tool: {ex.Message}" }
                },
                IsError = true
            };
        }
    }

    private void SendProgressNotification(string progressToken, ToolProgress progress)
    {
        try
        {
            var notification = new McpProgressNotification
            {
                Params = new ProgressParams
                {
                    ProgressToken = progressToken,
                    Progress = progress.Percentage,
                    Total = 100
                }
            };

            var json = JsonSerializer.Serialize(notification, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _output.WriteLine(json);
            _output.Flush();
        }
        catch
        {
        }
    }

    private static string ConvertToolId(string toolId)
    {
        return toolId.ToLowerInvariant().Replace("_", "-");
    }

    private ITool? ResolveTool(string toolName)
    {
        var resolvedName = ToolAliases.TryGetValue(toolName, out var alias) ? alias : toolName;
        return _tools.TryGetValue(ConvertToolId(resolvedName), out var tool) ? tool : null;
    }

    private static Dictionary<string, object> NormalizeArguments(Dictionary<string, object>? arguments)
    {
        var normalized = arguments is null
            ? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase)
            : new Dictionary<string, object>(arguments, StringComparer.OrdinalIgnoreCase);

        AddAliases(normalized, "file_path", "inputFile", "inputPath", "videoFile");
        AddAliases(normalized, "file_paths", "inputFiles", "inputPath", "files");
        AddAliases(normalized, "output_format", "outputFormat", "targetFormat", "format");
        AddAliases(normalized, "output_name", "outputFile", "outputPath");
        AddAliases(normalized, "start_time", "startTime");
        AddAliases(normalized, "end_time", "endTime");
        AddAliases(normalized, "subtitle_file", "subtitleFile");
        AddAliases(normalized, "background_color", "backgroundColor");
        AddAliases(normalized, "corner_radius", "cornerRadius");
        AddAliases(normalized, "watermark_text", "watermarkText");
        AddAliases(normalized, "watermark_image", "watermarkImagePath");
        AddAliases(normalized, "split_mode", "splitMode");
        AddAliases(normalized, "source_language", "sourceLanguage");
        AddAliases(normalized, "target_language", "targetLanguage");
        AddAliases(normalized, "json_string", "input");
        AddAliases(normalized, "text", "inputText", "input");
        AddAliases(normalized, "length", "summaryLength");
        AddAliases(normalized, "find", "findText");
        AddAliases(normalized, "replace", "replaceText");

        return normalized;
    }

    private static void AddAliases(Dictionary<string, object> arguments, string source, params string[] aliases)
    {
        if (!arguments.TryGetValue(source, out var value))
            return;

        foreach (var alias in aliases)
            arguments.TryAdd(alias, value);
    }
}
