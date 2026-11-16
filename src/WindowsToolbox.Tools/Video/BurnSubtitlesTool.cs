using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class BurnSubtitlesTool : BaseTool
{
    public override string Id => "burn-subtitles";
    public override string Name => "Burn In Subtitles";
    public override string Description => "Embed subtitles permanently into video";
    public override string Category => "video";
    public override string Icon => "Caption";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var videoFile = parameters.Get<string>("videoFile");
            var subtitleFile = parameters.Get<string>("subtitleFile");
            var fontSize = parameters.Get<int?>("fontSize") ?? 24;
            var fontColor = parameters.Get<string>("fontColor") ?? "white";
            var fontName = parameters.Get<string>("fontName") ?? "Arial";

            if (string.IsNullOrEmpty(videoFile) || !File.Exists(videoFile))
                return Failure("Video file not found");

            if (string.IsNullOrEmpty(subtitleFile) || !File.Exists(subtitleFile))
                return Failure("Subtitle file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(videoFile)!,
                $"{Path.GetFileNameWithoutExtension(videoFile)}_subtitled{Path.GetExtension(videoFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(videoFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Burning subtitles...");

            // Escape path for FFmpeg
            var escapedSubPath = subtitleFile.Replace("\\", "/").Replace(":", "\\:");

            var subtitleFilter = $"subtitles='{escapedSubPath}':force_style='FontName={fontName},FontSize={fontSize},PrimaryColour={GetFFmpegColor(fontColor)}'";

            var success = await FFMpegArguments
                .FromFileInput(videoFile)
                .OutputToFile(outputFile, true, options => options
                    .WithCustomArgument($"-vf \"{subtitleFilter}\"")
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Burning subtitles...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Subtitles burned!");
                return Success("Subtitles burned successfully",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Subtitle burning failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error burning subtitles: {ex.Message}", ex);
        }
    }

    private static string GetFFmpegColor(string color) => color.ToLower() switch
    {
        "white" => "&HFFFFFF",
        "black" => "&H000000",
        "red" => "&H0000FF",
        "green" => "&H00FF00",
        "blue" => "&HFF0000",
        "yellow" => "&H00FFFF",
        _ => "&HFFFFFF"
    };
}
