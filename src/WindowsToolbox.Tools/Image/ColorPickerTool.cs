using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace WindowsToolbox.Tools.Image;

public class ColorPickerTool : BaseTool
{
    public override string Id => "color-picker";
    public override string Name => "Color Picker";
    public override string Description => "Pick colors from images and get color values in multiple formats";
    public override string Category => "image";
    public override string Icon => "Color";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputPath = parameters.Get<string>("inputPath");
            var x = parameters.Get<int>("x");
            var y = parameters.Get<int>("y");
            var extractPalette = parameters.Get<bool>("extractPalette");
            var paletteColors = parameters.Get<int>("paletteColors") > 0 ? parameters.Get<int>("paletteColors") : 5;

            if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
                return Failure("Input image path is required and must exist");

            ReportProgress(progress, 0, "Loading image...");

            using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
            {
                ReportProgress(progress, 30, "Picking color...");

                if (x < 0 || x >= image.Width || y < 0 || y >= image.Height)
                    return Failure($"Coordinates ({x}, {y}) are outside image bounds ({image.Width}x{image.Height})");

                var pixel = image[x, y];
                var colorInfo = GetColorInfo(pixel);

                var result = new Dictionary<string, object>
                {
                    ["hex"] = colorInfo.Hex,
                    ["rgb"] = colorInfo.Rgb,
                    ["hsl"] = colorInfo.Hsl,
                    ["hsv"] = colorInfo.Hsv,
                    ["cmyk"] = colorInfo.Cmyk,
                    ["position"] = new { x, y }
                };

                if (extractPalette)
                {
                    ReportProgress(progress, 60, "Extracting color palette...");
                    var palette = ExtractColorPalette(image, paletteColors);
                    result["palette"] = palette;
                }

                ReportProgress(progress, 100, "Color picked successfully");

                return Success("Color picked successfully", result);
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to pick color: {ex.Message}", ex);
        }
    }

    private ColorInfo GetColorInfo(Rgba32 pixel)
    {
        var r = pixel.R;
        var g = pixel.G;
        var b = pixel.B;

        var hex = $"#{r:X2}{g:X2}{b:X2}";
        var rgb = $"rgb({r}, {g}, {b})";

        var (h, s, l) = RgbToHsl(r, g, b);
        var hsl = $"hsl({h:F0}, {s:F0}%, {l:F0}%)";

        var (hv, sv, v) = RgbToHsv(r, g, b);
        var hsv = $"hsv({hv:F0}, {sv:F0}%, {v:F0}%)";

        var (c, m, y, k) = RgbToCmyk(r, g, b);
        var cmyk = $"cmyk({c:F0}%, {m:F0}%, {y:F0}%, {k:F0}%)";

        return new ColorInfo
        {
            Hex = hex,
            Rgb = rgb,
            Hsl = hsl,
            Hsv = hsv,
            Cmyk = cmyk
        };
    }

    private List<ColorInfo> ExtractColorPalette(Image<Rgba32> image, int colorCount)
    {
        var colorFrequency = new Dictionary<Rgba32, int>();

        var sampleStep = Math.Max(1, image.Width * image.Height / 10000);

        for (int y = 0; y < image.Height; y += Math.Max(1, image.Height / 100))
        {
            for (int x = 0; x < image.Width; x += Math.Max(1, image.Width / 100))
            {
                var pixel = image[x, y];
                var quantized = new Rgba32(
                    (byte)(pixel.R / 32 * 32),
                    (byte)(pixel.G / 32 * 32),
                    (byte)(pixel.B / 32 * 32),
                    255
                );

                if (!colorFrequency.ContainsKey(quantized))
                    colorFrequency[quantized] = 0;
                
                colorFrequency[quantized]++;
            }
        }

        var topColors = colorFrequency
            .OrderByDescending(kvp => kvp.Value)
            .Take(colorCount)
            .Select(kvp => GetColorInfo(kvp.Key))
            .ToList();

        return topColors;
    }

    private (double h, double s, double l) RgbToHsl(byte r, byte g, byte b)
    {
        double rf = r / 255.0;
        double gf = g / 255.0;
        double bf = b / 255.0;

        double max = Math.Max(rf, Math.Max(gf, bf));
        double min = Math.Min(rf, Math.Min(gf, bf));
        double delta = max - min;

        double h = 0, s = 0, l = (max + min) / 2;

        if (delta != 0)
        {
            s = l > 0.5 ? delta / (2 - max - min) : delta / (max + min);

            if (max == rf)
                h = ((gf - bf) / delta + (gf < bf ? 6 : 0)) / 6;
            else if (max == gf)
                h = ((bf - rf) / delta + 2) / 6;
            else
                h = ((rf - gf) / delta + 4) / 6;
        }

        return (h * 360, s * 100, l * 100);
    }

    private (double h, double s, double v) RgbToHsv(byte r, byte g, byte b)
    {
        double rf = r / 255.0;
        double gf = g / 255.0;
        double bf = b / 255.0;

        double max = Math.Max(rf, Math.Max(gf, bf));
        double min = Math.Min(rf, Math.Min(gf, bf));
        double delta = max - min;

        double h = 0, s = max == 0 ? 0 : delta / max, v = max;

        if (delta != 0)
        {
            if (max == rf)
                h = ((gf - bf) / delta + (gf < bf ? 6 : 0)) / 6;
            else if (max == gf)
                h = ((bf - rf) / delta + 2) / 6;
            else
                h = ((rf - gf) / delta + 4) / 6;
        }

        return (h * 360, s * 100, v * 100);
    }

    private (double c, double m, double y, double k) RgbToCmyk(byte r, byte g, byte b)
    {
        double rf = r / 255.0;
        double gf = g / 255.0;
        double bf = b / 255.0;

        double k = 1 - Math.Max(rf, Math.Max(gf, bf));
        
        if (k == 1)
            return (0, 0, 0, 100);

        double c = (1 - rf - k) / (1 - k);
        double m = (1 - gf - k) / (1 - k);
        double y = (1 - bf - k) / (1 - k);

        return (c * 100, m * 100, y * 100, k * 100);
    }
}

public class ColorInfo
{
    public string Hex { get; set; } = "";
    public string Rgb { get; set; } = "";
    public string Hsl { get; set; } = "";
    public string Hsv { get; set; } = "";
    public string Cmyk { get; set; } = "";
}
