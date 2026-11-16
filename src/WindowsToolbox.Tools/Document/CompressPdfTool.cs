using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class CompressPdfTool : BaseTool
{
    public override string Id => "compress-pdf";
    public override string Name => "Compress PDF";
    public override string Description => "Reduce PDF file size";
    public override string Category => "document";
    public override string Icon => "Archive";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var compressionLevel = parameters.Get<string>("compressionLevel") ?? "medium";

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Compressing PDF...");

            var originalSize = new FileInfo(inputFile).Length;

            using var reader = new PdfReader(inputFile);
            var writerProperties = new WriterProperties();
            
            writerProperties.SetCompressionLevel(compressionLevel switch
            {
                "low" => 3,
                "high" => 9,
                _ => 6
            });

            using var writer = new PdfWriter(outputPath, writerProperties);
            using var sourcePdf = new PdfDocument(reader);
            using var outputPdf = new PdfDocument(writer);

            var totalPages = sourcePdf.GetNumberOfPages();
            for (int i = 1; i <= totalPages; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                ReportProgress(progress, (i * 100) / totalPages, $"Processing page {i}...");
                sourcePdf.CopyPagesTo(i, i, outputPdf);
            }

            outputPdf.Close();
            var compressedSize = new FileInfo(outputPath).Length;
            var savedPercent = ((originalSize - compressedSize) * 100.0 / originalSize);

            ReportProgress(progress, 100, "Compression complete");
            return await Task.FromResult(Success($"Reduced size by {savedPercent:F1}%", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["originalSize"] = originalSize,
                ["compressedSize"] = compressedSize,
                ["savedPercent"] = savedPercent
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to compress PDF: {ex.Message}", ex);
        }
    }
}
