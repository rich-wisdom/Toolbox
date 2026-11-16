using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class OrganizePdfTool : BaseTool
{
    public override string Id => "organize-pdf";
    public override string Name => "Organize PDF";
    public override string Description => "Reorder, rotate, and organize PDF pages";
    public override string Category => "document";
    public override string Icon => "Sort";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var pageOrder = parameters.Get<int[]>("pageOrder");
            var rotations = parameters.Get<Dictionary<int, int>>("rotations");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Organizing PDF...");

            using var reader = new PdfReader(inputFile);
            using var sourcePdf = new PdfDocument(reader);
            using var writer = new PdfWriter(outputPath);
            using var outputPdf = new PdfDocument(writer);

            if (pageOrder != null && pageOrder.Length > 0)
            {
                for (int i = 0; i < pageOrder.Length; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Failure("Operation cancelled");

                    var pageNum = pageOrder[i];
                    ReportProgress(progress, ((i + 1) * 100) / pageOrder.Length, $"Processing page {pageNum}...");

                    if (pageNum < 1 || pageNum > sourcePdf.GetNumberOfPages())
                        continue;

                    sourcePdf.CopyPagesTo(pageNum, pageNum, outputPdf);

                    if (rotations != null && rotations.TryGetValue(pageNum, out var rotation))
                    {
                        var page = outputPdf.GetPage(outputPdf.GetNumberOfPages());
                        page.SetRotation(rotation);
                    }
                }
            }
            else
            {
                sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), outputPdf);
            }

            ReportProgress(progress, 100, "Organization complete");
            return await Task.FromResult(Success($"Successfully organized PDF", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["pageCount"] = outputPdf.GetNumberOfPages()
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to organize PDF: {ex.Message}", ex);
        }
    }
}
