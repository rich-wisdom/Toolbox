using WindowsToolbox.Core.Interfaces;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class PowerPointToPdfTool : BaseTool
{
    public override string Id => "powerpoint-to-pdf";
    public override string Name => "PowerPoint to PDF";
    public override string Description => "Convert PowerPoint presentations to PDF";
    public override string Category => "document";
    public override string Icon => "Presentation";
    public override bool IsAvailable => false;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var includeNotes = parameters.Get<bool?>("includeNotes") ?? false;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PowerPoint file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            var extension = Path.GetExtension(inputFile).ToLower();
            if (extension != ".ppt" && extension != ".pptx")
                return Failure("Input must be a PowerPoint file (.ppt or .pptx)");

            return await Task.FromResult(Failure(
                "PowerPoint to PDF is not implemented. No output file was created."));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert PowerPoint to PDF: {ex.Message}", ex);
        }
    }
}
