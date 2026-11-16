using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class ChangeSpeedTool : BaseTool
{
    public override string Id => "change-video-speed";
    public override string Name => "Change Video Speed";
    public override string Description => "Speed up or slow down videos (0.25x to 4x)";
    public override string Category => "video";
    public override string Icon => "FastForward";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var speed = parameters.Get<double?>("speed") ?? 1.0; // 0.25 to 4.0
            var maintainPitch = parameters.Get<bool?>("maintainPitch") ?? true;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            if (speed < 0.25 || speed > 4.0)
                return Failure("Speed must be between 0.25x and 4x");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}_{speed}x{Path.GetExtension(inputFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, $"Changing speed to {speed}x...");

            var videoSpeed = 1.0 / speed;
            var audioSpeed = speed;

            var videoFilter = $"setpts={videoSpeed:F3}*PTS";
            var audioFilter = maintainPitch
                ? $"atempo={audioSpeed:F3}"
                : $"asetrate=44100*{audioSpeed:F3}";

            // atempo filter only supports 0.5-2.0, so chain multiple if needed
            var audioFilterFinal = audioFilter;
            if (audioSpeed < 0.5 || audioSpeed > 2.0)
            {
                var filters = new List<string>();
                var remaining = audioSpeed;
                while (remaining > 2.0)
                {
                    filters.Add("atempo=2.0");
                    remaining /= 2.0;
                }
                while (remaining < 0.5)
                {
                    filters.Add("atempo=0.5");
                    remaining *= 2.0;
                }
                if (remaining != 1.0)
                    filters.Add($"atempo={remaining:F3}");
                audioFilterFinal = string.Join(",", filters);
            }

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                    .WithCustomArgument($"-filter:v \"{videoFilter}\" -filter:a \"{audioFilterFinal}\"")
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Processing...");
                }, TimeSpan.FromSeconds(mediaInfo.Duration.TotalSeconds / speed))
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Speed change complete!");
                return Success($"Video speed changed to {speed}x",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Speed change failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error changing video speed: {ex.Message}", ex);
        }
    }
}
