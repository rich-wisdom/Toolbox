using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using Image = SixLabors.ImageSharp.Image;

#if WINDOWS
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
#endif

namespace WindowsToolbox.Tools.Image;

public class ScreenshotTool : BaseTool
{
    public override string Id => "screenshot";
    public override string Name => "Screenshot";
    public override string Description => "Capture screen, window, or region";
    public override string Category => "image";
    public override string Icon => "Camera";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
#if WINDOWS
        try
        {
            var captureMode = parameters.Get<string>("captureMode") ?? "fullscreen";
            var outputPath = parameters.Get<string>("outputPath") ?? Path.Combine(Path.GetTempPath(), $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png");
            var format = parameters.Get<string>("format") ?? "png";
            var includeCursor = parameters.Get<bool>("includeCursor");
            var delaySeconds = parameters.Get<int>("delaySeconds");
            var copyToClipboard = parameters.Get<bool>("copyToClipboard");

            ReportProgress(progress, 0, "Preparing to capture...");

            if (delaySeconds > 0)
            {
                ReportProgress(progress, 10, $"Waiting {delaySeconds} seconds...");
                await Task.Delay(delaySeconds * 1000, cancellationToken);
            }

            ReportProgress(progress, 30, "Capturing screenshot...");

            Bitmap? bitmap = null;

            switch (captureMode.ToLower())
            {
                case "fullscreen":
                    bitmap = CaptureFullScreen();
                    break;
                case "region":
                    var x = parameters.Get<int>("x");
                    var y = parameters.Get<int>("y");
                    var width = parameters.Get<int>("width");
                    var height = parameters.Get<int>("height");
                    bitmap = CaptureRegion(x, y, width, height);
                    break;
                default:
                    return Failure($"Unsupported capture mode: {captureMode}");
            }

            if (bitmap == null)
                return Failure("Failed to capture screenshot");

            ReportProgress(progress, 60, "Processing image...");

            using (var memStream = new MemoryStream())
            {
                bitmap.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                memStream.Position = 0;

                using (var image = Image.Load<Rgba32>(memStream))
                {
                    ReportProgress(progress, 80, "Saving screenshot...");

                    var encoder = format.ToLower() switch
                    {
                        "jpg" or "jpeg" => (IImageEncoder)new JpegEncoder { Quality = 90 },
                        "bmp" => new BmpEncoder(),
                        _ => new PngEncoder()
                    };

                    await image.SaveAsync(outputPath, encoder, cancellationToken);
                }
            }

            bitmap.Dispose();

            if (copyToClipboard)
            {
                ReportProgress(progress, 90, "Copying to clipboard...");
                CopyImageToClipboard(outputPath);
            }

            ReportProgress(progress, 100, "Screenshot captured successfully");

            return Success("Screenshot captured successfully", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["format"] = format,
                ["fileSize"] = new FileInfo(outputPath).Length
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to capture screenshot: {ex.Message}", ex);
        }
#else
        return await Task.FromResult(Failure("Screenshot tool is only available on Windows"));
#endif
    }


#if WINDOWS
    private Bitmap CaptureFullScreen()
    {
        var bounds = System.Windows.Forms.Screen.PrimaryScreen?.Bounds ?? new System.Drawing.Rectangle(0, 0, 1920, 1080);
        var bitmap = new Bitmap(bounds.Width, bounds.Height);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(bounds.X, bounds.Y, 0, 0, bounds.Size);
        }

        return bitmap;
    }

    private Bitmap CaptureRegion(int x, int y, int width, int height)
    {
        var bitmap = new Bitmap(width, height);

        using (var graphics = Graphics.FromImage(bitmap))
        {
            graphics.CopyFromScreen(x, y, 0, 0, new System.Drawing.Size(width, height));
        }

        return bitmap;
    }

    private void CopyImageToClipboard(string imagePath)
    {
        var image = System.Drawing.Image.FromFile(imagePath);
        System.Windows.Forms.Clipboard.SetImage(image);
        image.Dispose();
    }
#endif
}
