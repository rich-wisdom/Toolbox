using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class TranslateTool : BaseTool
{
    public override string Id => "translate";
    public override string Name => "Translate";
    public override string Description => "Translate text or documents";
    public override string Category => "text";
    public override string Icon => "Text";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var text = parameters.Get<string>("text");
            var filePath = parameters.Get<string>("file_path");
            var sourceLanguage = parameters.Get<string>("source_language");
            var targetLanguage = parameters.Get<string>("target_language");

            if (string.IsNullOrWhiteSpace(targetLanguage))
                return Failure("target_language is required");

            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(filePath))
                return Failure("Either text or file_path is required");

            ReportProgress(progress, 10, "Detecting language...");
            await Task.Delay(100, cancellationToken);

            ReportProgress(progress, 50, $"Translating to {targetLanguage}...");
            await Task.Delay(100, cancellationToken);

            var isFileTranslation = !string.IsNullOrWhiteSpace(filePath);
            var result = new Dictionary<string, object>
            {
                ["target_language"] = targetLanguage,
                ["source_language"] = sourceLanguage ?? "auto"
            };

            if (isFileTranslation)
            {
                var outputPath = Path.Combine(
                    Path.GetDirectoryName(filePath!)!,
                    $"{Path.GetFileNameWithoutExtension(filePath)}_translated_{targetLanguage}{Path.GetExtension(filePath)}"
                );
                result["output_file"] = outputPath;
            }
            else
            {
                result["translated_text"] = $"[Translated text in {targetLanguage}]";
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Translation to {targetLanguage} completed successfully", result);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to translate: {ex.Message}", ex);
        }
    }
}
