using System.Security.Cryptography;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.FileUtilities;

public class FindDuplicatesTool : BaseTool
{
    public override string Id => "find-duplicates";
    public override string Name => "Find Duplicates";
    public override string Description => "Find duplicate files in a folder";
    public override string Category => "file";
    public override string Icon => "Copy";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var folderPath = parameters.Get<string>("folderPath");
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
                return Failure("Invalid folder path");

            var recursive = parameters.Get<bool>("recursive");
            if (!parameters.Values.ContainsKey("recursive"))
                recursive = true;

            var method = parameters.Get<string>("method") ?? "hash"; // hash, name, size
            var fileTypes = parameters.Get<string[]>("fileTypes");

            ReportProgress(progress, 10, "Scanning files...");

            var searchOption = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            var files = Directory.GetFiles(folderPath, "*", searchOption);

            if (fileTypes != null && fileTypes.Length > 0)
            {
                files = files.Where(f => fileTypes.Any(ext => 
                    f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToArray();
            }

            ReportProgress(progress, 20, $"Found {files.Length} files...");

            var duplicateGroups = new List<List<Dictionary<string, object>>>();

            switch (method)
            {
                case "hash":
                    duplicateGroups = await FindDuplicatesByHash(files, progress, cancellationToken);
                    break;

                case "name":
                    duplicateGroups = FindDuplicatesByName(files, progress);
                    break;

                case "size":
                    duplicateGroups = FindDuplicatesBySize(files, progress);
                    break;

                default:
                    return Failure($"Invalid method: {method}");
            }

            ReportProgress(progress, 100, "Complete");

            var totalDuplicates = duplicateGroups.Sum(g => g.Count - 1);
            var wastedSpace = duplicateGroups.Sum(g => 
            {
                var size = new FileInfo(g[0]["path"].ToString()!).Length;
                return size * (g.Count - 1);
            });

            return Success($"Found {duplicateGroups.Count} group(s) with {totalDuplicates} duplicate(s)", new Dictionary<string, object>
            {
                ["groups"] = duplicateGroups,
                ["totalGroups"] = duplicateGroups.Count,
                ["totalDuplicates"] = totalDuplicates,
                ["wastedSpace"] = wastedSpace,
                ["method"] = method
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to find duplicates: {ex.Message}", ex);
        }
    }

    private async Task<List<List<Dictionary<string, object>>>> FindDuplicatesByHash(
        string[] files, IProgress<ToolProgress>? progress, CancellationToken cancellationToken)
    {
        var hashGroups = new Dictionary<string, List<Dictionary<string, object>>>();

        for (int i = 0; i < files.Length; i++)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var file = files[i];
            try
            {
                var hash = await ComputeFileHash(file, cancellationToken);
                var fileInfo = new FileInfo(file);

                var fileData = new Dictionary<string, object>
                {
                    ["path"] = file,
                    ["name"] = fileInfo.Name,
                    ["size"] = fileInfo.Length,
                    ["hash"] = hash
                };

                if (!hashGroups.ContainsKey(hash))
                    hashGroups[hash] = new List<Dictionary<string, object>>();

                hashGroups[hash].Add(fileData);
            }
            catch
            {
                // Skip files that can't be accessed
            }

            if (i % 10 == 0)
                ReportProgress(progress, 20 + (int)((i / (double)files.Length) * 70), $"Hashing {i + 1}/{files.Length}...");
        }

        return hashGroups.Where(g => g.Value.Count > 1).Select(g => g.Value).ToList();
    }

    private List<List<Dictionary<string, object>>> FindDuplicatesByName(
        string[] files, IProgress<ToolProgress>? progress)
    {
        var nameGroups = new Dictionary<string, List<Dictionary<string, object>>>();

        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fileInfo = new FileInfo(file);
            var name = fileInfo.Name.ToLowerInvariant();

            var fileData = new Dictionary<string, object>
            {
                ["path"] = file,
                ["name"] = fileInfo.Name,
                ["size"] = fileInfo.Length
            };

            if (!nameGroups.ContainsKey(name))
                nameGroups[name] = new List<Dictionary<string, object>>();

            nameGroups[name].Add(fileData);

            if (i % 100 == 0)
                ReportProgress(progress, 20 + (int)((i / (double)files.Length) * 70), $"Processing {i + 1}/{files.Length}...");
        }

        return nameGroups.Where(g => g.Value.Count > 1).Select(g => g.Value).ToList();
    }

    private List<List<Dictionary<string, object>>> FindDuplicatesBySize(
        string[] files, IProgress<ToolProgress>? progress)
    {
        var sizeGroups = new Dictionary<long, List<Dictionary<string, object>>>();

        for (int i = 0; i < files.Length; i++)
        {
            var file = files[i];
            var fileInfo = new FileInfo(file);

            var fileData = new Dictionary<string, object>
            {
                ["path"] = file,
                ["name"] = fileInfo.Name,
                ["size"] = fileInfo.Length
            };

            if (!sizeGroups.ContainsKey(fileInfo.Length))
                sizeGroups[fileInfo.Length] = new List<Dictionary<string, object>>();

            sizeGroups[fileInfo.Length].Add(fileData);

            if (i % 100 == 0)
                ReportProgress(progress, 20 + (int)((i / (double)files.Length) * 70), $"Processing {i + 1}/{files.Length}...");
        }

        return sizeGroups.Where(g => g.Value.Count > 1).Select(g => g.Value).ToList();
    }

    private async Task<string> ComputeFileHash(string filePath, CancellationToken cancellationToken)
    {
        using var sha256 = SHA256.Create();
        using var stream = System.IO.File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream, cancellationToken);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
