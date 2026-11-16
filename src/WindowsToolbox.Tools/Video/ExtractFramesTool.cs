using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class ExtractFramesTool : BaseTool
{
    public override string Id => "extract-frames";
    public override string Name => "Extract Frames";
    public override string Description => "Extract images from video at specified intervals";
    public override string Category => "video";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var mode = parameters.Get<string>("mode") ?? "interval"; // all, interval, times
            var interval = parameters.Get<int?>("interval") ?? 1; // Extract every Nth frame
            var fps = parameters.Get<int?>("fps") ?? 1; // Frames per second
            var outputFormat = parameters.Get<string>("outputFormat") ?? "png";
            var startTime = parameters.Get<TimeSpan?>("startTime");
            var endTime = parameters.Get<TimeSpan?>("endTime");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputDir = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}_frames");
            Directory.CreateDirectory(outputDir);

            var outputPattern = Path.Combine(outputDir, $"frame_%04d.{outputFormat}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Extracting frames...");

            var filterArgs = mode switch
            {
                "all" => "",
                "interval" => $"-vf \"select='not(mod(n\\,{interval}))'\" -vsync 0",
                "fps" => $"-vf \"fps={fps}\"",
                _ => $"-vf \"fps={fps}\""
            };

            var timeArgs = "";
            if (startTime.HasValue)
                timeArgs += $"-ss {startTime.Value.TotalSeconds:F3} ";
            if (endTime.HasValue && startTime.HasValue)
                timeArgs += $"-to {(endTime.Value - startTime.Value).TotalSeconds:F3} ";

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputPattern, true, options => options
                    .WithCustomArgument($"{timeArgs} {filterArgs}"))
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Extracting frames...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                var frameCount = Directory.GetFiles(outputDir, $"*.{outputFormat}").Length;
                ReportProgress(progress, 100, "Frame extraction complete!");
                return Success($"Extracted {frameCount} frames",
                    new Dictionary<string, object> 
                    { 
                        ["outputDir"] = outputDir,
                        ["frameCount"] = frameCount
                    });
            }

            return Failure("Frame extraction failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error extracting frames: {ex.Message}", ex);
        }
    }
}
