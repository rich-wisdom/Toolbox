using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Drawing;
using Color = SixLabors.ImageSharp.Color;

namespace WindowsToolbox.Tools.Image;

public class BeautifyScreenshotTool : BaseTool
{
    public override string Id => "beautify-screenshot";
    public override string Name => "Beautify Screenshot";
    public override string Description => "Add beautiful backgrounds, shadows, and styling to screenshots";
    public override string Category => "image";
    public override string Icon => "ImageEdit";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputPath = parameters.Get<string>("inputPath");
            var outputPath = parameters.Get<string>("outputPath") ?? Path.ChangeExtension(inputPath, "_beautified.png");
            var backgroundType = parameters.Get<string>("backgroundType") ?? "gradient";
            var backgroundColor = parameters.Get<string>("backgroundColor") ?? "#667eea";
            var padding = parameters.Get<int>("padding") != 0 ? parameters.Get<int>("padding") : 60;
            var cornerRadius = parameters.Get<int>("cornerRadius") != 0 ? parameters.Get<int>("cornerRadius") : 16;
            var shadowEnabled = parameters.Get<bool>("shadowEnabled") != false;
            var shadowOffsetX = parameters.Get<int>("shadowOffsetX") != 0 ? parameters.Get<int>("shadowOffsetX") : 0;
            var shadowOffsetY = parameters.Get<int>("shadowOffsetY") != 0 ? parameters.Get<int>("shadowOffsetY") : 20;
            var shadowBlur = parameters.Get<int>("shadowBlur") != 0 ? parameters.Get<int>("shadowBlur") : 40;
            var aspectRatio = parameters.Get<string>("aspectRatio");

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
                return Failure("Input image path is required and must exist");

            ReportProgress(progress, 0, "Loading image...");

            using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
            {
                ReportProgress(progress, 20, "Applying beautification...");

                int originalWidth = image.Width;
                int originalHeight = image.Height;

                int canvasWidth = originalWidth + (padding * 2);
                int canvasHeight = originalHeight + (padding * 2);

                if (!string.IsNullOrEmpty(aspectRatio))
                {
                    var parts = aspectRatio.Split(':');
                    if (parts.Length == 2 && int.TryParse(parts[0], out var widthRatio) && int.TryParse(parts[1], out var heightRatio))
                    {
                        float targetRatio = (float)widthRatio / heightRatio;
                        float currentRatio = (float)canvasWidth / canvasHeight;

                        if (currentRatio > targetRatio)
                        {
                            canvasHeight = (int)(canvasWidth / targetRatio);
                        }
                        else
                        {
                            canvasWidth = (int)(canvasHeight * targetRatio);
                        }
                    }
                }

                ReportProgress(progress, 40, "Creating background...");

                using (var canvas = new Image<Rgba32>(canvasWidth, canvasHeight))
                {
                    canvas.Mutate(ctx =>
                    {
                        var bgColor = ParseHexColor(backgroundColor);

                        if (backgroundType == "gradient")
                        {
                            var color2 = ParseHexColor("#764ba2");
                            ctx.Fill(new LinearGradientBrush(
                                new PointF(0, 0),
                                new PointF(canvasWidth, canvasHeight),
                                GradientRepetitionMode.None,
                                new ColorStop(0, bgColor),
                                new ColorStop(1, color2)));
                        }
                        else
                        {
                            ctx.BackgroundColor(bgColor);
                        }

                        int xPos = (canvasWidth - originalWidth) / 2;
                        int yPos = (canvasHeight - originalHeight) / 2;

                        if (shadowEnabled)
                        {
                            var shadowImage = image.Clone(img => img.GaussianBlur(shadowBlur / 4));
                            ctx.DrawImage(shadowImage, new Point(xPos + shadowOffsetX, yPos + shadowOffsetY), 0.3f);
                            shadowImage.Dispose();
                        }

                        ctx.DrawImage(image, new Point(xPos, yPos), 1f);
                    });

                    ReportProgress(progress, 80, "Saving image...");

                    await canvas.SaveAsPngAsync(outputPath, cancellationToken);
                }

                ReportProgress(progress, 100, "Beautification complete");

                return Success("Screenshot beautified successfully", new Dictionary<string, object>
                {
                    ["outputPath"] = outputPath,
                    ["originalSize"] = $"{originalWidth}x{originalHeight}",
                    ["canvasSize"] = $"{canvasWidth}x{canvasHeight}"
                });
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to beautify screenshot: {ex.Message}", ex);
        }
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
