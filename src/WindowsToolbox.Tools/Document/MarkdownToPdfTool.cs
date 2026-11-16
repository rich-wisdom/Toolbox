using WindowsToolbox.Core.Interfaces;
using Markdig;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class MarkdownToPdfTool : BaseTool
{
    public override string Id => "markdown-to-pdf";
    public override string Name => "Markdown to PDF";
    public override string Description => "Convert Markdown to PDF";
    public override string Category => "document";
    public override string Icon => "Document";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var markdownText = parameters.Get<string>("markdownText");
            var outputPath = parameters.Get<string>("outputPath");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            string markdown;
            if (!string.IsNullOrEmpty(inputFile) && File.Exists(inputFile))
            {
                markdown = await File.ReadAllTextAsync(inputFile);
            }
            else if (!string.IsNullOrEmpty(markdownText))
            {
                markdown = markdownText;
            }
            else
            {
                return Failure("Either input file or markdown text is required");
            }

            ReportProgress(progress, 0, "Converting Markdown to PDF...");

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .Build();

            var html = Markdown.ToHtml(markdown, pipeline);

            using var writer = new PdfWriter(outputPath);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            var lines = markdown.Split('\n');
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var paragraph = new Paragraph(line.TrimStart('#', ' '));
                
                if (line.StartsWith("# "))
                {
                    paragraph.SetFontSize(24);
                    paragraph.SetBold();
                }
                else if (line.StartsWith("## "))
                {
                    paragraph.SetFontSize(20);
                    paragraph.SetBold();
                }
                else if (line.StartsWith("### "))
                {
                    paragraph.SetFontSize(16);
                    paragraph.SetBold();
                }

                document.Add(paragraph);
            }

            ReportProgress(progress, 100, "Conversion complete");
            return await Task.FromResult(Success("Successfully converted Markdown to PDF", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert Markdown to PDF: {ex.Message}", ex);
        }
    }
}
