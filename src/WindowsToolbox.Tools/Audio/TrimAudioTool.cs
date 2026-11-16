using FFMpegCore;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Audio;

public class TrimAudioTool : BaseTool
{
    public override string Id => "trim-audio";
    public override string Name => "Trim Audio";
    public override string Description => "Trim audio files with precision";
    public override string Category => "audio";
    public override string Icon => "Cut";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFile = parameters.Get<string>("outputFile");
            var startTime = parameters.Get<TimeSpan>("startTime") ?? TimeSpan.Zero;
            var endTime = parameters.Get<TimeSpan?>("endTime");
            var fadeIn = parameters.Get<bool>("fadeIn") ?? false;
            var fadeOut = parameters.Get<bool>("fadeOut") ?? false;

            if (string.IsNullOrWhiteSpace(inputFile))
                return Failure("Input file is required");

            if (string.IsNullOrWhiteSpace(outputFile))
                return Failure("Output file is required");

            if (!File.Exists(inputFile))
                return Failure("Input file does not exist");

            ReportProgress(progress, 10, "Analyzing audio...");

            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            var duration = endTime ?? mediaInfo.Duration;

            if (startTime >= duration)
                return Failure("Start time must be less than duration");

            ReportProgress(progress, 30, "Trimming audio...");

            var filters = new List<string>();
            
            if (fadeIn)
                filters.Add("afade=t=in:st=0:d=2");
            
            if (fadeOut)
            {
                var fadeOutStart = (duration - startTime).TotalSeconds - 2;
                if (fadeOutStart > 0)
                    filters.Add($"afade=t=out:st={fadeOutStart}:d=2");
            }

            var arguments = FFMpegArguments
                .FromFileInput(inputFile, true, options => options
                    .Seek(startTime))
                .OutputToFile(outputFile, true, options =>
                {
                    var opts = options.WithDuration(duration - startTime);
                    
                    if (filters.Any())
                        opts = opts.WithCustomArgument($"-af \"{string.Join(",", filters)}\"");
                    
                    return opts.CopyChannel();
                });

            await arguments.CancellableThrough(cancellationToken).ProcessAsynchronously();

            ReportProgress(progress, 100, "Audio trimmed successfully");

            return Success("Audio trimmed successfully", new Dictionary<string, object>
            {
                ["outputFile"] = outputFile,
                ["duration"] = (duration - startTime).ToString()
            });
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to trim audio: {ex.Message}", ex);
        }
    }
}
