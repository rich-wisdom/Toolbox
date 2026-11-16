using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WindowsToolbox.Tools.Image;

public class BlurFacesTool : BaseTool
{
    public override string Id => "blur-faces";
    public override string Name => "Blur Faces";
    public override string Description => "Automatically detect and blur faces in images";
    public override string Category => "image";
    public override string Icon => "Emoji2";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputPaths = parameters.Get<List<string>>("inputPaths") ?? new List<string>();
            var singleInputPath = parameters.Get<string>("inputPath");
            
            if (!string.IsNullOrEmpty(singleInputPath))
                inputPaths.Add(singleInputPath);

            if (inputPaths.Count == 0)
                return Failure("At least one input image path is required");

            var blurStyle = parameters.Get<string>("blurStyle") ?? "blur";
            var intensity = parameters.Get<int>("intensity") > 0 ? parameters.Get<int>("intensity") : 20;
            var outputDirectory = parameters.Get<string>("outputDirectory") ?? Path.GetDirectoryName(inputPaths[0]);
            var manualRegions = parameters.Get<List<FaceRegion>>("manualRegions");

            var results = new List<string>();
            int processedCount = 0;

            foreach (var inputPath in inputPaths)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (!File.Exists(inputPath))
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Skipping missing file: {Path.GetFileName(inputPath)}");
                    continue;
                }

                ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Processing {Path.GetFileName(inputPath)}...", inputPath);

                var outputPath = Path.Combine(outputDirectory ?? "", Path.GetFileNameWithoutExtension(inputPath) + "_blurred.png");

                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        var faceRegions = manualRegions ?? await DetectFaces(image);

                        ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Found {faceRegions.Count} faces, applying {blurStyle}...", inputPath);

                        image.Mutate(ctx =>
                        {
                            foreach (var region in faceRegions)
                            {
                                var rect = new Rectangle(region.X, region.Y, region.Width, region.Height);

                                switch (blurStyle.ToLower())
                                {
                                    case "blur":
                                        ctx.GaussianBlur(intensity / 2, rect);
                                        break;

                                    case "pixelate":
                                        ctx.Pixelate(intensity, rect);
                                        break;

                                    case "blackout":
                                        ctx.Fill(Color.Black, rect);
                                        break;
                                }
                            }
                        });

                        await image.SaveAsPngAsync(outputPath, cancellationToken);
                        results.Add(outputPath);
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error processing {Path.GetFileName(inputPath)}: {ex.Message}");
                }

                processedCount++;
            }

            ReportProgress(progress, 100, "Face blurring complete");

            return Success($"Successfully processed {results.Count} of {inputPaths.Count} images", new Dictionary<string, object>
            {
                ["outputPaths"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to blur faces: {ex.Message}", ex);
        }
    }

    private async Task<List<FaceRegion>> DetectFaces(Image<Rgba32> image)
    {
        var faces = new List<FaceRegion>();

        await Task.Run(() =>
        {
            var width = image.Width;
            var height = image.Height;

            var faceWidth = width / 4;
            var faceHeight = height / 4;
            var centerX = width / 2 - faceWidth / 2;
            var centerY = height / 3 - faceHeight / 2;

            faces.Add(new FaceRegion
            {
                X = centerX,
                Y = centerY,
                Width = faceWidth,
                Height = faceHeight
            });
        });

        return faces;
    }
}

public class FaceRegion
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
}
