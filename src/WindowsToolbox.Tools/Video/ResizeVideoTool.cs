using FFMpegCore;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class ResizeVideoTool : BaseTool
{
    public override string Id => "resize-video";
    public override string Name => "Resize Video";
    public override string Description => "Change video resolution (4K, 1080p, 720p, etc.)";
    public override string Category => "video";
    public override string Icon => "Resize";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var preset = parameters.Get<string>("preset"); // 4K, 1080p, 720p, 480p, 360p
            var customWidth = parameters.Get<int?>("customWidth");
            var customHeight = parameters.Get<int?>("customHeight");
            var maintainAspectRatio = parameters.Get<bool?>("maintainAspectRatio") ?? true;
            var scalingAlgorithm = parameters.Get<string>("scalingAlgorithm") ?? "lanczos"; // bilinear, bicubic, lanczos

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}_resized{Path.GetExtension(inputFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            var (targetWidth, targetHeight) = GetTargetResolution(preset, customWidth, customHeight);

            if (targetWidth == 0 || targetHeight == 0)
                return Failure("Invalid resolution specified");

            ReportProgress(progress, 10, $"Resizing to {targetWidth}x{targetHeight}...");

            var scaleFilter = maintainAspectRatio
                ? $"scale={targetWidth}:{targetHeight}:force_original_aspect_ratio=decrease"
                : $"scale={targetWidth}:{targetHeight}";

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                    .WithCustomArgument($"-vf \"{scaleFilter}\"")
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Resizing...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Resizing complete!");
                return Success($"Video resized to {targetWidth}x{targetHeight}",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Resizing failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error resizing video: {ex.Message}", ex);
        }
    }

    private static (int width, int height) GetTargetResolution(string? preset, int? customWidth, int? customHeight)
    {
        if (customWidth.HasValue && customHeight.HasValue)
            return (customWidth.Value, customHeight.Value);

        return preset?.ToLower() switch
        {
            "4k" => (3840, 2160),
            "1080p" => (1920, 1080),
            "720p" => (1280, 720),
            "480p" => (854, 480),
            "360p" => (640, 360),
            _ => (1920, 1080)
        };
    }
}
