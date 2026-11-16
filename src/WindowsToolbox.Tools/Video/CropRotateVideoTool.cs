using FFMpegCore;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Tools.Video.Services;

namespace WindowsToolbox.Tools.Video;

public class CropRotateVideoTool : BaseTool
{
    public override string Id => "crop-rotate-video";
    public override string Name => "Crop & Rotate Video";
    public override string Description => "Crop and rotate videos with visual control";
    public override string Category => "video";
    public override string Icon => "Crop";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var cropX = parameters.Get<int?>("cropX");
            var cropY = parameters.Get<int?>("cropY");
            var cropWidth = parameters.Get<int?>("cropWidth");
            var cropHeight = parameters.Get<int?>("cropHeight");
            var rotation = parameters.Get<int?>("rotation") ?? 0; // 0, 90, 180, 270
            var flipHorizontal = parameters.Get<bool?>("flipHorizontal") ?? false;
            var flipVertical = parameters.Get<bool?>("flipVertical") ?? false;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            var outputFile = Path.Combine(Path.GetDirectoryName(inputFile)!,
                $"{Path.GetFileNameWithoutExtension(inputFile)}_cropped{Path.GetExtension(inputFile)}");

            ReportProgress(progress, 0, "Analyzing video...");
            
            FFmpegService.EnsureFFmpegPath();
            var mediaInfo = await FFProbe.AnalyseAsync(inputFile, cancellationToken: cancellationToken);
            
            ReportProgress(progress, 10, "Processing video...");

            var videoStream = mediaInfo.PrimaryVideoStream;
            if (videoStream == null)
                return Failure("No video stream found");

            var filters = new List<string>();

            // Add crop filter if dimensions specified
            if (cropX.HasValue && cropY.HasValue && cropWidth.HasValue && cropHeight.HasValue)
            {
                filters.Add($"crop={cropWidth}:{cropHeight}:{cropX}:{cropY}");
            }

            // Add rotation filter
            if (rotation == 90)
                filters.Add("transpose=1");
            else if (rotation == 180)
                filters.Add("transpose=1,transpose=1");
            else if (rotation == 270)
                filters.Add("transpose=2");

            // Add flip filters
            if (flipHorizontal)
                filters.Add("hflip");
            if (flipVertical)
                filters.Add("vflip");

            if (filters.Count == 0)
                return Failure("No operations specified");

            var filterString = string.Join(",", filters);

            var success = await FFMpegArguments
                .FromFileInput(inputFile)
                .OutputToFile(outputFile, true, options => options
                    .WithCustomArgument($"-vf \"{filterString}\"")
                    .WithFastStart())
                .CancellableThrough(cancellationToken)
                .NotifyOnProgress(percentage =>
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.9), "Processing...");
                }, mediaInfo.Duration)
                .ProcessAsynchronously();

            if (success)
            {
                ReportProgress(progress, 100, "Processing complete!");
                return Success("Video processed successfully",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure("Processing failed");
        }
        catch (Exception ex)
        {
            return Failure($"Error processing video: {ex.Message}", ex);
        }
    }
}
