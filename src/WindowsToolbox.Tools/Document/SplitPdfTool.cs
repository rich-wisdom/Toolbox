using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class SplitPdfTool : BaseTool
{
    public override string Id => "split-pdf";
    public override string Name => "Split PDF";
    public override string Description => "Split PDF into multiple files";
    public override string Category => "document";
    public override string Icon => "Cut";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFolder = parameters.Get<string>("outputFolder");
            var splitMode = parameters.Get<string>("splitMode") ?? "pages";
            var pagesPerFile = parameters.Get<int?>("pagesPerFile") ?? 1;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputFolder))
                return Failure("Output folder is required");

            Directory.CreateDirectory(outputFolder);
            ReportProgress(progress, 0, "Reading PDF...");

            using var reader = new PdfReader(inputFile);
            using var sourcePdf = new PdfDocument(reader);
            var totalPages = sourcePdf.GetNumberOfPages();

            var fileCount = 0;
            var baseName = Path.GetFileNameWithoutExtension(inputFile);

            if (splitMode == "single")
            {
                for (int i = 1; i <= totalPages; i++)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Failure("Operation cancelled");

                    ReportProgress(progress, (i * 100) / totalPages, $"Extracting page {i}...");

                    var outputPath = Path.Combine(outputFolder, $"{baseName}_page_{i}.pdf");
                    using var writer = new PdfWriter(outputPath);
                    using var outputPdf = new PdfDocument(writer);
                    sourcePdf.CopyPagesTo(i, i, outputPdf);
                    fileCount++;
                }
            }
            else if (splitMode == "range")
            {
                for (int i = 1; i <= totalPages; i += pagesPerFile)
                {
                    if (cancellationToken.IsCancellationRequested)
                        return Failure("Operation cancelled");

                    var endPage = Math.Min(i + pagesPerFile - 1, totalPages);
                    ReportProgress(progress, (i * 100) / totalPages, $"Creating file {fileCount + 1}...");

                    var outputPath = Path.Combine(outputFolder, $"{baseName}_part_{fileCount + 1}.pdf");
                    using var writer = new PdfWriter(outputPath);
                    using var outputPdf = new PdfDocument(writer);
                    sourcePdf.CopyPagesTo(i, endPage, outputPdf);
                    fileCount++;
                }
            }

            ReportProgress(progress, 100, "Split complete");
            return await Task.FromResult(Success($"Successfully split into {fileCount} files", new Dictionary<string, object>
            {
                ["fileCount"] = fileCount,
                ["outputFolder"] = outputFolder
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to split PDF: {ex.Message}", ex);
        }
    }
}
