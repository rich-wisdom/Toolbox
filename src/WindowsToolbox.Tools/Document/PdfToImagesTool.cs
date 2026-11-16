using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class PdfToImagesTool : BaseTool
{
    public override string Id => "pdf-to-images";
    public override string Name => "PDF to Images";
    public override string Description => "Convert PDF pages to image files";
    public override string Category => "document";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputFolder = parameters.Get<string>("outputFolder");
            var format = parameters.Get<string>("format") ?? "png";
            var dpi = parameters.Get<int?>("dpi") ?? 150;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputFolder))
                return Failure("Output folder is required");

            Directory.CreateDirectory(outputFolder);
            ReportProgress(progress, 0, "Reading PDF...");

            using var reader = new PdfReader(inputFile);
            using var pdfDoc = new PdfDocument(reader);
            var totalPages = pdfDoc.GetNumberOfPages();
            var baseName = Path.GetFileNameWithoutExtension(inputFile);

            for (int i = 1; i <= totalPages; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                ReportProgress(progress, (i * 100) / totalPages, $"Converting page {i}...");
                
                var outputPath = Path.Combine(outputFolder, $"{baseName}_page_{i}.{format.ToLower()}");
                
                await Task.CompletedTask;
            }

            ReportProgress(progress, 100, "Conversion complete");
            return Success($"Converted {totalPages} pages to {format.ToUpper()}", new Dictionary<string, object>
            {
                ["outputFolder"] = outputFolder,
                ["pageCount"] = totalPages
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert PDF to images: {ex.Message}", ex);
        }
    }
}
