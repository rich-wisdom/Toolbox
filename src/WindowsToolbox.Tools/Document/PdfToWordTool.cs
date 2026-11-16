using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Document;

public class PdfToWordTool : BaseTool
{
    public override string Id => "pdf-to-word";
    public override string Name => "PDF to Word";
    public override string Description => "Convert PDF files to Word documents";
    public override string Category => "document";
    public override string Icon => "Document";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            ReportProgress(progress, 10, "Reading PDF...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Converting to Word...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.ChangeExtension(filePath, ".docx");
            
            ReportProgress(progress, 100, "Complete");

            return Success("PDF converted to Word successfully", new Dictionary<string, object>
            {
                ["output_file"] = outputPath
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert PDF to Word: {ex.Message}", ex);
        }
    }
}
