using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class MergeVideosTool : BaseTool
{
    public override string Id => "merge-videos";
    public override string Name => "Merge Videos";
    public override string Description => "Concatenate multiple videos into one";
    public override string Category => "video";
    public override string Icon => "Merge";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFiles = parameters.Get<List<string>>("inputFiles");
            var outputFile = parameters.Get<string>("outputFile");

            if (inputFiles == null || inputFiles.Count < 2)
                return Failure("At least 2 input files are required");

            foreach (var file in inputFiles)
            {
                if (!File.Exists(file))
                    return Failure($"File not found: {file}");
            }

            if (string.IsNullOrEmpty(outputFile))
                outputFile = Path.Combine(Path.GetDirectoryName(inputFiles[0])!,
                    $"merged_{DateTime.Now:yyyyMMdd_HHmmss}.mp4");

            ReportProgress(progress, 0, "Analyzing videos...");
            
            FFmpegService.EnsureFFmpegPath();

            ReportProgress(progress, 10, "Merging videos...");

            var success = await FFMpegArguments
                .FromConcatInput(inputFiles)
                .OutputToFile(outputFile, true, options => options
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), $"Merging {inputFiles.Count} videos...");
                })
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Merge complete!");
                return Success($"Successfully merged {inputFiles.Count} videos",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Merge failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error merging videos: {ex.Message}", ex);
        }
    }
}
