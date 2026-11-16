using FFMpegCore;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class ConvertVideoTool : BaseTool
{
    public override string Id => "convert-video";
    public override string Name => "Convert Video";
    public override string Description => "Convert videos between different formats (MP4, MOV, AVI, MKV, WebM)";
    public override string Category => "video";
    public override string Icon => "VideoConvert";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFormat = parameters.Get<string>("outputFormat") ?? "mp4";
            var videoCodec = parameters.Get<string>("videoCodec") ?? "copy";
            var audioCodec = parameters.Get<string>("audioCodec") ?? "copy";

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}.{outputFormat}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Converting video...");

            var arguments = FFMpegArguments.FromFileInput(inputFile);
            
            var success = await arguments
                .OutputToFile(outputFile, true, options =>
                {
                    if (videoCodec != "copy")
                        options.WithVideoCodec(GetVideoCodec(videoCodec));
                    else
                        options.CopyChannel(Channel.Video);

                    if (audioCodec != "copy")
                        options.WithAudioCodec(GetAudioCodec(audioCodec));
                    else
                        options.CopyChannel(Channel.Audio);

                    options.WithFastStart();
                })
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Converting...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Conversion complete!");
                return Success($"Video converted to {outputFormat.ToUpper()} successfully",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Conversion failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error converting video: {ex.Message}", ex);
        }
    }

    private static string GetVideoCodec(string codec) => codec.ToLower() switch
    {
        "h264" => VideoCodec.LibX264,
        "h265" => VideoCodec.LibX265,
        "vp9" => VideoCodec.LibVpx,
        _ => VideoCodec.LibX264
    };

    private static string GetAudioCodec(string codec) => codec.ToLower() switch
    {
        "aac" => AudioCodec.Aac,
        "mp3" => AudioCodec.LibMp3Lame,
        "opus" => AudioCodec.LibOpus,
        _ => AudioCodec.Aac
    };
}
