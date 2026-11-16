using System.Text.Json.Serialization;

namespace WindowsToolbox.MCP.Models;

public record JsonRpcRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
    
    [JsonPropertyName("method")]
    public string Method { get; init; } = "";
    
    [JsonPropertyName("params")]
    public object? Params { get; init; }
    
    [JsonPropertyName("id")]
    public object? Id { get; init; }
}

public record JsonRpcResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
    
    [JsonPropertyName("result")]
    public object? Result { get; init; }
    
    [JsonPropertyName("error")]
    public JsonRpcError? Error { get; init; }
    
    [JsonPropertyName("id")]
    public object? Id { get; init; }
}

public record JsonRpcError
{
    [JsonPropertyName("code")]
    public int Code { get; init; }
    
    [JsonPropertyName("message")]
    public string Message { get; init; } = "";
    
    [JsonPropertyName("data")]
    public object? Data { get; init; }
}

public record McpServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "windows-toolbox";
    
    [JsonPropertyName("version")]
    public string Version { get; init; } = "1.0.0";
}

public record McpCapabilities
{
    [JsonPropertyName("tools")]
    public ToolCapabilities Tools { get; init; } = new();
}

public record ToolCapabilities
{
    [JsonPropertyName("listChanged")]
    public bool ListChanged { get; init; } = true;
}

public record McpInitializeResult
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; init; } = "2024-11-05";
    
    [JsonPropertyName("capabilities")]
    public McpCapabilities Capabilities { get; init; } = new();
    
    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; init; } = new();
}

public record McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";
    
    [JsonPropertyName("description")]
    public string Description { get; init; } = "";
    
    [JsonPropertyName("inputSchema")]
    public ToolInputSchema InputSchema { get; init; } = new();
}

public record ToolInputSchema
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "object";
    
    [JsonPropertyName("properties")]
    public Dictionary<string, SchemaProperty> Properties { get; init; } = new();
    
    [JsonPropertyName("required")]
    public List<string> Required { get; init; } = new();
}

public record SchemaProperty
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "string";
    
    [JsonPropertyName("description")]
    public string Description { get; init; } = "";
    
    [JsonPropertyName("enum")]
    public List<string>? Enum { get; init; }
    
    [JsonPropertyName("items")]
    public SchemaProperty? Items { get; init; }
}

public record McpToolsListResult
{
    [JsonPropertyName("tools")]
    public List<McpTool> Tools { get; init; } = new();
}

public record McpToolCallParams
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = "";
    
    [JsonPropertyName("arguments")]
    public Dictionary<string, object>? Arguments { get; init; }
}

public record McpToolCallResult
{
    [JsonPropertyName("content")]
    public List<McpContent> Content { get; init; } = new();
    
    [JsonPropertyName("isError")]
    public bool IsError { get; init; }
    
    [JsonPropertyName("_meta")]
    public Dictionary<string, object>? Meta { get; init; }
}

public record McpContent
{
    [JsonPropertyName("type")]
    public string Type { get; init; } = "text";
    
    [JsonPropertyName("text")]
    public string Text { get; init; } = "";
}

public record McpProgressNotification
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; init; } = "2.0";
    
    [JsonPropertyName("method")]
    public string Method { get; init; } = "notifications/progress";
    
    [JsonPropertyName("params")]
    public ProgressParams Params { get; init; } = new();
}

public record ProgressParams
{
    [JsonPropertyName("progressToken")]
    public string ProgressToken { get; init; } = "";
    
    [JsonPropertyName("progress")]
    public int Progress { get; init; }
    
    [JsonPropertyName("total")]
    public int Total { get; init; } = 100;
}
