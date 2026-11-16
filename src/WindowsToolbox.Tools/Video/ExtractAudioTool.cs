using FFMpegCore;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class ExtractAudioTool : BaseTool
{
    public override string Id => "extract-audio";
    public override string Name => "Extract Audio";
    public override string Description => "Extract audio track from video files";
    public override string Category => "video";
    public override string Icon => "MusicNote";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFormat = parameters.Get<string>("outputFormat") ?? "mp3"; // mp3, aac, wav, flac, ogg
            var bitrate = parameters.Get<int?>("bitrate") ?? 192; // kbps

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}.{outputFormat}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            if (mediaInfo.PrimaryAudioStream == null)
                return Failure("No audio stream found in video");

            ReportProgress(progress, 10, "Extracting audio...");

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options =>
                {
                    options.WithCustomArgument("-vn"); // No video
                    
                    if (outputFormat.ToLower() == "mp3")
                        options.WithAudioCodec(AudioCodec.LibMp3Lame).WithAudioBitrate(bitrate);
                    else if (outputFormat.ToLower() == "aac")
                        options.WithAudioCodec(AudioCodec.Aac).WithAudioBitrate(bitrate);
                    else if (outputFormat.ToLower() == "wav")
                        options.WithAudioCodec(AudioCodec.LibWavPack);
                    else if (outputFormat.ToLower() == "flac")
                        options.WithCustomArgument("-c:a flac");
                    else if (outputFormat.ToLower() == "ogg")
                        options.WithAudioCodec(AudioCodec.LibVorbis).WithAudioBitrate(bitrate);
                    else
                        options.WithAudioCodec(AudioCodec.LibMp3Lame).WithAudioBitrate(bitrate);
                })
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Extracting audio...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                var fileSize = new FileInfo(outputFile).Length / 1024.0 / 1024.0;
                ReportProgress(progress, 100, "Audio extraction complete!");
                return Success($"Audio extracted successfully ({fileSize:F2}MB)",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Audio extraction failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error extracting audio: {ex.Message}", ex);
        }
    }
}
