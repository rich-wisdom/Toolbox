using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WindowsToolbox.Tools.Image;

public class RemoveBackgroundTool : BaseTool
{
    public override string Id => "remove-background";
    public override string Name => "Remove Background";
    public override string Description => "Remove image backgrounds using AI";
    public override string Category => "image";
    public override string Icon => "CropLandscape";

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

            var backgroundColor = parameters.Get<string>("backgroundColor") ?? "transparent";
            var cropToSubject = parameters.Get<bool>("cropToSubject");
            var outputDirectory = parameters.Get<string>("outputDirectory") ?? Path.GetDirectoryName(inputPaths[0]);

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

                var outputPath = Path.Combine(outputDirectory ?? "", Path.GetFileNameWithoutExtension(inputPath) + "_nobg.png");

                try
                {
                    using (var image = await SixLabors.ImageSharp.Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        image.Mutate(ctx =>
                        {
                            var bgColor = ParseBackgroundColor(backgroundColor);
                            
                            if (backgroundColor.ToLower() == "transparent")
                            {
                                ApplySimpleBackgroundRemoval(ctx, image);
                            }
                            else if (bgColor.HasValue)
                            {
                                ApplySimpleBackgroundRemoval(ctx, image);
                                ctx.BackgroundColor(bgColor.Value);
                            }

                            if (cropToSubject)
                            {
                                var bounds = FindContentBounds(image);
                                if (bounds.Width > 0 && bounds.Height > 0)
                                {
                                    ctx.Crop(bounds);
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

            ReportProgress(progress, 100, "Background removal complete");

            return Success($"Successfully processed {results.Count} of {inputPaths.Count} images", new Dictionary<string, object>
            {
                ["outputPaths"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to remove background: {ex.Message}", ex);
        }
    }

    private void ApplySimpleBackgroundRemoval(IImageProcessingContext ctx, Image<Rgba32> image)
    {
        var backgroundColor = DetectBackgroundColor(image);
        var tolerance = 30;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);

                for (int x = 0; x < pixelRow.Length; x++)
                {
                    ref var pixel = ref pixelRow[x];

                    if (IsColorSimilar(pixel, backgroundColor, tolerance))
                    {
                        pixel.A = 0;
                    }
                }
            }
        });
    }

    private Rgba32 DetectBackgroundColor(Image<Rgba32> image)
    {
        var corners = new List<Rgba32>
        {
            image[0, 0],
            image[image.Width - 1, 0],
            image[0, image.Height - 1],
            image[image.Width - 1, image.Height - 1]
        };

        return corners.GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .First()
            .Key;
    }

    private bool IsColorSimilar(Rgba32 color1, Rgba32 color2, int tolerance)
    {
        return Math.Abs(color1.R - color2.R) <= tolerance &&
               Math.Abs(color1.G - color2.G) <= tolerance &&
               Math.Abs(color1.B - color2.B) <= tolerance;
    }

    private Rectangle FindContentBounds(Image<Rgba32> image)
    {
        int minX = image.Width, minY = image.Height, maxX = 0, maxY = 0;
        bool foundContent = false;

        image.ProcessPixelRows(accessor =>
        {
            for (int y = 0; y < accessor.Height; y++)
            {
                var pixelRow = accessor.GetRowSpan(y);

                for (int x = 0; x < pixelRow.Length; x++)
                {
                    ref var pixel = ref pixelRow[x];

                    if (pixel.A > 0)
                    {
                        foundContent = true;
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }
        });

        if (!foundContent)
            return new Rectangle(0, 0, image.Width, image.Height);

        return new Rectangle(minX, minY, maxX - minX + 1, maxY - minY + 1);
    }

    private Color? ParseBackgroundColor(string colorStr)
    {
        if (colorStr.ToLower() == "transparent")
            return null;

        if (colorStr.ToLower() == "white")
            return Color.White;

        if (colorStr.ToLower() == "black")
            return Color.Black;

        if (colorStr.StartsWith("#"))
        {
            var hex = colorStr.TrimStart('#');
            if (hex.Length == 6)
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                return Color.FromRgb(r, g, b);
            }
        }

        return null;
    }
}
