using System.Text.Json;
using WindowsToolbox.MCP.Models;

namespace WindowsToolbox.MCP.Services;

public class McpProtocolHandler
{
    private readonly ToolExecutor _toolExecutor;
    private readonly TextReader _input;
    private readonly TextWriter _output;
    private readonly TextWriter _errorOutput;
    private readonly JsonSerializerOptions _jsonOptions;

    public McpProtocolHandler(ToolExecutor toolExecutor, TextReader input, TextWriter output, TextWriter errorOutput)
    {
        _toolExecutor = toolExecutor;
        _input = input;
        _output = output;
        _errorOutput = errorOutput;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await LogDebugAsync("MCP Server starting...");

            while (!cancellationToken.IsCancellationRequested)
            {
                var line = await _input.ReadLineAsync();
                if (line == null)
                {
                    await LogDebugAsync("End of input stream");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                await LogDebugAsync($"Received: {line}");

                try
                {
                    var request = JsonSerializer.Deserialize<JsonRpcRequest>(line, _jsonOptions);
                    if (request == null)
                    {
                        await SendErrorResponse(null, -32700, "Parse error");
                        continue;
                    }

                    await HandleRequestAsync(request, cancellationToken);
                }
                catch (JsonException ex)
                {
                    await LogDebugAsync($"JSON parse error: {ex.Message}");
                    await SendErrorResponse(null, -32700, "Parse error: Invalid JSON");
                }
                catch (Exception ex)
                {
                    await LogDebugAsync($"Error processing request: {ex.Message}");
                    await SendErrorResponse(null, -32603, $"Internal error: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            await LogDebugAsync($"Fatal error: {ex.Message}");
            throw;
        }
    }

    private async Task HandleRequestAsync(JsonRpcRequest request, CancellationToken cancellationToken)
    {
        object? result = null;
        JsonRpcError? error = null;

        try
        {
            switch (request.Method)
            {
                case "initialize":
                    result = HandleInitialize();
                    break;

                case "initialized":
                    await LogDebugAsync("Client acknowledged initialization");
                    return;

                case "tools/list":
                    result = HandleListTools();
                    break;

                case "tools/call":
                    result = await HandleCallToolAsync(request.Params, cancellationToken);
                    break;

                case "ping":
                    result = new { status = "pong" };
                    break;

                default:
                    error = new JsonRpcError
                    {
                        Code = -32601,
                        Message = $"Method not found: {request.Method}"
                    };
                    break;
            }
        }
        catch (Exception ex)
        {
            await LogDebugAsync($"Error handling method {request.Method}: {ex.Message}");
            error = new JsonRpcError
            {
                Code = -32603,
                Message = $"Internal error: {ex.Message}"
            };
        }

        var response = new JsonRpcResponse
        {
            Result = result,
            Error = error,
            Id = request.Id
        };

        await SendResponseAsync(response);
    }

    private McpInitializeResult HandleInitialize()
    {
        return new McpInitializeResult();
    }

    private McpToolsListResult HandleListTools()
    {
        return new McpToolsListResult
        {
            Tools = ToolSchemaProvider.GetAllToolSchemas()
        };
    }

    private async Task<McpToolCallResult> HandleCallToolAsync(object? paramsObj, CancellationToken cancellationToken)
    {
        if (paramsObj == null)
        {
            return new McpToolCallResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = "Missing parameters" }
                },
                IsError = true
            };
        }

        var paramsJson = JsonSerializer.Serialize(paramsObj, _jsonOptions);
        var toolCallParams = JsonSerializer.Deserialize<McpToolCallParams>(paramsJson, _jsonOptions);

        if (toolCallParams == null || string.IsNullOrEmpty(toolCallParams.Name))
        {
            return new McpToolCallResult
            {
                Content = new List<McpContent>
                {
                    new McpContent { Type = "text", Text = "Invalid tool call parameters" }
                },
                IsError = true
            };
        }

        await LogDebugAsync($"Calling tool: {toolCallParams.Name}");
        return await _toolExecutor.ExecuteToolAsync(toolCallParams.Name, toolCallParams.Arguments, cancellationToken);
    }

    private async Task SendResponseAsync(JsonRpcResponse response)
    {
        try
        {
            var json = JsonSerializer.Serialize(response, _jsonOptions);
            await LogDebugAsync($"Sending response: {json}");
            await _output.WriteLineAsync(json);
            await _output.FlushAsync();
        }
        catch (Exception ex)
        {
            await LogDebugAsync($"Error sending response: {ex.Message}");
        }
    }

    private async Task SendErrorResponse(object? id, int code, string message)
    {
        var response = new JsonRpcResponse
        {
            Error = new JsonRpcError { Code = code, Message = message },
            Id = id
        };
        await SendResponseAsync(response);
    }

    private async Task LogDebugAsync(string message)
    {
        try
        {
            await _errorOutput.WriteLineAsync($"[MCP] {DateTime.Now:HH:mm:ss.fff} {message}");
            await _errorOutput.FlushAsync();
        }
        catch
        {
        }
    }
}
