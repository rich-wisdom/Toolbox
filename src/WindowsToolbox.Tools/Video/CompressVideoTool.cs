using FFMpegCore;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class CompressVideoTool : BaseTool
{
    public override string Id => "compress-video";
    public override string Name => "Compress Video";
    public override string Description => "Reduce video file size while maintaining quality";
    public override string Category => "video";
    public override string Icon => "Video";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFile = parameters.Get<string>("outputFile");
            var quality = parameters.Get<string>("quality") ?? "medium"; // high, medium, low
            var targetSizeMB = parameters.Get<int?>("targetSizeMB");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            if (string.IsNullOrEmpty(outputFile))
                outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!, 
                    $"{Path.GetFileNameWithoutExtension(inputFile)}_compressed{Path.GetExtension(inputFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Compressing video...");

            // CRF values: 18-28 (lower = better quality, higher file size)
            var crf = quality switch
            {
                "high" => 20,
                "low" => 28,
                _ => 23
            };

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                    .WithVideoCodec(VideoCodec.LibX264)
                    .WithConstantRateFactor(crf)
                    .WithAudioCodec(AudioCodec.Aac)
                    .WithAudioBitrate(128)
                    .WithFastStart()
                    .WithVariableBitrate(2))
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Compressing...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                var originalSize = new FileInfo(inputFile).Length / 1024.0 / 1024.0;
                var compressedSize = new FileInfo(outputFile).Length / 1024.0 / 1024.0;
                var savedPercent = ((originalSize - compressedSize) / originalSize) * 100;

                ReportProgress(progress, 100, "Compression complete!");
                return Success($"Video compressed successfully. Saved {savedPercent:F1}% ({originalSize:F2}MB → {compressedSize:F2}MB)",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Compression failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error compressing video: {ex.Message}", ex);
        }
    }
}
