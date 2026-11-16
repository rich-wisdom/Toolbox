using WindowsToolbox.Core.Interfaces;
using iText.Kernel.Pdf;
using iText.Forms;
using iText.Forms.Fields;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class FillFormsTool : BaseTool
{
    public override string Id => "fill-forms";
    public override string Name => "Fill & Flatten Forms";
    public override string Description => "Fill PDF forms and flatten them";
    public override string Category => "document";
    public override string Icon => "Form";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var formData = parameters.Get<Dictionary<string, string>>("formData");
            var flatten = parameters.Get<bool?>("flatten") ?? false;

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input PDF file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            ReportProgress(progress, 0, "Filling form...");

            using var reader = new PdfReader(inputFile);
            using var writer = new PdfWriter(outputPath);
            using var pdfDoc = new PdfDocument(reader, writer);

            var form = PdfAcroForm.GetAcroForm(pdfDoc, true);
            if (form == null)
                return Failure("PDF does not contain a form");

            var fields = form.GetAllFormFields();
            var filledCount = 0;

            if (formData != null)
            {
                foreach (var kvp in formData)
                {
                    if (fields.TryGetValue(kvp.Key, out var field))
                    {
                        field.SetValue(kvp.Value);
                        filledCount++;
                    }
                }
            }

            if (flatten)
            {
                form.FlattenFields();
            }

            ReportProgress(progress, 100, "Form filled");
            return await Task.FromResult(Success($"Filled {filledCount} fields", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["filledCount"] = filledCount,
                ["flattened"] = flatten
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to fill form: {ex.Message}", ex);
        }
    }
}
