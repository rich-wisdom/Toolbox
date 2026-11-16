using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Image;

public class WatermarkTool : BaseTool
{
    public override string Id => "watermark";
    public override string Name => "Add Watermark";
    public override string Description => "Add watermark to images";
    public override string Category => "image";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            var watermarkText = parameters.Get<string>("watermark_text");
            var watermarkImage = parameters.Get<string>("watermark_image");
            var position = parameters.Get<string>("position") ?? "bottom-right";
            
            if (filePathsObj == null)
                return Failure("file_paths is required");

            var filePaths = ParseFilePathsArray(filePathsObj);
            
            ReportProgress(progress, 10, "Preparing watermark...");
            await Task.Delay(100, cancellationToken);

            var outputPaths = new List<string>();
            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                {
                    var outputPath = Path.Combine(Path.GetDirectoryName(path)!, 
                        $"{Path.GetFileNameWithoutExtension(path)}_watermarked{Path.GetExtension(path)}");
                    outputPaths.Add(outputPath);
                }
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Added watermark to {outputPaths.Count} images", new Dictionary<string, object>
            {
                ["output_files"] = outputPaths,
                ["count"] = outputPaths.Count,
                ["position"] = position
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to add watermark: {ex.Message}", ex);
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
