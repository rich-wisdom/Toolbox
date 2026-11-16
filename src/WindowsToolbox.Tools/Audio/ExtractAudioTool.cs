using FFMpegCore;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class ExtractAudioTool : BaseTool
{
    public override string Id => "extract-audio";
    public override string Name => "Extract Audio";
    public override string Description => "Extract audio from video files";
    public override string Category => "audio";
    public override string Icon => "MusicNote";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFiles = parameters.Get<string[]>("inputFiles") ?? parameters.Get<string>("inputFile")?.Split(';');
            var outputFormat = parameters.Get<string>("outputFormat") ?? "mp3";
            var bitrate = parameters.Get<int>("bitrate") ?? 192;
            var trackIndex = parameters.Get<int>("trackIndex") ?? 0;

            if (inputFiles == null || inputFiles.Length == 0)
                return Failure("At least one input file is required");

            ReportProgress(progress, 5, "Starting extraction...");

            var results = new List<string>();
            var totalFiles = inputFiles.Length;

            for (int i = 0; i < totalFiles; i++)
            {
                var inputFile = inputFiles[i];
                
                if (!File.Exists(inputFile))
                {
                    ReportProgress(progress, (i + 1) * 100 / totalFiles, $"Skipping missing file: {Path.GetFileName(inputFile)}");
                    continue;
                }

                var outputFile = Path.ChangeExtension(inputFile, outputFormat);
                var baseProgress = i * 100 / totalFiles;

                ReportProgress(progress, baseProgress + 10, $"Extracting: {Path.GetFileName(inputFile)}", inputFile);

                var arguments = FFMpegArguments
                    .FromFileInput(inputFile)
                    .OutputToFile(outputFile, true, options =>
                    {
                        var opts = options.WithAudioCodec(outputFormat.ToLower() switch
                        {
                            "mp3" => AudioCodec.LibMp3Lame,
                            "aac" => AudioCodec.Aac,
                            "flac" => AudioCodec.Flac,
                            "wav" => AudioCodec.Pcm16,
                            "ogg" => AudioCodec.LibVorbis,
                            "m4a" => AudioCodec.Aac,
                            _ => AudioCodec.LibMp3Lame
                        });

                        if (outputFormat.ToLower() != "flac" && outputFormat.ToLower() != "wav")
                            opts = opts.WithAudioBitrate(bitrate);

                        return opts.DisableChannel(Channel.Video);
                    });

                await arguments.CancellableThrough(cancellationToken).ProcessAsynchronously();

                results.Add(outputFile);
                ReportProgress(progress, (i + 1) * 100 / totalFiles, $"Extracted: {Path.GetFileName(outputFile)}");
            }

            return Success($"Extracted audio from {results.Count} file(s)", new Dictionary<string, object>
            {
                ["outputFiles"] = results.ToArray(),
                ["count"] = results.Count
            });
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to extract audio: {ex.Message}", ex);
        }
    }
}
