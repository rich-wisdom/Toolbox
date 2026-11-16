using System.IO.Compression;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.File;

public class ZipUnzipTool : BaseTool
{
    public override string Id => "zip-unzip";
    public override string Name => "Zip & Unzip";
    public override string Description => "Create and extract ZIP archives";
    public override string Category => "file";
    public override string Icon => "ZipFolder";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var mode = parameters.Get<string>("mode") ?? "compress"; // compress or decompress

            if (mode == "compress")
            {
                return await Compress(parameters, progress, cancellationToken);
            }
            else if (mode == "decompress")
            {
                return await Decompress(parameters, progress, cancellationToken);
            }
            else
            {
                return Failure($"Invalid mode: {mode}");
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to process archive: {ex.Message}", ex);
        }
    }

    private async Task<ToolResult> Compress(ToolParameters parameters, IProgress<ToolProgress>? progress, CancellationToken cancellationToken)
    {
        var sourcePaths = parameters.Get<string[]>("sourcePaths");
        if (sourcePaths == null || sourcePaths.Length == 0)
            return Failure("No source files or folders provided");

        var outputPath = parameters.Get<string>("outputPath");
        if (string.IsNullOrWhiteSpace(outputPath))
            return Failure("Output path is required");

        var compressionLevel = parameters.Get<string>("compressionLevel") ?? "optimal"; // fastest, optimal, nocompression

        ReportProgress(progress, 10, "Creating archive...");

        var level = compressionLevel switch
        {
            "fastest" => CompressionLevel.Fastest,
            "nocompression" => CompressionLevel.NoCompression,
            _ => CompressionLevel.Optimal
        };

        if (System.IO.File.Exists(outputPath))
            System.IO.File.Delete(outputPath);

        using (var archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
        {
            var allFiles = new List<(string sourcePath, string entryName)>();

            foreach (var sourcePath in sourcePaths)
            {
                if (System.IO.File.Exists(sourcePath))
                {
                    allFiles.Add((sourcePath, Path.GetFileName(sourcePath)));
                }
                else if (Directory.Exists(sourcePath))
                {
                    var dirName = new DirectoryInfo(sourcePath).Name;
                    var files = Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories);
                    
                    foreach (var file in files)
                    {
                        var relativePath = Path.Combine(dirName, file.Substring(sourcePath.Length).TrimStart(Path.DirectorySeparatorChar));
                        allFiles.Add((file, relativePath));
                    }
                }
            }

            ReportProgress(progress, 20, $"Adding {allFiles.Count} file(s)...");

            for (int i = 0; i < allFiles.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var (filePath, entryName) = allFiles[i];
                
                try
                {
                    archive.CreateEntryFromFile(filePath, entryName, level);
                }
                catch
                {
                    // Skip files that can't be accessed
                }

                if (i % 10 == 0)
                    ReportProgress(progress, 20 + (int)((i / (double)allFiles.Count) * 70), $"Added {i + 1}/{allFiles.Count}...");
            }
        }

        ReportProgress(progress, 100, "Complete");

        var outputFileInfo = new FileInfo(outputPath);
        return Success($"Created archive with {outputFileInfo.Length} bytes", new Dictionary<string, object>
        {
            ["outputPath"] = outputPath,
            ["size"] = outputFileInfo.Length
        });
    }

    private async Task<ToolResult> Decompress(ToolParameters parameters, IProgress<ToolProgress>? progress, CancellationToken cancellationToken)
    {
        var archivePath = parameters.Get<string>("archivePath");
        if (string.IsNullOrWhiteSpace(archivePath) || !System.IO.File.Exists(archivePath))
            return Failure("Invalid archive path");

        var outputFolder = parameters.Get<string>("outputFolder");
        if (string.IsNullOrWhiteSpace(outputFolder))
            return Failure("Output folder is required");

        ReportProgress(progress, 10, "Reading archive...");

        if (!Directory.Exists(outputFolder))
            Directory.CreateDirectory(outputFolder);

        using (var archive = ZipFile.OpenRead(archivePath))
        {
            var entries = archive.Entries;
            ReportProgress(progress, 20, $"Extracting {entries.Count} file(s)...");

            for (int i = 0; i < entries.Count; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                var entry = entries[i];
                
                if (string.IsNullOrEmpty(entry.Name))
                    continue; // Skip directory entries

                try
                {
                    var destinationPath = Path.Combine(outputFolder, entry.FullName);
                    var destinationDir = Path.GetDirectoryName(destinationPath);
                    
                    if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
                        Directory.CreateDirectory(destinationDir);

                    entry.ExtractToFile(destinationPath, overwrite: true);
                }
                catch
                {
                    // Skip files that can't be extracted
                }

                if (i % 10 == 0)
                    ReportProgress(progress, 20 + (int)((i / (double)entries.Count) * 75), $"Extracted {i + 1}/{entries.Count}...");
            }
        }

        ReportProgress(progress, 100, "Complete");

        return Success($"Extracted archive to {outputFolder}", new Dictionary<string, object>
        {
            ["outputFolder"] = outputFolder
        });
    }
}
