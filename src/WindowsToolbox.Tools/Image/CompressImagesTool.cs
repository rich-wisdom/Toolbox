using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;

namespace WindowsToolbox.Tools.Image;

public class CompressImagesTool : BaseTool
{
    public override string Id => "compress-images";
    public override string Name => "Compress Images";
    public override string Description => "Reduce image file size while maintaining quality";
    public override string Category => "image";
    public override string Icon => "Zoom";

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

            var compressionMode = parameters.Get<string>("compressionMode") ?? "quality";
            var quality = parameters.Get<int>("quality") > 0 ? parameters.Get<int>("quality") : 80;
            var targetSizeKb = parameters.Get<int>("targetSizeKb");
            var resizePercent = parameters.Get<int>("resizePercent");
            var stripMetadata = parameters.Get<bool>("stripMetadata");
            var outputDirectory = parameters.Get<string>("outputDirectory") ?? Path.GetDirectoryName(inputPaths[0]);

            var results = new List<CompressionResult>();
            int processedCount = 0;
            long totalOriginalSize = 0;
            long totalCompressedSize = 0;

            foreach (var inputPath in inputPaths)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                if (!File.Exists(inputPath))
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Skipping missing file: {Path.GetFileName(inputPath)}");
                    continue;
                }

                var originalSize = new FileInfo(inputPath).Length;
                totalOriginalSize += originalSize;

                ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Compressing {Path.GetFileName(inputPath)}...", inputPath);

                var outputPath = Path.Combine(
                    outputDirectory ?? "", 
                    Path.GetFileNameWithoutExtension(inputPath) + "_compressed" + Path.GetExtension(inputPath)
                );

                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        if (stripMetadata)
                        {
                            image.Metadata.ExifProfile = null;
                            image.Metadata.IccProfile = null;
                            image.Metadata.XmpProfile = null;
                        }

                        if (resizePercent > 0 && resizePercent < 100)
                        {
                            var newWidth = (int)(image.Width * resizePercent / 100.0);
                            var newHeight = (int)(image.Height * resizePercent / 100.0);
                            image.Mutate(x => x.Resize(newWidth, newHeight));
                        }

                        if (compressionMode == "targetSize" && targetSizeKb > 0)
                        {
                            await CompressToTargetSize(image, outputPath, targetSizeKb, cancellationToken);
                        }
                        else
                        {
                            var extension = Path.GetExtension(inputPath).ToLower();
                            var encoder = extension switch
                            {
                                ".jpg" or ".jpeg" => (IImageEncoder)new JpegEncoder { Quality = quality },
                                ".webp" => new WebpEncoder { Quality = quality },
                                ".png" => new PngEncoder { CompressionLevel = PngCompressionLevel.BestCompression },
                                _ => new JpegEncoder { Quality = quality }
                            };

                            await image.SaveAsync(outputPath, encoder, cancellationToken);
                        }

                        var compressedSize = new FileInfo(outputPath).Length;
                        totalCompressedSize += compressedSize;

                        results.Add(new CompressionResult
                        {
                            InputPath = inputPath,
                            OutputPath = outputPath,
                            OriginalSize = originalSize,
                            CompressedSize = compressedSize,
                            SavingsPercent = (int)((originalSize - compressedSize) * 100.0 / originalSize)
                        });
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error compressing {Path.GetFileName(inputPath)}: {ex.Message}");
                }

                processedCount++;
            }

            ReportProgress(progress, 100, "Image compression complete");

            var totalSavingsPercent = totalOriginalSize > 0 
                ? (int)((totalOriginalSize - totalCompressedSize) * 100.0 / totalOriginalSize)
                : 0;

            return Success($"Successfully compressed {results.Count} of {inputPaths.Count} images. Saved {totalSavingsPercent}% ({FormatBytes(totalOriginalSize - totalCompressedSize)})", new Dictionary<string, object>
            {
                ["results"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count,
                ["totalOriginalSize"] = totalOriginalSize,
                ["totalCompressedSize"] = totalCompressedSize,
                ["totalSavingsPercent"] = totalSavingsPercent
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to compress images: {ex.Message}", ex);
        }
    }

    private async Task CompressToTargetSize(Image<Rgba32> image, string outputPath, int targetSizeKb, CancellationToken cancellationToken)
    {
        var targetSizeBytes = targetSizeKb * 1024;
        int quality = 95;
        int minQuality = 10;

        while (quality >= minQuality)
        {
            var encoder = new JpegEncoder { Quality = quality };
            await image.SaveAsync(outputPath, encoder, cancellationToken);

            var fileSize = new FileInfo(outputPath).Length;

            if (fileSize <= targetSizeBytes)
                break;

            quality -= 5;
        }
    }

    private string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }

        return $"{len:0.##} {sizes[order]}";
    }
}

public class CompressionResult
{
    public string InputPath { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public long OriginalSize { get; set; }
    public long CompressedSize { get; set; }
    public int SavingsPercent { get; set; }
}
