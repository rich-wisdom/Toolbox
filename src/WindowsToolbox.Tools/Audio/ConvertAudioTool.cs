using FFMpegCore;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class ConvertAudioTool : BaseTool
{
    public override string Id => "convert-audio";
    public override string Name => "Convert Audio";
    public override string Description => "Convert audio between different formats";
    public override string Category => "audio";
    public override string Icon => "Convert";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFiles = parameters.Get<string[]>("inputFiles") ?? parameters.Get<string>("inputFile")?.Split(';');
            var outputFormat = parameters.Get<string>("outputFormat") ?? "mp3";
            var bitrate = parameters.Get<int>("bitrate") ?? 192;
            var sampleRate = parameters.Get<int?>("sampleRate");
            var channels = parameters.Get<int?>("channels");
            var volumeAdjustment = parameters.Get<double?>("volumeAdjustment");
            var preserveMetadata = parameters.Get<bool>("preserveMetadata") ?? true;

            if (inputFiles == null || inputFiles.Length == 0)
                return Failure("At least one input file is required");

            ReportProgress(progress, 5, "Starting conversion...");

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
                if (outputFile == inputFile)
                    outputFile = Path.Combine(
                        Path.GetDirectoryName(inputFile) ?? "",
                        $"{Path.GetFileNameWithoutExtension(inputFile)}_converted.{outputFormat}");

                var baseProgress = i * 100 / totalFiles;

                ReportProgress(progress, baseProgress + 10, $"Converting: {Path.GetFileName(inputFile)}", inputFile);

                var audioCodec = outputFormat.ToLower() switch
                {
                    "mp3" => AudioCodec.LibMp3Lame,
                    "aac" => AudioCodec.Aac,
                    "flac" => AudioCodec.Flac,
                    "wav" => AudioCodec.Pcm16,
                    "ogg" => AudioCodec.LibVorbis,
                    "m4a" => AudioCodec.Aac,
                    "wma" => AudioCodec.Wmav2,
                    "aiff" => AudioCodec.Pcm16,
                    _ => AudioCodec.LibMp3Lame
                };

                var arguments = FFMpegArguments
                    .FromFileInput(inputFile)
                    .OutputToFile(outputFile, true, options =>
                    {
                        var opts = options.WithAudioCodec(audioCodec);

                        if (outputFormat.ToLower() != "flac" && outputFormat.ToLower() != "wav" && outputFormat.ToLower() != "aiff")
                            opts = opts.WithAudioBitrate(bitrate);

                        if (sampleRate.HasValue)
                            opts = opts.WithAudioSamplingRate(sampleRate.Value);

                        if (channels.HasValue)
                            opts = opts.WithCustomArgument($"-ac {channels.Value}");

                        if (volumeAdjustment.HasValue && volumeAdjustment.Value != 1.0)
                            opts = opts.WithCustomArgument($"-af volume={volumeAdjustment.Value}");

                        if (preserveMetadata)
                            opts = opts.WithCustomArgument("-map_metadata 0");

                        return opts;
                    });

                await arguments.CancellableThrough(cancellationToken).ProcessAsynchronously();

                results.Add(outputFile);
                ReportProgress(progress, (i + 1) * 100 / totalFiles, $"Converted: {Path.GetFileName(outputFile)}");
            }

            return Success($"Converted {results.Count} file(s) to {outputFormat.ToUpper()}", new Dictionary<string, object>
            {
                ["outputFiles"] = results.ToArray(),
                ["count"] = results.Count,
                ["format"] = outputFormat
            });
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert audio: {ex.Message}", ex);
        }
    }
}
