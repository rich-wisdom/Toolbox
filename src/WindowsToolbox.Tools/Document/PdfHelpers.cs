using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.IO;

namespace WindowsToolbox.Tools.Document;

internal static class PdfHelpers
{
    public static bool IsPdfFile(string filePath)
    {
        try
        {
            using var reader = new PdfReader(filePath);
            using var document = new PdfDocument(reader);
            return document.GetNumberOfPages() > 0;
        }
        catch
        {
            return false;
        }
    }

    public static int GetPageCount(string pdfPath)
    {
        using var reader = new PdfReader(pdfPath);
        using var document = new PdfDocument(reader);
        return document.GetNumberOfPages();
    }

    public static async Task<byte[]> ConvertImageToPdfAsync(string imagePath)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new PdfWriter(memoryStream);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);
        
        var imageData = ImageDataFactory.Create(imagePath);
        var image = new Image(imageData);
        
        var pageSize = iText.Kernel.Geom.PageSize.A4;
        if (image.GetImageWidth() > image.GetImageHeight())
            pageSize = pageSize.Rotate();
        
        pdf.SetDefaultPageSize(pageSize);
        
        image.ScaleToFit(pageSize.GetWidth() - 72, pageSize.GetHeight() - 72);
        image.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
        
        document.Add(image);
        document.Close();
        
        return await Task.FromResult(memoryStream.ToArray());
    }

    public static void ValidatePassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty");
        
        if (password.Length < 4)
            throw new ArgumentException("Password must be at least 4 characters");
    }
}
