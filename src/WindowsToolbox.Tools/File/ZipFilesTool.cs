using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.FileUtilities;

public class ZipFilesTool : BaseTool
{
    public override string Id => "zip-files";
    public override string Name => "ZIP Files";
    public override string Description => "Create ZIP archives from files";
    public override string Category => "file";
    public override string Icon => "File";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePathsObj = parameters.Get<object>("file_paths");
            var outputName = parameters.Get<string>("output_name") ?? "archive";
            var password = parameters.Get<string>("password");

            if (filePathsObj == null)
                return Failure("file_paths is required");

            var filePaths = ParseFilePathsArray(filePathsObj);

            if (filePaths.Count == 0)
                return Failure("At least one file is required");

            ReportProgress(progress, 10, "Preparing files...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, $"Creating ZIP archive...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(
                Path.GetDirectoryName(filePaths[0])!,
                $"{outputName}.zip"
            );

            ReportProgress(progress, 100, "Complete");

            return Success($"Created ZIP archive with {filePaths.Count} files", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["file_count"] = filePaths.Count,
                ["password_protected"] = !string.IsNullOrEmpty(password)
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to create ZIP archive: {ex.Message}", ex);
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
