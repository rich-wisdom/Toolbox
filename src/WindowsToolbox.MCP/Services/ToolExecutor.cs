using System.Text.Json;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.MCP.Models;

namespace WindowsToolbox.MCP.Services;

public class ToolExecutor
{
    private readonly Dictionary<string, ITool> _tools;
    private readonly TextWriter _output;

    public ToolExecutor(IEnumerable<ITool> tools, TextWriter output)
    {
        _tools = tools.ToDictionary(t => ConvertToolId(t.Id), t => t);
        _output = output;
    }

    public async Task<McpToolCallResult> ExecuteToolAsync(string toolName, Dictionary<string, object>? arguments, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!_tools.TryGetValue(toolName, out var tool))
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

            var parameters = new ToolParameters(arguments ?? new Dictionary<string, object>());
            
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
}
