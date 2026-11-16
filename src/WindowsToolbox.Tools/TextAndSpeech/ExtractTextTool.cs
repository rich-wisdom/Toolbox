using System.Text;
using Tesseract;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class ExtractTextTool : BaseTool
{
    public override string Id => "extract-text";
    public override string Name => "Extract Text";
    public override string Description => "Extract text from images using OCR";
    public override string Category => "text-speech";
    public override string Icon => "TextRecognition";

    private static readonly string TessdataPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "WindowsToolbox", "Tessdata");

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputFiles = parameters.Get<string[]>("inputFiles") ?? parameters.Get<string>("inputFile")?.Split(';');
            var language = parameters.Get<string>("language") ?? "eng";
            var outputFormat = parameters.Get<string>("outputFormat") ?? "txt";

            if (inputFiles == null || inputFiles.Length == 0)
                return Failure("At least one input file is required");

            ReportProgress(progress, 5, "Preparing OCR...");

            Directory.CreateDirectory(TessdataPath);

            await EnsureTessdataExists(language);

            var results = new List<string>();
            var allText = new StringBuilder();
            var totalFiles = inputFiles.Length;

            for (int i = 0; i < totalFiles; i++)
            {
                var inputFile = inputFiles[i];
                
                if (!File.Exists(inputFile))
                {
                    ReportProgress(progress, (i + 1) * 100 / totalFiles, $"Skipping missing file: {Path.GetFileName(inputFile)}");
                    continue;
                }

                var baseProgress = i * 100 / totalFiles;
                ReportProgress(progress, baseProgress + 10, $"Processing: {Path.GetFileName(inputFile)}", inputFile);

                using var engine = new TesseractEngine(TessdataPath, language, EngineMode.Default);
                using var img = Pix.LoadFromFile(inputFile);
                using var page = engine.Process(img);

                var text = page.GetText();
                var confidence = page.GetMeanConfidence();

                allText.AppendLine($"=== {Path.GetFileName(inputFile)} (Confidence: {confidence:P}) ===");
                allText.AppendLine(text);
                allText.AppendLine();

                ReportProgress(progress, (i + 1) * 100 / totalFiles, $"Extracted: {Path.GetFileName(inputFile)}");
            }

            var outputFile = outputFormat.ToLower() switch
            {
                "txt" => Path.Combine(Path.GetDirectoryName(inputFiles[0]) ?? "", "extracted_text.txt"),
                _ => Path.Combine(Path.GetDirectoryName(inputFiles[0]) ?? "", "extracted_text.txt")
            };

            await File.WriteAllTextAsync(outputFile, allText.ToString(), cancellationToken);

            ReportProgress(progress, 100, "Text extraction complete");

            return Success($"Extracted text from {inputFiles.Length} file(s)", new Dictionary<string, object>
            {
                ["outputFile"] = outputFile,
                ["text"] = allText.ToString(),
                ["fileCount"] = inputFiles.Length
            });
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to extract text: {ex.Message}", ex);
        }
    }

    private async Task EnsureTessdataExists(string language)
    {
        var trainedDataFile = Path.Combine(TessdataPath, $"{language}.traineddata");
        
        if (!File.Exists(trainedDataFile))
        {
            var url = $"https://github.com/tesseract-ocr/tessdata_fast/raw/main/{language}.traineddata";
            
            using var httpClient = new HttpClient();
            var data = await httpClient.GetByteArrayAsync(url);
            await File.WriteAllBytesAsync(trainedDataFile, data);
        }
    }
}
