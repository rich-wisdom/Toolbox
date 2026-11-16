using WindowsToolbox.Core.Interfaces;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class WordToPdfTool : BaseTool
{
    public override string Id => "word-to-pdf";
    public override string Name => "Word to PDF";
    public override string Description => "Convert Word documents to PDF";
    public override string Category => "document";
    public override string Icon => "Document";
    public override bool IsAvailable => false;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input Word file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            var extension = Path.GetExtension(inputFile).ToLower();
            if (extension != ".doc" && extension != ".docx")
                return Failure("Input must be a Word document (.doc or .docx)");

            return await Task.FromResult(Failure(
                "Word to PDF is not implemented. No output file was created."));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert Word to PDF: {ex.Message}", ex);
        }
    }
}
