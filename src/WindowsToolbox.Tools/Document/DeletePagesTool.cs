using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class DeletePagesTool : BaseTool
{
    public override string Id => "delete-pages";
    public override string Name => "Delete Pages";
    public override string Description => "Remove specific pages from PDF";
    public override string Category => "document";
    public override string Icon => "Delete";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var pagesToDelete = parameters.Get<int[]>("pagesToDelete");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            if (pagesToDelete == null || pagesToDelete.Length == 0)
                return Failure("No pages specified for deletion");

            ReportProgress(progress, 0, "Reading PDF...");

            using var reader = new PdfReader(inputFile);
            using var sourcePdf = new PdfDocument(reader);
            var totalPages = sourcePdf.GetNumberOfPages();

            using var writer = new PdfWriter(outputPath);
            using var outputPdf = new PdfDocument(writer);

            var deleteSet = new HashSet<int>(pagesToDelete);
            var copiedPages = 0;

            for (int i = 1; i <= totalPages; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                if (!deleteSet.Contains(i))
                {
                    ReportProgress(progress, (i * 100) / totalPages, $"Processing page {i}...");
                    sourcePdf.CopyPagesTo(i, i, outputPdf);
                    copiedPages++;
                }
            }

            ReportProgress(progress, 100, "Delete complete");
            return await Task.FromResult(Success($"Deleted {pagesToDelete.Length} pages, kept {copiedPages} pages", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["deletedCount"] = pagesToDelete.Length,
                ["remainingPages"] = copiedPages
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to delete pages: {ex.Message}", ex);
        }
    }
}
