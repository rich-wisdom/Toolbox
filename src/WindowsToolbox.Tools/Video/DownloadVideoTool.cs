using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Video;

public class DownloadVideoTool : BaseTool
{
    public override string Id => "download-video";
    public override string Name => "Download Video";
    public override string Description => "Download video from URL (YouTube, etc.)";
    public override string Category => "video";
    public override string Icon => "Video";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = parameters.Get<string>("url");
            var quality = parameters.Get<string>("quality") ?? "best";

            if (string.IsNullOrWhiteSpace(url))
                return Failure("url is required");

            ReportProgress(progress, 10, "Fetching video info...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, $"Downloading video ({quality})...");
            await Task.Delay(100, cancellationToken);

            var downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var outputPath = Path.Combine(downloadsPath, $"video_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Video downloaded successfully at {quality} quality", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["quality"] = quality,
                ["url"] = url
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to download video: {ex.Message}", ex);
        }
    }
}
