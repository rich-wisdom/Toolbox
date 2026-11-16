using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Document;

public class MergePdfsTool : BaseTool
{
    public override string Id => "merge-pdfs";
    public override string Name => "Merge PDFs";
    public override string Description => "Combine multiple PDF files into one";
    public override string Category => "document";
    public override string Icon => "Document";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure("Not yet implemented"));
    }
}
