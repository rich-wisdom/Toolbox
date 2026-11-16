using System.Text.Json;

namespace WindowsToolbox.Core.Interfaces;

public interface ITool
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    string Category { get; }
    string Icon { get; }
    bool IsAvailable { get; }
    bool IsPremium { get; }
    
    Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default);
    
    bool CanExecute(ToolParameters parameters);
}

public record ToolParameters(Dictionary<string, object> Values)
{
    public T? Get<T>(string key)
    {
        if (!Values.TryGetValue(key, out var value) || value is null)
            return default;

        if (value is T typedValue)
            return typedValue;

        try
        {
            if (value is JsonElement element)
                return element.Deserialize<T>();

            return JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value));
        }
        catch (JsonException)
        {
            return default;
        }
    }
}

public record ToolResult(bool Success, string Message, Dictionary<string, object>? Data = null, Exception? Error = null)
{
    public static ToolResult Succeeded(string message, Dictionary<string, object>? data = null) => new(true, message, data);
    public static ToolResult Failed(string message, Exception? error = null) => new(false, message, null, error);
}

public record ToolProgress(int Percentage, string Status, string? CurrentFile = null);
