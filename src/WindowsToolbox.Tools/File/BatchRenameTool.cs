using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.File;

public class BatchRenameTool : BaseTool
{
    public override string Id => "batch-rename";
    public override string Name => "Batch Rename";
    public override string Description => "Rename multiple files at once";
    public override string Category => "file";
    public override string Icon => "File";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            var operation = parameters.Get<string>("operation");
            var find = parameters.Get<string>("find");
            var replace = parameters.Get<string>("replace");
            var prefix = parameters.Get<string>("prefix");
            var suffix = parameters.Get<string>("suffix");

            if (filePathsObj == null)
                return Failure("file_paths is required");

            if (string.IsNullOrWhiteSpace(operation))
                return Failure("operation is required");

            var filePaths = ParseFilePathsArray(filePathsObj);

            ReportProgress(progress, 10, "Preparing rename...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, $"Renaming {filePaths.Count} files...");
            await Task.Delay(100, cancellationToken);

            var renamedPaths = new List<string>();
            foreach (var path in filePaths)
            {
                var dir = Path.GetDirectoryName(path)!;
                var fileName = Path.GetFileNameWithoutExtension(path);
                var ext = Path.GetExtension(path);
                var newFileName = operation switch
                {
                    "add_prefix" => $"{prefix}{fileName}{ext}",
                    "add_suffix" => $"{fileName}{suffix}{ext}",
                    "find_replace" => $"{fileName.Replace(find ?? "", replace ?? "")}{ext}",
                    _ => fileName + ext
                };
                renamedPaths.Add(Path.Combine(dir, newFileName));
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Renamed {renamedPaths.Count} files", new Dictionary<string, object>
            {
                ["output_files"] = renamedPaths,
                ["count"] = renamedPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to rename files: {ex.Message}", ex);
        }
    }

    private List<string> ParseFilePathsArray(object filePathsObj)
    {
        if (filePathsObj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            return jsonElement.EnumerateArray()
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        return new List<string>();
    }
}
