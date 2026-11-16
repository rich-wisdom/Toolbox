using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.Fonts;

namespace WindowsToolbox.Tools.Image;

public class AnnotateScreenshotTool : BaseTool
{
    public override string Id => "annotate-screenshot";
    public override string Name => "Annotate Screenshot";
    public override string Description => "Add arrows, text, shapes, and highlights to images";
    public override string Category => "image";
    public override string Icon => "Edit";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputPath = parameters.Get<string>("inputPath");
            var outputPath = parameters.Get<string>("outputPath") ?? Path.ChangeExtension(inputPath, "_annotated.png");
            var annotations = parameters.Get<List<Annotation>>("annotations") ?? new List<Annotation>();

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
                return Failure("Input image path is required and must exist");

            ReportProgress(progress, 0, "Loading image...");

            using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
            {
                ReportProgress(progress, 20, "Applying annotations...");

                image.Mutate(ctx =>
                {
                    int annotationIndex = 0;
                    foreach (var annotation in annotations)
                    {
                        ReportProgress(progress, 20 + (60 * annotationIndex / annotations.Count), $"Adding annotation {annotationIndex + 1}/{annotations.Count}");

                        var color = ParseHexColor(annotation.Color ?? "#FF0000");
                        var thickness = annotation.Thickness > 0 ? annotation.Thickness : 3;

                        switch (annotation.Type?.ToLower())
                        {
                            case "rectangle":
                                ctx.Draw(color, thickness, new RectangularPolygon(annotation.X, annotation.Y, annotation.Width, annotation.Height));
                                break;

                            case "circle":
                                ctx.Draw(color, thickness, new EllipsePolygon(annotation.X + annotation.Width / 2, annotation.Y + annotation.Height / 2, annotation.Width / 2, annotation.Height / 2));
                                break;

                            case "arrow":
                                DrawArrow(ctx, annotation.X, annotation.Y, annotation.EndX, annotation.EndY, color, thickness);
                                break;

                            case "line":
                                ctx.DrawLines(color, thickness, new PointF(annotation.X, annotation.Y), new PointF(annotation.EndX, annotation.EndY));
                                break;

                            case "text":
                                DrawText(ctx, annotation.Text ?? "", annotation.X, annotation.Y, color, annotation.FontSize > 0 ? annotation.FontSize : 16);
                                break;

                            case "highlight":
                                var highlightColor = Color.FromRgba(color.R, color.G, color.B, 80);
                                ctx.Fill(highlightColor, new RectangularPolygon(annotation.X, annotation.Y, annotation.Width, annotation.Height));
                                break;

                            case "blur":
                                var blurRegion = new Rectangle(annotation.X, annotation.Y, annotation.Width, annotation.Height);
                                ctx.GaussianBlur(10, blurRegion);
                                break;

                            case "pixelate":
                                var pixelateRegion = new Rectangle(annotation.X, annotation.Y, annotation.Width, annotation.Height);
                                ctx.Pixelate(10, pixelateRegion);
                                break;
                        }

                        annotationIndex++;
                    }
                });

                ReportProgress(progress, 80, "Saving annotated image...");

                await image.SaveAsPngAsync(outputPath, cancellationToken);

                ReportProgress(progress, 100, "Annotation complete");

                return Success("Image annotated successfully", new Dictionary<string, object>
                {
                    ["outputPath"] = outputPath,
                    ["annotationsApplied"] = annotations.Count
                });
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to annotate image: {ex.Message}", ex);
        }
    }

    private void DrawArrow(IImageProcessingContext ctx, float x1, float y1, float x2, float y2, Color color, float thickness)
    {
        ctx.DrawLines(color, thickness, new PointF(x1, y1), new PointF(x2, y2));

        var angle = Math.Atan2(y2 - y1, x2 - x1);
        var arrowLength = 15;
        var arrowAngle = Math.PI / 6;

        var point1 = new PointF(
            x2 - (float)(arrowLength * Math.Cos(angle - arrowAngle)),
            y2 - (float)(arrowLength * Math.Sin(angle - arrowAngle))
        );

        var point2 = new PointF(
            x2 - (float)(arrowLength * Math.Cos(angle + arrowAngle)),
            y2 - (float)(arrowLength * Math.Sin(angle + arrowAngle))
        );

        ctx.DrawLines(color, thickness, new PointF(x2, y2), point1);
        ctx.DrawLines(color, thickness, new PointF(x2, y2), point2);
    }

    private void DrawText(IImageProcessingContext ctx, string text, float x, float y, Color color, float fontSize)
    {
        try
        {
            var fontFamily = SystemFonts.Families.FirstOrDefault(f => f.Name == "Arial") ?? SystemFonts.Families.First();
            var font = fontFamily.CreateFont(fontSize, FontStyle.Regular);

            var textOptions = new RichTextOptions(font)
            {
                Origin = new PointF(x, y),
                WrappingLength = -1,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top
            };

            ctx.DrawText(textOptions, text, color);
        }
        catch
        {
            // Fallback if font rendering fails
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
        return Color.Red;
    }
}

public class Annotation
{
    public string? Type { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    public float EndX { get; set; }
    public float EndY { get; set; }
    public string? Color { get; set; }
    public float Thickness { get; set; }
    public string? Text { get; set; }
    public float FontSize { get; set; }
}
