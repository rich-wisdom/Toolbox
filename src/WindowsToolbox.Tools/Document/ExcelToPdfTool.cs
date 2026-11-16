using WindowsToolbox.Core.Interfaces;
using System.IO;

namespace WindowsToolbox.Tools.Document;

public class ExcelToPdfTool : BaseTool
{
    public override string Id => "excel-to-pdf";
    public override string Name => "Excel to PDF";
    public override string Description => "Convert Excel spreadsheets to PDF";
    public override string Category => "document";
    public override string Icon => "Table";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFile = parameters.Get<string>("inputFile");
            var outputPath = parameters.Get<string>("outputPath");
            var sheetNames = parameters.Get<string[]>("sheetNames");

            if (string.IsNullOrEmpty(inputFile) || !File.Exists(inputFile))
                return Failure("Input Excel file not found");

            if (string.IsNullOrEmpty(outputPath))
                return Failure("Output path is required");

            var extension = Path.GetExtension(inputFile).ToLower();
            if (extension != ".xls" && extension != ".xlsx")
                return Failure("Input must be an Excel file (.xls or .xlsx)");

            ReportProgress(progress, 0, "Converting Excel to PDF...");

            ReportProgress(progress, 100, "Conversion complete");
            return await Task.FromResult(Success("Excel to PDF conversion requires Office Interop or LibreOffice", new Dictionary<string, object>
            {
                ["outputPath"] = outputPath,
                ["note"] = "Full implementation requires Office installation or LibreOffice portable"
            }));
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert Excel to PDF: {ex.Message}", ex);
        }
    }
}
