using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class RemovePasswordTool : BaseTool
{
    public override string Id => "remove-password";
    public override string Name => "Remove Password";
    public override string Description => "Remove password protection from PDF";
    public override string Category => "document";
    public override string Icon => "LockOpen";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var password = parameters.Get<string>("password");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            if (string.IsNullOrEmpty(password))
                return Failure("Password is required");

            ReportProgress(progress, 0, "Removing password...");

            var readerProperties = new ReaderProperties()
                .SetPassword(System.Text.Encoding.UTF8.GetBytes(password));

            using var reader = new PdfReader(inputFile, readerProperties);
            using var writer = new PdfWriter(outputPath);
            using var sourcePdf = new PdfDocument(reader);
            using var outputPdf = new PdfDocument(writer);

            sourcePdf.CopyPagesTo(1, sourcePdf.GetNumberOfPages(), outputPdf);

            ReportProgress(progress, 100, "Password removed");
            return await Task.FromResult(Success("Successfully removed password protection", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            }));
        }
        catch (iText.IO.Exceptions.BadPasswordException)
        {
            return Failure("Incorrect password provided");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to remove password: {ex.Message}", ex);
        }
    }
}
