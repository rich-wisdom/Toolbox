using WindowsToolbox.Core.Interfaces;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.IO;
using System.Text;

namespace WindowsToolbox.Tools.Document;

public class ConvertDocumentsTool : BaseTool
{
    public override string Id => "convert-documents";
    public override string Name => "Convert Documents";
    public override string Description => "Convert between document formats";
    public override string Category => "document";
    public override string Icon => "Transform";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var outputFormat = parameters.Get<string>("outputFormat");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            if (string.IsNullOrEmpty(outputFormat))
                return Failure("Output format is required");

            ReportProgress(progress, 0, "Converting document...");

            var inputExt = Path.GetExtension(inputFile).ToLower();
            var outputExt = Path.GetExtension(outputPath).ToLower();

            if (inputExt == ".docx" && outputExt == ".txt")
            {
                await ConvertDocxToTextAsync(inputFile, outputPath);
            }
            else if (inputExt == ".txt" && outputExt == ".docx")
            {
                await ConvertTextToDocxAsync(inputFile, outputPath);
            }
            else
            {
                return Failure($"Conversion from {inputExt} to {outputExt} not yet supported");
            }

            ReportProgress(progress, 100, "Conversion complete");
            return Success($"Successfully converted to {outputFormat}", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert document: {ex.Message}", ex);
        }
    }

    private async Task ConvertDocxToTextAsync(string inputPath, string outputPath)
    {
        using var doc = WordprocessingDocument.Open(inputPath, false);
        var body = doc.MainDocumentPart?.Document.Body;
        if (body == null)
            throw new InvalidOperationException("Document body not found");

        var text = body.InnerText;
        await File.WriteAllTextAsync(outputPath, text);
    }

    private async Task ConvertTextToDocxAsync(string inputPath, string outputPath)
    {
        var text = await File.ReadAllTextAsync(inputPath);
        
        using var doc = WordprocessingDocument.Create(outputPath, DocumentFormat.OpenXml.WordprocessingDocumentType.Document);
        var mainPart = doc.AddMainDocumentPart();
        mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
        var body = mainPart.Document.AppendChild(new Body());

        foreach (var line in text.Split('\n'))
        {
            var para = body.AppendChild(new Paragraph());
            var run = para.AppendChild(new Run());
            run.AppendChild(new Text(line));
        }
    }
}
