using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class OcrImageTool : BaseTool
{
    public override string Id => "ocr-image";
    public override string Name => "OCR Image";
    public override string Description => "Extract text from images using OCR";
    public override string Category => "text";
    public override string Icon => "Text";
    public override bool IsAvailable => false;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var language = parameters.Get<string>("language") ?? "en";

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!System.IO.File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            return await Task.FromResult(Failure(
                "Image OCR is not implemented. No text was extracted."));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to perform OCR: {ex.Message}", ex);
        }
    }
}
