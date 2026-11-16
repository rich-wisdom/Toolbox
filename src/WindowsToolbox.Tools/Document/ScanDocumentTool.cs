using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class ScanDocumentTool : BaseTool
{
    public override string Id => "scan-document";
    public override string Name => "Scan Document";
    public override string Description => "Create PDF from scanned images";
    public override string Category => "document";
    public override string Icon => "Scan";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var imageFiles = parameters.Get<string[]>("imageFiles");
            var outputPath = parameters.Get<string>("outputPath");
            var autoRotate = parameters.Get<bool?>("autoRotate") ?? true;
            var grayscale = parameters.Get<bool?>("grayscale") ?? false;

            if (imageFiles == null || imageFiles.Length == 0)
                return Failure("No images provided for scanning");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Creating scanned PDF...");

            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            for (int i = 0; i < imageFiles.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                var imageFile = imageFiles[i];
                ReportProgress(progress, ((i + 1) * 100) / imageFiles.Length, $"Scanning page {i + 1}...", imageFile);

                if (!File.Exists(imageFile))
                    continue;

                var imageData = ImageDataFactory.Create(imageFile);
                var image = new Image(imageData);

                var pageSize = iText.Kernel.Geom.PageSize.A4;
                
                if (autoRotate && image.GetImageWidth() > image.GetImageHeight())
                    pageSize = pageSize.Rotate();

                pdf.SetDefaultPageSize(pageSize);
                image.ScaleToFit(pageSize.GetWidth(), pageSize.GetHeight());
                image.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);

                document.Add(image);
                
                if (i < imageFiles.Length - 1)
                    document.Add(new AreaBreak());
            }

            ReportProgress(progress, 100, "Scan complete");
            return await Task.FromResult(Success($"Scanned {imageFiles.Length} pages", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["pageCount"] = imageFiles.Length
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to scan document: {ex.Message}", ex);
        }
    }
}
