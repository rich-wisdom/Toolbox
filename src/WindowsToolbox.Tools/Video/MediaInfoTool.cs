using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;
using System.Text;
using System.Text.Json;

namespace WindowsToolbox.Tools.Video;

public class MediaInfoTool : BaseTool
{
    public override string Id => "media-info";
    public override string Name => "Media Info";
    public override string Description => "Display detailed information about video and audio files";
    public override string Category => "video";
    public override string Icon => "Info";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var exportJson = parameters.Get<bool?>("exportJson") ?? false;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            ReportProgress(progress, 0, "Analyzing media file...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 50, "Gathering information...");

            var info = new Dictionary<string, object>
            {
                ["FilePath"] = inputFile,
                ["FileName"] = Path.GetFileName(inputFile),
                ["FileSize"] = $"{new FileInfo(inputFile).Length / 1024.0 / 1024.0:F2} MB",
                ["Duration"] = mediaInfo.Duration.ToString(@"hh\:mm\:ss\.fff"),
                ["Format"] = mediaInfo.Format.FormatName,
                ["Bitrate"] = $"{mediaInfo.Format.BitRate / 1024.0:F0} kbps"
            };

            if (mediaInfo.PrimaryVideoStream != null)
            {
                var video = mediaInfo.PrimaryVideoStream;
                info["VideoCodec"] = video.CodecName;
                info["Resolution"] = $"{video.Width}x{video.Height}";
                info["FrameRate"] = $"{video.FrameRate:F2} fps";
                info["VideoBitrate"] = $"{video.BitRate / 1024.0:F0} kbps";
                info["AspectRatio"] = video.DisplayAspectRatio;
                info["PixelFormat"] = video.PixelFormat;
            }

            if (mediaInfo.PrimaryAudioStream != null)
            {
                var audio = mediaInfo.PrimaryAudioStream;
                info["AudioCodec"] = audio.CodecName;
                info["SampleRate"] = $"{audio.SampleRateHz} Hz";
                info["Channels"] = audio.Channels;
                info["AudioBitrate"] = $"{audio.BitRate / 1024.0:F0} kbps";
            }

            var infoText = new StringBuilder();
            infoText.AppendLine("Media Information");
            infoText.AppendLine("=================");
            foreach (var kvp in info)
            {
                infoText.AppendLine($"{kvp.Key}: {kvp.Value}");
            }

            if (exportJson)
            {
                var jsonFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                    $"{Path.GetFileNameWithoutExtension(inputFile)}_info.json");
                await File.WriteAllTextAsync(jsonFile, JsonSerializer.Serialize(info, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
                info["ExportedTo"] = jsonFile;
            }

            ReportProgress(progress, 100, "Analysis complete!");
            return Success(infoText.ToString(), info);
        }
        catch (Exception ex)
        {
            return Failure($"Error analyzing media: {ex.Message}", ex);
        }
    }
}
