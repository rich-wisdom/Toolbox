using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class RedactPdfTool : BaseTool
{
    public override string Id => "redact-pdf";
    public override string Name => "Redact PDF";
    public override string Description => "Redact sensitive information from PDF";
    public override string Category => "document";
    public override string Icon => "EyeOff";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var searchText = parameters.Get<string>("searchText");
            var redactionAreas = parameters.Get<List<RedactionArea>>("redactionAreas");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Redacting PDF...");

            using var reader = new PdfReader(inputFile);
            using var writer = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(reader, writer);

            var totalPages = pdfDoc.GetNumberOfPages();
            var redactionCount = 0;

            for (int i = 1; i <= totalPages; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                ReportProgress(progress, (i * 100) / totalPages, $"Processing page {i}...");

                var page = pdfDoc.GetPage(i);
                var canvas = new PdfCanvas(page);

                if (redactionAreas != null)
                {
                    foreach (var area in redactionAreas.Where(a => a.PageNumber == i))
                    {
                        canvas.SaveState();
                        canvas.SetFillColor(ColorConstants.BLACK);
                        canvas.Rectangle(area.X, area.Y, area.Width, area.Height);
                        canvas.Fill();
                        canvas.RestoreState();
                        redactionCount++;
                    }
                }
            }

            ReportProgress(progress, 100, "Redaction complete");
            return await Task.FromResult(Success($"Applied {redactionCount} redactions", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["redactionCount"] = redactionCount
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to redact PDF: {ex.Message}", ex);
        }
    }

    private class RedactionArea
    {
        public int PageNumber { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
    }
}
