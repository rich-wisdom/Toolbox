using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class TrimVideoTool : BaseTool
{
    public override string Id => "trim-video";
    public override string Name => "Trim Video";
    public override string Description => "Cut video segments by start and end time";
    public override string Category => "video";
    public override string Icon => "Cut";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var startTime = parameters.Get<TimeSpan?>("startTime") ?? TimeSpan.Zero;
            var endTime = parameters.Get<TimeSpan?>("endTime");
            var fastMode = parameters.Get<bool?>("fastMode") ?? true; // stream copy vs re-encode

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}_trimmed{Path.GetExtension(inputFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);

            if (!endTime.HasValue)
                endTime = mediaInfo.Duration;

            var duration = endTime.Value - startTime;

            ReportProgress(progress, 10, "Trimming video...");

            var success = await FFMpegArguments
                .FromFileInput(inputFile, true, options => options.Seek(startTime))
                .OutputToFile(outputFile, true, options =>
                {
                    options.WithDuration(duration);
                    if (fastMode)
                    {
                        options.CopyChannel();
                    }
                    else
                    {
                        options.WithFastStart();
                    }
                })
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Trimming...");
                }, duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Trimming complete!");
                return Success($"Video trimmed from {startTime:hh\\:mm\\:ss} to {endTime:hh\\:mm\\:ss}",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Trimming failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error trimming video: {ex.Message}", ex);
        }
    }
}
