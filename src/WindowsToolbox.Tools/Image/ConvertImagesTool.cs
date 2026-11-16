using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Tiff;
using SixLabors.ImageSharp.Formats.Webp;

namespace WindowsToolbox.Tools.Image;

public class ConvertImagesTool : BaseTool
{
    public override string Id => "convert-images";
    public override string Name => "Convert Images";
    public override string Description => "Convert images between different formats";
    public override string Category => "image";
    public override string Icon => "Switch";

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

            var targetFormat = parameters.Get<string>("targetFormat")?.ToLower() ?? "png";
            var quality = parameters.Get<int>("quality") > 0 ? parameters.Get<int>("quality") : 90;
            var preserveExif = parameters.Get<bool>("preserveExif") != false;
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

                ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Converting {Path.GetFileName(inputPath)}...", inputPath);

                var outputPath = Path.Combine(
                    outputDirectory ?? "", 
                    Path.GetFileNameWithoutExtension(inputPath) + $".{targetFormat}"
                );

                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        var encoder = GetEncoder(targetFormat, quality);

                        if (!preserveExif && image.Metadata.ExifProfile != null)
                        {
                            image.Metadata.ExifProfile = null;
                        }

                        await image.SaveAsync(outputPath, encoder, cancellationToken);
                        results.Add(outputPath);
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error converting {Path.GetFileName(inputPath)}: {ex.Message}");
                }

                processedCount++;
            }

            ReportProgress(progress, 100, "Image conversion complete");

            return Success($"Successfully converted {results.Count} of {inputPaths.Count} images to {targetFormat.ToUpper()}", new Dictionary<string, object>
            {
                ["outputPaths"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count,
                ["targetFormat"] = targetFormat.ToUpper()
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert images: {ex.Message}", ex);
        }
    }

    private IImageEncoder GetEncoder(string format, int quality)
    {
        return format.ToLower() switch
        {
            "jpg" or "jpeg" => new JpegEncoder { Quality = quality },
            "png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
            "bmp" => new BmpEncoder(),
            "gif" => new GifEncoder(),
            "tiff" or "tif" => new TiffEncoder(),
            "webp" => new WebpEncoder { Quality = quality },
            _ => new PngEncoder()
        };
    }
}
