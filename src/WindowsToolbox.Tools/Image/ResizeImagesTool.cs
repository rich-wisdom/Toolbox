using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WindowsToolbox.Tools.Image;

public class ResizeImagesTool : BaseTool
{
    public override string Id => "resize-images";
    public override string Name => "Resize Images";
    public override string Description => "Resize images by dimensions or percentage";
    public override string Category => "image";
    public override string Icon => "FullScreen";

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

            var resizeMode = parameters.Get<string>("resizeMode") ?? "percentage";
            var percentage = parameters.Get<int>("percentage") > 0 ? parameters.Get<int>("percentage") : 50;
            var width = parameters.Get<int>("width");
            var height = parameters.Get<int>("height");
            var maintainAspectRatio = parameters.Get<bool>("maintainAspectRatio") != false;
            var resamplingAlgorithm = parameters.Get<string>("resamplingAlgorithm") ?? "bicubic";
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

                ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Resizing {Path.GetFileName(inputPath)}...", inputPath);

                var outputPath = Path.Combine(
                    outputDirectory ?? "", 
                    Path.GetFileNameWithoutExtension(inputPath) + "_resized" + Path.GetExtension(inputPath)
                );

                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        int newWidth, newHeight;

                        if (resizeMode == "percentage")
                        {
                            newWidth = (int)(image.Width * percentage / 100.0);
                            newHeight = (int)(image.Height * percentage / 100.0);
                        }
                        else if (resizeMode == "dimensions")
                        {
                            if (maintainAspectRatio)
                            {
                                var aspectRatio = (double)image.Width / image.Height;

                                if (width > 0 && height > 0)
                                {
                                    var targetRatio = (double)width / height;
                                    if (aspectRatio > targetRatio)
                                    {
                                        newWidth = width;
                                        newHeight = (int)(width / aspectRatio);
                                    }
                                    else
                                    {
                                        newHeight = height;
                                        newWidth = (int)(height * aspectRatio);
                                    }
                                }
                                else if (width > 0)
                                {
                                    newWidth = width;
                                    newHeight = (int)(width / aspectRatio);
                                }
                                else if (height > 0)
                                {
                                    newHeight = height;
                                    newWidth = (int)(height * aspectRatio);
                                }
                                else
                                {
                                    return Failure("Width or height must be specified");
                                }
                            }
                            else
                            {
                                newWidth = width > 0 ? width : image.Width;
                                newHeight = height > 0 ? height : image.Height;
                            }
                        }
                        else
                        {
                            return Failure($"Unsupported resize mode: {resizeMode}");
                        }

                        var resampler = GetResampler(resamplingAlgorithm);

                        image.Mutate(x => x.Resize(new ResizeOptions
                        {
                            Size = new Size(newWidth, newHeight),
                            Sampler = resampler,
                            Mode = ResizeMode.Max
                        }));

                        await image.SaveAsync(outputPath, cancellationToken);
                        results.Add(outputPath);
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error resizing {Path.GetFileName(inputPath)}: {ex.Message}");
                }

                processedCount++;
            }

            ReportProgress(progress, 100, "Image resizing complete");

            return Success($"Successfully resized {results.Count} of {inputPaths.Count} images", new Dictionary<string, object>
            {
                ["outputPaths"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to resize images: {ex.Message}", ex);
        }
    }

    private IResampler GetResampler(string algorithm)
    {
        return algorithm.ToLower() switch
        {
            "nearest" or "nearestneighbor" => KnownResamplers.NearestNeighbor,
            "bilinear" => KnownResamplers.Triangle,
            "bicubic" => KnownResamplers.Bicubic,
            "lanczos" or "lanczos3" => KnownResamplers.Lanczos3,
            "lanczos5" => KnownResamplers.Lanczos5,
            "lanczos8" => KnownResamplers.Lanczos8,
            _ => KnownResamplers.Bicubic
        };
    }
}
