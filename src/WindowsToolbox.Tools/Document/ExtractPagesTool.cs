using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class ExtractPagesTool : BaseTool
{
    public override string Id => "extract-pages";
    public override string Name => "Extract Pages";
    public override string Description => "Extract specific pages to new PDF";
    public override string Category => "document";
    public override string Icon => "DocumentCopy";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var pagesToExtract = parameters.Get<int[]>("pagesToExtract");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            if (pagesToExtract == null || pagesToExtract.Length == 0)
                return Failure("No pages specified for extraction");

            ReportProgress(progress, 0, "Reading PDF...");

            using var reader = new PdfReader(inputFile);
            using var sourcePdf = new PdfDocument(reader);
            
            using var writer = new PdfWriter(outputPath);
            using var outputPdf = new PdfDocument(writer);

            var sortedPages = pagesToExtract.OrderBy(p => p).ToArray();

            for (int i = 0; i < sortedPages.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                var pageNum = sortedPages[i];
                ReportProgress(progress, ((i + 1) * 100) / sortedPages.Length, $"Extracting page {pageNum}...");
                
                if (pageNum < 1 || pageNum > sourcePdf.GetNumberOfPages())
                    continue;

                sourcePdf.CopyPagesTo(pageNum, pageNum, outputPdf);
            }

            ReportProgress(progress, 100, "Extraction complete");
            return await Task.FromResult(Success($"Extracted {sortedPages.Length} pages", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["pageCount"] = sortedPages.Length
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to extract pages: {ex.Message}", ex);
        }
    }
}
