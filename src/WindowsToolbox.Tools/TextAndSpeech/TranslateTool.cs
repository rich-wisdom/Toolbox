using System.Text;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class TranslateTool : BaseTool
{
    public override string Id => "translate";
    public override string Name => "Translate";
    public override string Description => "Translate text between languages";
    public override string Category => "text-speech";
    public override string Icon => "LocalLanguage";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputText = parameters.Get<string>("inputText");
            var inputFile = parameters.Get<string>("inputFile");
            var sourceLanguage = parameters.Get<string>("sourceLanguage") ?? "auto";
            var targetLanguage = parameters.Get<string>("targetLanguage") ?? "en";

            string textToTranslate;

            if (!string.IsNullOrWhiteSpace(inputFile))
            {
                if (!File.Exists(inputFile))
                    return Failure("Input file does not exist");

                ReportProgress(progress, 10, "Reading file...");
                
                var extension = Path.GetExtension(inputFile).ToLowerInvariant();
                textToTranslate = extension switch
                {
                    ".txt" => await File.ReadAllTextAsync(inputFile, cancellationToken),
                    ".srt" => await File.ReadAllTextAsync(inputFile, cancellationToken),
                    ".vtt" => await File.ReadAllTextAsync(inputFile, cancellationToken),
                    _ => throw new NotSupportedException($"File format {extension} is not supported")
                };
            }
            else if (!string.IsNullOrWhiteSpace(inputText))
            {
                textToTranslate = inputText;
            }
            else
            {
                return Failure("Either input text or input file is required");
            }

            ReportProgress(progress, 30, "Translating...");

            var translatedText = await TranslateTextAsync(textToTranslate, sourceLanguage, targetLanguage, cancellationToken);

            if (!string.IsNullOrWhiteSpace(inputFile))
            {
                var outputFile = Path.Combine(
                    Path.GetDirectoryName(inputFile) ?? "",
                    $"{Path.GetFileNameWithoutExtension(inputFile)}_{targetLanguage}{Path.GetExtension(inputFile)}");

                await File.WriteAllTextAsync(outputFile, translatedText, cancellationToken);

                ReportProgress(progress, 100, "Translation complete");

                return Success("Translation completed successfully", new Dictionary<string, object>
                {
                    ["outputFile"] = outputFile,
                    ["translatedText"] = translatedText
                });
            }
            else
            {
                ReportProgress(progress, 100, "Translation complete");

                return Success("Translation completed successfully", new Dictionary<string, object>
                {
                    ["translatedText"] = translatedText
                });
            }
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to translate: {ex.Message}", ex);
        }
    }

    private async Task<string> TranslateTextAsync(string text, string sourceLanguage, string targetLanguage, CancellationToken cancellationToken)
    {
        await Task.CompletedTask;
        
        return $"[Translation from {sourceLanguage} to {targetLanguage}]\n\n{text}\n\n[Note: Local translation requires additional model files. This is a placeholder implementation. For production use, integrate with Windows ML ONNX models or external translation APIs with user consent.]";
    }
}
