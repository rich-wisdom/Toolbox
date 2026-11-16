using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.FileUtilities;

public class OrganizeDownloadsTool : BaseTool
{
    public override string Id => "organize-downloads";
    public override string Name => "Organize Downloads";
    public override string Description => "Automatically organize files into categorized folders";
    public override string Category => "file";
    public override string Icon => "FolderList";

    private static readonly Dictionary<string, string[]> DefaultCategories = new()
    {
        ["Images"] = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".svg", ".webp", ".ico", ".tif", ".tiff" },
        ["Documents"] = new[] { ".pdf", ".doc", ".docx", ".txt", ".rtf", ".odt", ".xls", ".xlsx", ".ppt", ".pptx", ".csv" },
        ["Videos"] = new[] { ".mp4", ".avi", ".mkv", ".mov", ".wmv", ".flv", ".webm", ".m4v", ".mpg", ".mpeg" },
        ["Audio"] = new[] { ".mp3", ".wav", ".flac", ".aac", ".ogg", ".wma", ".m4a", ".opus" },
        ["Archives"] = new[] { ".zip", ".rar", ".7z", ".tar", ".gz", ".bz2", ".xz", ".iso" },
        ["Installers"] = new[] { ".exe", ".msi", ".dmg", ".pkg", ".deb", ".rpm", ".appimage" },
        ["Code"] = new[] { ".cs", ".js", ".ts", ".py", ".java", ".cpp", ".c", ".h", ".html", ".css", ".json", ".xml" }
    };

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var folderPath = parameters.Get<string>("folderPath");
            if (string.IsNullOrWhiteSpace(folderPath) || !Directory.Exists(folderPath))
            {
                folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (!Directory.Exists(folderPath))
                    return Failure("Downloads folder not found");
            }

            var preview = parameters.Get<bool>("preview");
            var categories = DefaultCategories; // Could be customized via parameters

            ReportProgress(progress, 10, "Scanning files...");

            var files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
            var operations = new List<Dictionary<string, object>>();

            ReportProgress(progress, 20, $"Found {files.Length} files...");

            for (int i = 0; i < files.Length; i++)
            {
                var file = files[i];
                var fileInfo = new FileInfo(file);
                var extension = fileInfo.Extension.ToLowerInvariant();

                string? category = null;
                foreach (var cat in categories)
                {
                    if (cat.Value.Contains(extension))
                    {
                        category = cat.Key;
                        break;
                    }
                }

                if (category == null)
                    category = "Other";

                var targetFolder = Path.Combine(folderPath, category);
                var targetPath = Path.Combine(targetFolder, fileInfo.Name);

                var operation = new Dictionary<string, object>
                {
                    ["source"] = file,
                    ["target"] = targetPath,
                    ["category"] = category,
                    ["size"] = fileInfo.Length,
                    ["extension"] = extension
                };

                if (System.IO.File.Exists(targetPath) && targetPath != file)
                {
                    operation["conflict"] = "Target file already exists";
                }

                operations.Add(operation);

                if (i % 50 == 0)
                    ReportProgress(progress, 20 + (int)((i / (double)files.Length) * 30), $"Analyzed {i + 1}/{files.Length}...");
            }

            if (!preview)
            {
                ReportProgress(progress, 60, "Organizing files...");

                int organized = 0;
                foreach (var category in categories.Keys.Concat(new[] { "Other" }))
                {
                    var categoryFolder = Path.Combine(folderPath, category);
                    if (!Directory.Exists(categoryFolder))
                        Directory.CreateDirectory(categoryFolder);
                }

                for (int i = 0; i < operations.Count; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        break;

                    var op = operations[i];
                    
                    if (op.ContainsKey("conflict"))
                        continue;

                    var source = op["source"].ToString()!;
                    var target = op["target"].ToString()!;

                    if (source == target)
                        continue;

                    try
                    {
                        System.IO.File.Move(source, target);
                        op["organized"] = true;
                        organized++;
                    }
                    catch (Exception ex)
                    {
                        op["error"] = ex.Message;
                    }

                    if (i % 10 == 0)
                        ReportProgress(progress, 60 + (int)((i / (double)operations.Count) * 35), $"Organized {organized}/{files.Length}...");
                }

                ReportProgress(progress, 100, "Complete");

                var categoryCounts = operations
                    .Where(o => (bool)(o.TryGetValue("organized", out var org) ? org : false))
                    .GroupBy(o => o["category"].ToString())
                    .ToDictionary(g => g.Key!, g => g.Count());

                return Success($"Organized {organized} file(s)", new Dictionary<string, object>
                {
                    ["operations"] = operations,
                    ["organized"] = organized,
                    ["total"] = files.Length,
                    ["categoryCounts"] = categoryCounts
                });
            }
            else
            {
                ReportProgress(progress, 100, "Preview complete");

                var categoryCounts = operations
                    .Where(o => !o.ContainsKey("conflict"))
                    .GroupBy(o => o["category"].ToString())
                    .ToDictionary(g => g.Key!, g => g.Count());

                return Success($"Preview: {operations.Count(o => !o.ContainsKey("conflict"))} file(s) will be organized", new Dictionary<string, object>
                {
                    ["operations"] = operations,
                    ["preview"] = true,
                    ["categoryCounts"] = categoryCounts
                });
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to organize files: {ex.Message}", ex);
        }
    }
}
