using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Kernel.Utils;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class MergePdfsTool : BaseTool
{
    public override string Id => "merge-pdfs";
    public override string Name => "Merge PDFs";
    public override string Description => "Combine multiple PDF files into one";
    public override string Category => "document";
    public override string Icon => "Document";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFiles = parameters.Get<string[]>("inputFiles");
            var outputPath = parameters.Get<string>("outputPath");

            if (inputFiles == null || inputFiles.Length < 2)
                return Failure("At least 2 PDF files are required");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Starting merge...");

            using var outputStream = new FileStream(outputPath, FileMode.Create);
            using var writer = new PdfWriter(outputStream);
            using var mergedPdf = new PdfDocument(writer);
            var merger = new PdfMerger(mergedPdf);

            for (int i = 0; i < inputFiles.Length; i++)
            {
                if (cancellationToken.IsCancellationRequested)
                    return Failure("Operation cancelled");

                var inputFile = inputFiles[i];
                ReportProgress(progress, (i * 100) / inputFiles.Length, $"Merging {Path.GetFileName(inputFile)}...", inputFile);

                if (!File.Exists(inputFile))
                    return Failure($"File not found: {inputFile}");

                using var reader = new PdfReader(inputFile);
                using var sourcePdf = new PdfDocument(reader);
                merger.Merge(sourcePdf, 1, sourcePdf.GetNumberOfPages());
            }

            mergedPdf.Close();
            ReportProgress(progress, 100, "Merge complete");

            return await Task.FromResult(Success($"Successfully merged {inputFiles.Length} PDFs", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["pageCount"] = mergedPdf.GetNumberOfPages()
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to merge PDFs: {ex.Message}", ex);
        }
    }
}
