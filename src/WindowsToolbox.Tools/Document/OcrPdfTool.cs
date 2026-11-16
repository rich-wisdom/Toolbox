using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Document;

public class OcrPdfTool : BaseTool
{
    public override string Id => "ocr-pdf";
    public override string Name => "OCR PDF";
    public override string Description => "Add OCR text layer to scanned PDF files";
    public override string Category => "document";
    public override string Icon => "Document";
    public override bool IsAvailable => false;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var language = parameters.Get<string>("language") ?? "en";

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            return await Task.FromResult(Failure(
                "PDF OCR is not implemented. No output file was created."));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to perform OCR on PDF: {ex.Message}", ex);
        }
    }
}
