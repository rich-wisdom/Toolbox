using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace WindowsToolbox.Tools.Image;

public class RedactImageTool : BaseTool
{
    public override string Id => "redact-image";
    public override string Name => "Redact Image";
    public override string Description => "Redact sensitive information from images";
    public override string Category => "image";
    public override string Icon => "ProtectedDocument";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputPath = parameters.Get<string>("inputPath");
            var outputPath = parameters.Get<string>("outputPath") ?? Path.ChangeExtension(inputPath, "_redacted.png");
            var redactions = parameters.Get<List<RedactionRegion>>("redactions") ?? new List<RedactionRegion>();

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
                return Failure("Input image path is required and must exist");

            if (redactions.Count == 0)
                return Failure("At least one redaction region is required");

            ReportProgress(progress, 0, "Loading image...");

            using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
            {
                ReportProgress(progress, 20, $"Applying {redactions.Count} redactions...");

                image.Mutate(ctx =>
                {
                    int redactionIndex = 0;
                    foreach (var redaction in redactions)
                    {
                        ReportProgress(progress, 20 + (70 * redactionIndex / redactions.Count), $"Applying redaction {redactionIndex + 1}/{redactions.Count}");

                        var rect = new Rectangle(redaction.X, redaction.Y, redaction.Width, redaction.Height);

                        switch (redaction.Style?.ToLower() ?? "blackout")
                        {
                            case "blur":
                                var blurAmount = redaction.Intensity > 0 ? redaction.Intensity : 20;
                                ctx.GaussianBlur(blurAmount / 2, rect);
                                break;

                            case "pixelate":
                                var pixelSize = redaction.Intensity > 0 ? redaction.Intensity : 10;
                                ctx.Pixelate(pixelSize, rect);
                                break;

                            case "blackout":
                                ctx.Fill(Color.Black, new RectangularPolygon(rect));
                                break;

                            case "color":
                                var color = ParseHexColor(redaction.Color ?? "#000000");
                                ctx.Fill(color, new RectangularPolygon(rect));
                                break;

                            case "whiteout":
                                ctx.Fill(Color.White, new RectangularPolygon(rect));
                                break;
                        }

                        redactionIndex++;
                    }
                });

                ReportProgress(progress, 90, "Saving redacted image...");

                await image.SaveAsPngAsync(outputPath, cancellationToken);

                ReportProgress(progress, 100, "Redaction complete");

                return Success("Image redacted successfully", new Dictionary<string, object>
                {
                    ["outputPath"] = outputPath,
                    ["redactionsApplied"] = redactions.Count
                });
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to redact image: {ex.Message}", ex);
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
        return Color.Black;
    }
}

public class RedactionRegion
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string? Style { get; set; }
    public string? Color { get; set; }
    public int Intensity { get; set; }
}
