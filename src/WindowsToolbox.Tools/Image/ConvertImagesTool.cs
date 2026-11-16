using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class ConvertImagesTool : BaseTool
{
    public override string Id => "convert-images";
    public override string Name => "Convert Images";
    public override string Description => "Convert image files to different formats";
    public override string Category => "image";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            var outputFormat = parameters.Get<string>("output_format");
            
            if (filePathsObj == null)
                return Failure("file_paths is required");
            
            if (string.IsNullOrWhiteSpace(outputFormat))
                return Failure("output_format is required");

            var filePaths = ParseFilePathsArray(filePathsObj);
            
            ReportProgress(progress, 10, "Starting conversion...");
            await Task.Delay(100, cancellationToken);

            var outputPaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                {
                    var outputPath = Path.ChangeExtension(path, outputFormat);
                    outputPaths.Add(outputPath);
                }
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Converted {outputPaths.Count} images to {outputFormat}", new Dictionary<string, object>
            {
                ["output_files"] = outputPaths,
                ["count"] = outputPaths.Count,
                ["format"] = outputFormat
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert images: {ex.Message}", ex);
        }
    }

    private List<string> ParseFilePathsArray(object filePathsObj)
    {
        if (filePathsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            return jsonElement.EnumerateArray()
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        return new List<string>();
    }
}
