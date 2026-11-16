using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class VideoToGifTool : BaseTool
{
    public override string Id => "video-to-gif";
    public override string Name => "Video to GIF";
    public override string Description => "Convert videos to animated GIF with quality optimization";
    public override string Category => "video";
    public override string Icon => "Gif";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var startTime = parameters.Get<TimeSpan?>("startTime");
            var duration = parameters.Get<TimeSpan?>("duration");
            var fps = parameters.Get<int?>("fps") ?? 10; // Frames per second
            var width = parameters.Get<int?>("width") ?? 480; // Width in pixels
            var quality = parameters.Get<int?>("quality") ?? 80; // 1-100

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}.gif");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Converting to GIF...");

            // Build filter string for high-quality GIF
            // Use palette for better colors
            var filters = $"fps={fps},scale={width}:-1:flags=lanczos,split[s0][s1];[s0]palettegen=stats_mode=diff[p];[s1][p]paletteuse=dither=bayer:bayer_scale=5";

            var arguments = FFMpegArguments.FromFileInput(inputFile);
            
            if (startTime.HasValue)
                arguments = FFMpegArguments.FromFileInput(inputFile, true, opts => opts.Seek(startTime.Value));

            var processDuration = duration ?? (startTime.HasValue ? mediaInfo.Duration - startTime.Value : mediaInfo.Duration);
            
            var success = await arguments
                .OutputToFile(outputFile, true, options =>
                {
                    options.WithCustomArgument($"-vf \"{filters}\"");
                    if (duration.HasValue)
                        options.WithDuration(duration.Value);
                    options.WithCustomArgument("-loop 0"); // Infinite loop
                })
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Converting to GIF...");
                }, processDuration)
                .ProcessAsynchronously();

            if (success)
            {
                var fileSize = new FileInfo(outputFile).Length / 1024.0 / 1024.0;
                ReportProgress(progress, 100, "GIF created!");
                return Success($"GIF created successfully ({fileSize:F2}MB, {fps} fps)",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("GIF conversion failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error converting to GIF: {ex.Message}", ex);
        }
    }
}
