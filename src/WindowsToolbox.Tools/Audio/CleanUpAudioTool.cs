using FFMpegCore;
using FFMpegCore.Arguments;
using FFMpegCore.Enums;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class CleanUpAudioTool : BaseTool
{
    public override string Id => "clean-up-audio";
    public override string Name => "Clean Up Audio";
    public override string Description => "Remove background noise from audio recordings";
    public override string Category => "audio";
    public override string Icon => "Audio";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFile = parameters.Get<string>("outputFile");
            var removeNoise = parameters.Get<bool>("removeNoise") ?? true;
            var normalize = parameters.Get<bool>("normalize") ?? true;
            var removeSilence = parameters.Get<bool>("removeSilence") ?? false;
            var highpassFilter = parameters.Get<int>("highpassFilter") ?? 80;

            if (string.IsNullOrWhiteSpace(inputFile))
                return Failure("Input file is required");

            if (string.IsNullOrWhiteSpace(outputFile))
                return Failure("Output file is required");

            if (!File.Exists(inputFile))
                return Failure("Input file does not exist");

            ReportProgress(progress, 10, "Analyzing audio...");

            var filters = new List<string>();

            if (highpassFilter > 0)
                filters.Add($"highpass=f={highpassFilter}");

            if (removeNoise)
                filters.Add("afftdn=nf=-25");

            if (normalize)
                filters.Add("loudnorm");

            if (removeSilence)
                filters.Add("silenceremove=1:0:-50dB");

            ReportProgress(progress, 30, "Applying filters...");

            var filterComplex = string.Join(",", filters);

            await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                    .WithAudioCodec(AudioCodec.LibMp3Lame)
                    .WithCustomArgument($"-af \"{filterComplex}\""))
                .CancellableThrough(cancellationToken)
                .ProcessAsynchronously();

            ReportProgress(progress, 100, "Audio cleaned successfully");

            return Success("Audio cleaned successfully", new Dictionary<string, object>
            {
                ["outputFile"] = outputFile
            });
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to clean audio: {ex.Message}", ex);
        }
    }
}
