using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class JsonFormatterTool : BaseTool
{
    public override string Id => "json-formatter";
    public override string Name => "JSON Formatter";
    public override string Description => "Format and validate JSON data";
    public override string Category => "developer";
    public override string Icon => "Code";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = parameters.Get<string>("input");
            if (string.IsNullOrWhiteSpace(input))
                return Failure("Input JSON is required");

            ReportProgress(progress, 30, "Parsing JSON...");

            var jsonDocument = JsonDocument.Parse(input);
            
            ReportProgress(progress, 70, "Formatting JSON...");

            var options = new JsonSerializerOptions
            {
                WriteIndented = parameters.Get<bool>("indent"),
                PropertyNamingPolicy = parameters.Get<bool>("camelCase") ? JsonNamingPolicy.CamelCase : null
            };

            var formatted = JsonSerializer.Serialize(jsonDocument, options);

            ReportProgress(progress, 100, "Complete");

            return Success("JSON formatted successfully", new Dictionary<string, object>
            {
                ["output"] = formatted,
                ["isValid"] = true
            });
        }
        catch (JsonException ex)
        {
            return Failure($"Invalid JSON: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to format JSON: {ex.Message}", ex);
        }
    }
}
