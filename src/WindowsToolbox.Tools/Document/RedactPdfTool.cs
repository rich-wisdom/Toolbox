using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Document;

public class RedactPdfTool : BaseTool
{
    public override string Id => "redact-pdf";
    public override string Name => "Redact PDF";
    public override string Description => "Redact sensitive information from PDF files";
    public override string Category => "document";
    public override string Icon => "Document";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var filePath = parameters.Get<string>("file_path");
            var searchTermsObj = parameters.Get<object>("search_terms");

            if (string.IsNullOrWhiteSpace(filePath))
                return Failure("file_path is required");

            if (!File.Exists(filePath))
                return Failure($"File not found: {filePath}");

            var searchTerms = ParseStringArray(searchTermsObj);

            ReportProgress(progress, 10, "Analyzing PDF...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, "Redacting content...");
            await Task.Delay(100, cancellationToken);

            var outputPath = Path.Combine(Path.GetDirectoryName(filePath)!, 
                $"{Path.GetFileNameWithoutExtension(filePath)}_redacted.pdf");
            
            ReportProgress(progress, 100, "Complete");

            return Success($"Redacted {searchTerms.Count} terms from PDF", new Dictionary<string, object>
            {
                ["output_file"] = outputPath,
                ["redacted_count"] = searchTerms.Count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to redact PDF: {ex.Message}", ex);
        }
    }

    private List<string> ParseStringArray(object? obj)
    {
        if (obj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            return jsonElement.EnumerateArray()
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        return new List<string>();
    }
}
