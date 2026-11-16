using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace WindowsToolbox.Tools.Image;

public class RotateImagesTool : BaseTool
{
    public override string Id => "rotate-images";
    public override string Name => "Rotate Images";
    public override string Description => "Rotate and flip images";
    public override string Category => "image";
    public override string Icon => "Rotate";

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

            var rotationMode = parameters.Get<string>("rotationMode") ?? "90";
            var customAngle = parameters.Get<float>("customAngle");
            var flipMode = parameters.Get<string>("flipMode");
            var autoRotateFromExif = parameters.Get<bool>("autoRotateFromExif");
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

                ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Rotating {Path.GetFileName(inputPath)}...", inputPath);

                var outputPath = Path.Combine(
                    outputDirectory ?? "", 
                    Path.GetFileNameWithoutExtension(inputPath) + "_rotated" + Path.GetExtension(inputPath)
                );

                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        image.Mutate(ctx =>
                        {
                            if (autoRotateFromExif && image.Metadata.ExifProfile != null)
                            {
                                ctx.AutoOrient();
                            }
                            else
                            {
                                if (!string.IsNullOrEmpty(flipMode))
                                {
                                    switch (flipMode.ToLower())
                                    {
                                        case "horizontal":
                                            ctx.Flip(FlipMode.Horizontal);
                                            break;
                                        case "vertical":
                                            ctx.Flip(FlipMode.Vertical);
                                            break;
                                    }
                                }

                                if (!string.IsNullOrEmpty(rotationMode))
                                {
                                    switch (rotationMode.ToLower())
                                    {
                                        case "90":
                                            ctx.Rotate(RotateMode.Rotate90);
                                            break;
                                        case "180":
                                            ctx.Rotate(RotateMode.Rotate180);
                                            break;
                                        case "270":
                                            ctx.Rotate(RotateMode.Rotate270);
                                            break;
                                        case "custom":
                                            if (customAngle != 0)
                                            {
                                                ctx.Rotate(customAngle);
                                            }
                                            break;
                                    }
                                }
                            }
                        });

                        await image.SaveAsync(outputPath, cancellationToken);
                        results.Add(outputPath);
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error rotating {Path.GetFileName(inputPath)}: {ex.Message}");
                }

                processedCount++;
            }

            ReportProgress(progress, 100, "Image rotation complete");

            return Success($"Successfully rotated {results.Count} of {inputPaths.Count} images", new Dictionary<string, object>
            {
                ["outputPaths"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to rotate images: {ex.Message}", ex);
        }
    }
}
