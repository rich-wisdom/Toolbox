using WindowsToolbox.Core.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;

namespace WindowsToolbox.Tools.Image;

public class RemoveMetadataTool : BaseTool
{
    public override string Id => "remove-metadata";
    public override string Name => "Remove Metadata";
    public override string Description => "Strip EXIF and other metadata from images";
    public override string Category => "image";
    public override string Icon => "Delete";

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

            var stripAll = parameters.Get<bool>("stripAll") != false;
            var stripGps = parameters.Get<bool>("stripGps");
            var stripCamera = parameters.Get<bool>("stripCamera");
            var stripDateTime = parameters.Get<bool>("stripDateTime");
            var stripCopyright = parameters.Get<bool>("stripCopyright");
            var showMetadata = parameters.Get<bool>("showMetadata");
            var outputDirectory = parameters.Get<string>("outputDirectory") ?? Path.GetDirectoryName(inputPaths[0]);

            var results = new List<MetadataResult>();
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

                ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Processing {Path.GetFileName(inputPath)}...", inputPath);

                var outputPath = Path.Combine(
                    outputDirectory ?? "", 
                    Path.GetFileNameWithoutExtension(inputPath) + "_clean" + Path.GetExtension(inputPath)
                );

                try
                {
                    using (var image = await Image.LoadAsync<Rgba32>(inputPath, cancellationToken))
                    {
                        var metadata = new Dictionary<string, string>();

                        if (showMetadata && image.Metadata.ExifProfile != null)
                        {
                            foreach (var value in image.Metadata.ExifProfile.Values)
                            {
                                metadata[value.Tag.ToString()] = value.ToString() ?? "";
                            }
                        }

                        if (stripAll)
                        {
                            image.Metadata.ExifProfile = null;
                            image.Metadata.IccProfile = null;
                            image.Metadata.XmpProfile = null;
                            image.Metadata.IptcProfile = null;
                        }
                        else
                        {
                            if (image.Metadata.ExifProfile != null)
                            {
                                if (stripGps)
                                {
                                    RemoveGpsData(image.Metadata.ExifProfile);
                                }

                                if (stripCamera)
                                {
                                    RemoveCameraData(image.Metadata.ExifProfile);
                                }

                                if (stripDateTime)
                                {
                                    RemoveDateTimeData(image.Metadata.ExifProfile);
                                }

                                if (stripCopyright)
                                {
                                    RemoveCopyrightData(image.Metadata.ExifProfile);
                                }
                            }
                        }

                        await image.SaveAsync(outputPath, cancellationToken);

                        results.Add(new MetadataResult
                        {
                            InputPath = inputPath,
                            OutputPath = outputPath,
                            Metadata = metadata,
                            MetadataRemoved = stripAll || stripGps || stripCamera || stripDateTime || stripCopyright
                        });
                    }
                }
                catch (Exception ex)
                {
                    ReportProgress(progress, (processedCount * 100) / inputPaths.Count, $"Error processing {Path.GetFileName(inputPath)}: {ex.Message}");
                }

                processedCount++;
            }

            ReportProgress(progress, 100, "Metadata removal complete");

            return Success($"Successfully processed {results.Count} of {inputPaths.Count} images", new Dictionary<string, object>
            {
                ["results"] = results,
                ["processedCount"] = results.Count,
                ["totalCount"] = inputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to remove metadata: {ex.Message}", ex);
        }
    }

    private void RemoveGpsData(ExifProfile profile)
    {
        profile.RemoveValue(ExifTag.GPSLatitude);
        profile.RemoveValue(ExifTag.GPSLongitude);
        profile.RemoveValue(ExifTag.GPSAltitude);
        profile.RemoveValue(ExifTag.GPSLatitudeRef);
        profile.RemoveValue(ExifTag.GPSLongitudeRef);
        profile.RemoveValue(ExifTag.GPSAltitudeRef);
        profile.RemoveValue(ExifTag.GPSTimestamp);
        profile.RemoveValue(ExifTag.GPSDateStamp);
    }

    private void RemoveCameraData(ExifProfile profile)
    {
        profile.RemoveValue(ExifTag.Make);
        profile.RemoveValue(ExifTag.Model);
        profile.RemoveValue(ExifTag.Software);
        profile.RemoveValue(ExifTag.LensModel);
        profile.RemoveValue(ExifTag.LensMake);
        profile.RemoveValue(ExifTag.ExposureTime);
        profile.RemoveValue(ExifTag.FNumber);
        profile.RemoveValue(ExifTag.ISOSpeedRatings);
        profile.RemoveValue(ExifTag.FocalLength);
        profile.RemoveValue(ExifTag.Flash);
        profile.RemoveValue(ExifTag.WhiteBalance);
    }

    private void RemoveDateTimeData(ExifProfile profile)
    {
        profile.RemoveValue(ExifTag.DateTime);
        profile.RemoveValue(ExifTag.DateTimeOriginal);
        profile.RemoveValue(ExifTag.DateTimeDigitized);
    }

    private void RemoveCopyrightData(ExifProfile profile)
    {
        profile.RemoveValue(ExifTag.Copyright);
        profile.RemoveValue(ExifTag.Artist);
    }
}

public class MetadataResult
{
    public string InputPath { get; set; } = "";
    public string OutputPath { get; set; } = "";
    public Dictionary<string, string> Metadata { get; set; } = new();
    public bool MetadataRemoved { get; set; }
}
