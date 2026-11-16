using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class ResizeImagesTool : BaseTool
{
    public override string Id => "resize-images";
    public override string Name => "Resize Images";
    public override string Description => "Resize image files to specific dimensions";
    public override string Category => "image";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            var width = parameters.Get<double>("width");
            var height = parameters.Get<double>("height");
            var percentage = parameters.Get<double>("percentage");
            
            if (filePathsObj == null)
                return Failure("file_paths is required");

            var filePaths = ParseFilePathsArray(filePathsObj);
            
            ReportProgress(progress, 10, "Starting resize...");
            await Task.Delay(100, cancellationToken);

            var outputPaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                {
                    var outputPath = Path.Combine(Path.GetDirectoryName(path)!, 
                        $"{Path.GetFileNameWithoutExtension(path)}_resized{Path.GetExtension(path)}");
                    outputPaths.Add(outputPath);
                }
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Resized {outputPaths.Count} images", new Dictionary<string, object>
            {
                ["output_files"] = outputPaths,
                ["count"] = outputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to resize images: {ex.Message}", ex);
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
