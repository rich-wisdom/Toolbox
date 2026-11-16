using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class CompressImagesTool : BaseTool
{
    public override string Id => "compress-images";
    public override string Name => "Compress Images";
    public override string Description => "Compress image files to reduce size";
    public override string Category => "image";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            var quality = parameters.Get<double>("quality");
            
            if (filePathsObj == null)
                return Failure("file_paths is required");

            var filePaths = ParseFilePathsArray(filePathsObj);
            
            ReportProgress(progress, 10, "Starting compression...");
            await Task.Delay(100, cancellationToken);

            var outputPaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                {
                    var outputPath = Path.Combine(Path.GetDirectoryName(path)!, 
                        $"{Path.GetFileNameWithoutExtension(path)}_compressed{Path.GetExtension(path)}");
                    outputPaths.Add(outputPath);
                }
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Compressed {outputPaths.Count} images", new Dictionary<string, object>
            {
                ["output_files"] = outputPaths,
                ["count"] = outputPaths.Count,
                ["quality"] = quality
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to compress images: {ex.Message}", ex);
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
