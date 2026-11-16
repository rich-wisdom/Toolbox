using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Geom;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class CropPdfTool : BaseTool
{
    public override string Id => "crop-pdf";
    public override string Name => "Crop PDF";
    public override string Description => "Crop pages to specified dimensions";
    public override string Category => "document";
    public override string Icon => "Crop";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var left = parameters.Get<float?>("left") ?? 0;
            var top = parameters.Get<float?>("top") ?? 0;
            var right = parameters.Get<float?>("right") ?? 0;
            var bottom = parameters.Get<float?>("bottom") ?? 0;
            var applyToAll = parameters.Get<bool?>("applyToAll") ?? true;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Cropping PDF...");

            using var reader = new PdfReader(inputFile);
            using var writer = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(reader, writer);

            var totalPages = pdfDoc.GetNumberOfPages();

            for (int i = 1; i <= totalPages; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                ReportProgress(progress, (i * 100) / totalPages, $"Cropping page {i}...");

                var page = pdfDoc.GetPage(i);
                var mediaBox = page.GetMediaBox();

                var newMediaBox = new Rectangle(
                    mediaBox.GetLeft() + left,
                    mediaBox.GetBottom() + bottom,
                    mediaBox.GetWidth() - left - right,
                    mediaBox.GetHeight() - top - bottom
                );

                page.SetCropBox(newMediaBox);
                page.SetMediaBox(newMediaBox);

                if (!applyToAll)
                    break;
            }

            ReportProgress(progress, 100, "Crop complete");
            return await Task.FromResult(Success($"Successfully cropped {(applyToAll ? totalPages : 1)} pages", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to crop PDF: {ex.Message}", ex);
        }
    }
}
