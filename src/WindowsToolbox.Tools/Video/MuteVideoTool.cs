using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class MuteVideoTool : BaseTool
{
    public override string Id => "mute-video";
    public override string Name => "Mute Video";
    public override string Description => "Remove audio from videos";
    public override string Category => "video";
    public override string Icon => "MusicNote";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}_muted{Path.GetExtension(inputFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Removing audio...");

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                    .WithCustomArgument("-an") // Remove audio
                    .CopyChannel(Channel.Video)
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Removing audio...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Audio removed!");
                return Success("Audio removed successfully",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Muting failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error muting video: {ex.Message}", ex);
        }
    }
}
