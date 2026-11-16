using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class ImagesToPdfTool : BaseTool
{
    public override string Id => "images-to-pdf";
    public override string Name => "Images to PDF";
    public override string Description => "Convert multiple images to PDF";
    public override string Category => "document";
    public override string Icon => "Image";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var imageFiles = parameters.Get<string[]>("imageFiles");
            var outputPath = parameters.Get<string>("outputPath");
            var fitToPage = parameters.Get<bool?>("fitToPage") ?? true;

            if (imageFiles == null || imageFiles.Length == 0)
                return Failure("No image files provided");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Creating PDF...");

            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            for (int i = 0; i < imageFiles.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                var imageFile = imageFiles[i];
                ReportProgress(progress, ((i + 1) * 100) / imageFiles.Length, $"Adding {Path.GetFileName(imageFile)}...", imageFile);

                if (!File.Exists(imageFile))
                    continue;

                var imageData = ImageDataFactory.Create(imageFile);
                var image = new Image(imageData);

                var pageSize = iText.Kernel.Geom.PageSize.A4;
                if (image.GetImageWidth() > image.GetImageHeight())
                    pageSize = pageSize.Rotate();

                pdf.SetDefaultPageSize(pageSize);

                if (fitToPage)
                {
                    image.ScaleToFit(pageSize.GetWidth() - 72, pageSize.GetHeight() - 72);
                    image.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                }

                document.Add(image);
                
                if (i < imageFiles.Length - 1)
                    document.Add(new AreaBreak());
            }

            ReportProgress(progress, 100, "PDF created");
            return await Task.FromResult(Success($"Created PDF with {imageFiles.Length} images", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["imageCount"] = imageFiles.Length
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to create PDF from images: {ex.Message}", ex);
        }
    }
}
