using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;
using Color = SixLabors.ImageSharp.Color;
using PointF = SixLabors.ImageSharp.PointF;

namespace WindowsToolbox.Tools.Image;

public class WatermarkTool : BaseTool
{
    public override string Id => "watermark";
    public override string Name => "Watermark";
    public override string Description => "Add text or image watermarks to images";
    public override string Category => "image";
    public override string Icon => "Badge";

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

            var watermarkType = parameters.Get<string>("watermarkType") ?? "text";
            var watermarkText = parameters.Get<string>("watermarkText");
            var watermarkImagePath = parameters.Get<string>("watermarkImagePath");
            var position = parameters.Get<string>("position") ?? "bottomRight";
            var fontSize = parameters.Get<int>("fontSize") > 0 ? parameters.Get<int>("fontSize") : 24;
            var color = parameters.Get<string>("color") ?? "#FFFFFF";
            var opacity = parameters.Get<float>("opacity") > 0 ? parameters.Get<float>("opacity") : 0.5f;
            var rotation = parameters.Get<float>("rotation");
            var tiled = parameters.Get<bool>("tiled");
            var outputDirectory = parameters.Get<string>("outputDirectory") ?? Path.GetDirectoryName(inputPaths[0]);

            var results = new List<string>();
            int processedCount = 0;

            Image<Rgba32>? watermarkImage = null;
            if (watermarkType == "image" && !string.IsNullOrEmpty(watermarkImagePath) && File.Exists(watermarkImagePath))
            {
                watermarkImage = await Image.LoadAsync<Rgba32>(watermarkImagePath, cancellationToken);
            }

            try
            {
                foreach (var inputPath in inputPaths)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    if (!File.Exists(inputPath))
                    {
                        ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Skipping missing file: {Path.GetFileName(inputPath)}");
                        continue;
                    }

                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Watermarking {Path.GetFileName(inputPath)}...", inputPath);

                    var outputPath = Path.Combine(
                        outputDirectory ?? "", 
                        Path.GetFileNameWithoutExtension(inputPath) + "_watermarked" + Path.GetExtension(inputPath)
                    );

                    try
                    {
                        using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                        {
                            image.Mutate(ctx =>
                            {
                                if (watermarkType == "text" && !string.IsNullOrEmpty(watermarkText))
                                {
                                    ApplyTextWatermark(ctx, image, watermarkText, position, fontSize, color, opacity, rotation, tiled);
                                }
                                else if (watermarkType == "image" && watermarkImage != null)
                                {
                                    ApplyImageWatermark(ctx, image, watermarkImage, position, opacity, rotation, tiled);
                                }
                            });

                            await image.SaveAsync(outputPath, cancellationToken);
                            results.Add(outputPath);
                        }
                    }
                    catch (Exception ex)
                    {
                        ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error watermarking {Path.GetFileName(inputPath)}: {ex.Message}");
                    }

                    processedCount++;
                }
            }
            finally
            {
                watermarkImage?.Dispose();
            }

            ReportProgress(progress, 100, "Watermarking complete");

            return Success($"Successfully watermarked {results.Count} of {inputPaths.Count} images", new Dictionary<string, object>
            {
                ["outputPaths"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to apply watermark: {ex.Message}", ex);
        }
    }

    private void ApplyTextWatermark(IImageProcessingContext ctx, Image<Rgba32> image, string text, string position, int fontSize, string colorStr, float opacity, float rotation, bool tiled)
    {
        var color = ParseHexColor(colorStr);
        var watermarkColor = Color.FromRgba(color.R, color.G, color.B, (byte)(255 * opacity));

        var fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == "Arial") ?? SystemFonts.Families.First();
        var font = fontFamily.CreateFont(fontSize, FontStyle.Bold);

        if (tiled)
        {
            var textSize = TextMeasurer.MeasureSize(text, new RichTextOptions(font));
            var spacing = 100;

            for (int y = 0; y < image.Height; y += (int)textSize.Height + spacing)
            {
                for (int x = 0; x < image.Width; x += (int)textSize.Width + spacing)
                {
                    var textOptions = new RichTextOptions(font)
                    {
                        Origin = new PointF(x, y)
                    };

                    ctx.DrawText(textOptions, text, watermarkColor);
                }
            }
        }
        else
        {
            var textSize = TextMeasurer.MeasureSize(text, new RichTextOptions(font));
            var pos = GetPosition(position, image.Width, image.Height, (int)textSize.Width, (int)textSize.Height);

            var textOptions = new RichTextOptions(font)
            {
                Origin = pos
            };

            ctx.DrawText(textOptions, text, watermarkColor);
        }
    }

    private void ApplyImageWatermark(IImageProcessingContext ctx, Image<Rgba32> image, Image<Rgba32> watermark, string position, float opacity, float rotation, bool tiled)
    {
        var watermarkCopy = watermark.Clone(wCtx =>
        {
            if (rotation != 0)
            {
                wCtx.Rotate(rotation);
            }
        });

        if (tiled)
        {
            var spacing = 50;

            for (int y = 0; y < image.Height; y += watermarkCopy.Height + spacing)
            {
                for (int x = 0; x < image.Width; x += watermarkCopy.Width + spacing)
                {
                    ctx.DrawImage(watermarkCopy, new Point(x, y), opacity);
                }
            }
        }
        else
        {
            var pos = GetPosition(position, image.Width, image.Height, watermarkCopy.Width, watermarkCopy.Height);
            ctx.DrawImage(watermarkCopy, new Point((int)pos.X, (int)pos.Y), opacity);
        }

        watermarkCopy.Dispose();
    }

    private PointF GetPosition(string position, int imageWidth, int imageHeight, int watermarkWidth, int watermarkHeight)
    {
        var margin = 20;

        return position.ToLower() switch
        {
            "topleft" => new PointF(margin, margin),
            "topcenter" => new PointF((imageWidth - watermarkWidth) / 2, margin),
            "topright" => new PointF(imageWidth - watermarkWidth - margin, margin),
            "middleleft" => new PointF(margin, (imageHeight - watermarkHeight) / 2),
            "center" => new PointF((imageWidth - watermarkWidth) / 2, (imageHeight - watermarkHeight) / 2),
            "middleright" => new PointF(imageWidth - watermarkWidth - margin, (imageHeight - watermarkHeight) / 2),
            "bottomleft" => new PointF(margin, imageHeight - watermarkHeight - margin),
            "bottomcenter" => new PointF((imageWidth - watermarkWidth) / 2, imageHeight - watermarkHeight - margin),
            "bottomright" => new PointF(imageWidth - watermarkWidth - margin, imageHeight - watermarkHeight - margin),
            _ => new PointF(imageWidth - watermarkWidth - margin, imageHeight - watermarkHeight - margin)
        };
    }

    private static Color ParseHexColor(string hex)
    {
        hex = hex.TrimStart('#');
        if (hex.Length == 6)
        {
            var r = Convert.ToByte(hex.Substring(0, 2), 16);
            var g = Convert.ToByte(hex.Substring(2, 2), 16);
            var b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromRgb(r, g, b);
        }
        return Color.White;
    }
}
