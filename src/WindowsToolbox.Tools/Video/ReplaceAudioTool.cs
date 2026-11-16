using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class ReplaceAudioTool : BaseTool
{
    public override string Id => "replace-audio";
    public override string Name => "Replace Audio";
    public override string Description => "Replace video audio with a different audio file";
    public override string Category => "video";
    public override string Icon => "MusicNote";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var videoFile = parameters.Get<string>("videoFile");
            var audioFile = parameters.Get<string>("audioFile");
            var audioOffset = parameters.Get<TimeSpan?>("audioOffset") ?? TimeSpan.Zero;
            var volume = parameters.Get<double?>("volume") ?? 1.0; // 0.0 to 2.0

            if (string.IsNullOrEmpty(videoFile) || !File.Exists(videoFile))
                return Failure("Video file not found");

            if (string.IsNullOrEmpty(audioFile) || !File.Exists(audioFile))
                return Failure("Audio file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(videoFile)!,
                $"{Path.GetFileNameWithoutExtension(videoFile)}_newaudio{Path.GetExtension(videoFile)}");

            ReportProgress(progress, 0, "Analyzing files...");
            
            FFmpegService.EnsureFFmpegPath();
            var videoInfo = await FFProbe.AnalyseAsync(videoFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Replacing audio...");

            var volumeFilter = volume != 1.0 ? $"volume={volume:F2}" : "";
            var filterArg = string.IsNullOrEmpty(volumeFilter) ? "" : $"-af \"{volumeFilter}\"";

            var offsetArg = audioOffset != TimeSpan.Zero ? $"-itsoffset {audioOffset.TotalSeconds:F3}" : "";

            var success = await FFMpegArguments
                .FromFileInput(videoFile)
                .AddFileInput(audioFile)
                .OutputToFile(outputFile, true, options => options
                    .WithCustomArgument($"-map 0:v -map 1:a {offsetArg} {filterArg} -c:v copy -shortest")
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Replacing audio...");
                }, videoInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Audio replaced!");
                return Success("Audio replaced successfully",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Audio replacement failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error replacing audio: {ex.Message}", ex);
        }
    }
}
