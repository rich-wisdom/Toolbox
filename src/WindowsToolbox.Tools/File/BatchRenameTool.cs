using System.Text.RegularExpressions;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.File;

public class BatchRenameTool : BaseTool
{
    public override string Id => "batch-rename";
    public override string Name => "Batch Rename";
    public override string Description => "Rename multiple files at once";
    public override string Category => "file";
    public override string Icon => "Rename";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var files = parameters.Get<string[]>("files");
            if (files == null || files.Length == 0)
                return Failure("No files provided");

            var operation = parameters.Get<string>("operation") ?? "replace"; // replace, prefix, suffix, number, remove
            var findText = parameters.Get<string>("findText") ?? "";
            var replaceText = parameters.Get<string>("replaceText") ?? "";
            var prefix = parameters.Get<string>("prefix") ?? "";
            var suffix = parameters.Get<string>("suffix") ?? "";
            var startNumber = parameters.Get<int>("startNumber");
            if (startNumber <= 0) startNumber = 1;
            var useRegex = parameters.Get<bool>("useRegex");
            var preview = parameters.Get<bool>("preview");

            ReportProgress(progress, 10, "Analyzing files...");

            var renameOperations = new List<Dictionary<string, object>>();

            for (int i = 0; i < files.Length; i++)
            {
                var filePath = files[i];
                if (!System.IO.File.Exists(filePath))
                {
                    renameOperations.Add(new Dictionary<string, object>
                    {
                        ["original"] = filePath,
                        ["error"] = "File not found"
                    });
                    continue;
                }

                var directory = Path.GetDirectoryName(filePath) ?? "";
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var extension = Path.GetExtension(filePath);
                var newFileName = fileName;

                switch (operation)
                {
                    case "replace":
                        if (useRegex)
                            newFileName = Regex.Replace(fileName, findText, replaceText);
                        else
                            newFileName = fileName.Replace(findText, replaceText);
                        break;

                    case "prefix":
                        newFileName = prefix + fileName;
                        break;

                    case "suffix":
                        newFileName = fileName + suffix;
                        break;

                    case "number":
                        newFileName = $"{fileName}_{(startNumber + i):D3}";
                        break;

                    case "remove":
                        var removeCount = parameters.Get<int>("removeCount");
                        var removeFrom = parameters.Get<string>("removeFrom") ?? "start"; // start or end
                        if (removeFrom == "start")
                            newFileName = fileName.Length > removeCount ? fileName.Substring(removeCount) : fileName;
                        else
                            newFileName = fileName.Length > removeCount ? fileName.Substring(0, fileName.Length - removeCount) : fileName;
                        break;

                    case "case":
                        var caseType = parameters.Get<string>("caseType") ?? "lower"; // lower, upper, title
                        newFileName = caseType switch
                        {
                            "upper" => fileName.ToUpperInvariant(),
                            "title" => System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(fileName.ToLower()),
                            _ => fileName.ToLowerInvariant()
                        };
                        break;
                }

                var newFilePath = Path.Combine(directory, newFileName + extension);
                var changed = filePath != newFilePath;

                renameOperations.Add(new Dictionary<string, object>
                {
                    ["original"] = filePath,
                    ["new"] = newFilePath,
                    ["changed"] = changed,
                    ["exists"] = System.IO.File.Exists(newFilePath)
                });

                if (i % 10 == 0)
                    ReportProgress(progress, 10 + (int)((i / (double)files.Length) * 40), $"Processed {i + 1}/{files.Length}...");
            }

            if (!preview)
            {
                ReportProgress(progress, 60, "Renaming files...");

                int renamed = 0;
                for (int i = 0; i < renameOperations.Count; i++)
                {
                    var op = renameOperations[i];
                    if (op.ContainsKey("error")) continue;
                    if (!(bool)op["changed"]) continue;
                    if ((bool)op["exists"])
                    {
                        op["error"] = "Target file already exists";
                        continue;
                    }

                    try
                    {
                        System.IO.File.Move(op["original"].ToString()!, op["new"].ToString()!);
                        op["renamed"] = true;
                        renamed++;
                    }
                    catch (Exception ex)
                    {
                        op["error"] = ex.Message;
                    }

                    if (i % 10 == 0)
                        ReportProgress(progress, 60 + (int)((i / (double)renameOperations.Count) * 35), $"Renamed {renamed}/{files.Length}...");
                }

                ReportProgress(progress, 100, "Complete");

                return Success($"Renamed {renamed} file(s)", new Dictionary<string, object>
                {
                    ["operations"] = renameOperations,
                    ["renamed"] = renamed,
                    ["total"] = files.Length
                });
            }
            else
            {
                ReportProgress(progress, 100, "Preview complete");

                return Success($"Preview: {renameOperations.Count(o => (bool)(o.TryGetValue("changed", out var c) ? c : false))} file(s) will be renamed", new Dictionary<string, object>
                {
                    ["operations"] = renameOperations,
                    ["preview"] = true
                });
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to rename files: {ex.Message}", ex);
        }
    }
}
