using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.IO.Image;
using iText.Layout;
using iText.Layout.Element;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class SignPdfTool : BaseTool
{
    public override string Id => "sign-pdf";
    public override string Name => "Sign PDF";
    public override string Description => "Add signature to PDF document";
    public override string Category => "document";
    public override string Icon => "Edit";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var signatureImage = parameters.Get<string>("signatureImage");
            var pageNumber = parameters.Get<int?>("pageNumber") ?? 1;
            var x = parameters.Get<float?>("x") ?? 100;
            var y = parameters.Get<float?>("y") ?? 100;
            var width = parameters.Get<float?>("width") ?? 200;
            var height = parameters.Get<float?>("height") ?? 80;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            if (string.IsNullOrEmpty(signatureImage) || !File.Exists(signatureImage))
                return Failure("Signature image not found");

            ReportProgress(progress, 0, "Adding signature...");

            using var reader = new PdfReader(inputFile);
            using var writer = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(reader, writer);

            if (pageNumber < 1 || pageNumber > pdfDoc.GetNumberOfPages())
                return Failure($"Invalid page number: {pageNumber}");

            var page = pdfDoc.GetPage(pageNumber);
            var canvas = new PdfCanvas(page);

            var imageData = ImageDataFactory.Create(signatureImage);
            canvas.AddImageFittedIntoRectangle(imageData, 
                new iText.Kernel.Geom.Rectangle(x, y, width, height), false);

            ReportProgress(progress, 100, "Signature added");
            return await Task.FromResult(Success("Successfully added signature", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to sign PDF: {ex.Message}", ex);
        }
    }
}
