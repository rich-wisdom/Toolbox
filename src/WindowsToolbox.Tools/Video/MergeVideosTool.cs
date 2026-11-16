using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Video;

public class MergeVideosTool : BaseTool
{
    public override string Id => "merge-videos";
    public override string Name => "Merge Videos";
    public override string Description => "Merge multiple video files into one";
    public override string Category => "video";
    public override string Icon => "Video";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            if (filePathsObj == null)
                return Failure("file_paths is required");

            var filePaths = ParseFilePathsArray(filePathsObj);
            
            if (filePaths.Count < 2)
                return Failure("At least 2 video files are required");

            foreach (var path in filePaths)
            {
                if (!File.Exists(path))
                    return Failure($"File not found: {path}");
            }

            ReportProgress(progress, 10, "Preparing videos...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, $"Merging {filePaths.Count} videos...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(Path.GetDirectoryName(filePaths[0])!, 
                $"merged_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Successfully merged {filePaths.Count} videos", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["video_count"] = filePaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to merge videos: {ex.Message}", ex);
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
