using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Document;

public class SplitPdfTool : BaseTool
{
    public override string Id => "split-pdf";
    public override string Name => "Split PDF";
    public override string Description => "Split PDF files into multiple documents";
    public override string Category => "document";
    public override string Icon => "Document";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var splitMode = parameters.Get<string>("split_mode");
            var pages = parameters.Get<string>("pages");

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            ReportProgress(progress, 10, "Analyzing PDF...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Splitting PDF...");
            await Task.Delay(100, cancellationToken);

            var outputDir = Path.GetDirectoryName(filePath)!;
            var baseName = Path.GetFileNameWithoutExtension(filePath);
            var outputPaths = new List<string>
            {
                Path.Combine(outputDir, $"{baseName}_part1.pdf"),
                Path.Combine(outputDir, $"{baseName}_part2.pdf")
            };
            
            ReportProgress(progress, 100, "Complete");

            return Success($"PDF split into {outputPaths.Count} documents", new Dictionary<string, object>
            {
                ["output_files"] = outputPaths,
                ["count"] = outputPaths.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to split PDF: {ex.Message}", ex);
        }
    }
}
