using System.Text;
using System.Text.RegularExpressions;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.TextAndSpeech;

public class SummarizeTool : BaseTool
{
    public override string Id => "summarize";
    public override string Name => "Summarize";
    public override string Description => "Summarize long text into key points";
    public override string Category => "text-speech";
    public override string Icon => "TextGrammarOptions";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var inputText = parameters.Get<string>("inputText");
            var inputFile = parameters.Get<string>("inputFile");
            var summaryLength = parameters.Get<string>("summaryLength") ?? "medium";
            var style = parameters.Get<string>("style") ?? "paragraph";

            string textToSummarize;

            if (!string.IsNullOrWhiteSpace(inputFile))
            {
                if (!File.Exists(inputFile))
                    return Failure("Input file does not exist");

                ReportProgress(progress, 10, "Reading file...");
                textToSummarize = await File.ReadAllTextAsync(inputFile, cancellationToken);
            }
            else if (!string.IsNullOrWhiteSpace(inputText))
            {
                textToSummarize = inputText;
            }
            else
            {
                return Failure("Either input text or input file is required");
            }

            ReportProgress(progress, 30, "Analyzing text...");

            var sentences = SplitIntoSentences(textToSummarize);
            
            if (sentences.Length == 0)
                return Failure("No sentences found in the input text");

            ReportProgress(progress, 50, "Scoring sentences...");

            var sentenceScores = ScoreSentences(sentences);

            ReportProgress(progress, 70, "Generating summary...");

            var summaryCount = summaryLength.ToLower() switch
            {
                "brief" => Math.Max(1, sentences.Length / 10),
                "medium" => Math.Max(2, sentences.Length / 5),
                "detailed" => Math.Max(3, sentences.Length / 3),
                _ => Math.Max(2, sentences.Length / 5)
            };

            var topSentences = sentenceScores
                .OrderByDescending(kvp => kvp.Value)
                .Take(summaryCount)
                .OrderBy(kvp => Array.IndexOf(sentences, kvp.Key))
                .Select(kvp => kvp.Key)
                .ToList();

            var summary = style.ToLower() switch
            {
                "bullets" => FormatAsBullets(topSentences),
                "paragraph" => string.Join(" ", topSentences),
                _ => string.Join(" ", topSentences)
            };

            ReportProgress(progress, 90, "Finalizing summary...");

            if (!string.IsNullOrWhiteSpace(inputFile))
            {
                var outputFile = Path.Combine(
                    Path.GetDirectoryName(inputFile) ?? "",
                    $"{Path.GetFileNameWithoutExtension(inputFile)}_summary.txt");

                await File.WriteAllTextAsync(outputFile, summary, cancellationToken);

                ReportProgress(progress, 100, "Summary complete");

                return Success("Summary created successfully", new Dictionary<string, object>
                {
                    ["outputFile"] = outputFile,
                    ["summary"] = summary,
                    ["originalSentences"] = sentences.Length,
                    ["summarySentences"] = topSentences.Count
                });
            }
            else
            {
                ReportProgress(progress, 100, "Summary complete");

                return Success("Summary created successfully", new Dictionary<string, object>
                {
                    ["summary"] = summary,
                    ["originalSentences"] = sentences.Length,
                    ["summarySentences"] = topSentences.Count
                });
            }
        }
        catch (OperationCanceledException)
        {
            return Failure("Operation was cancelled");
        }
        catch (Exception ex)
        {
            return Failure($"Failed to summarize: {ex.Message}", ex);
        }
    }

    private string[] SplitIntoSentences(string text)
    {
        var sentencePattern = @"(?<=[.!?])\s+(?=[A-Z])";
        var sentences = Regex.Split(text, sentencePattern)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToArray();
        return sentences;
    }

    private Dictionary<string, double> ScoreSentences(string[] sentences)
    {
        var wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var stopWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "is", "at", "which", "on", "a", "an", "and", "or", "but", "in", "with", "to", "for", "of", "as", "by", "that", "this", "it", "from", "be", "are", "was", "were", "been", "have", "has", "had", "do", "does", "did", "will", "would", "could", "should"
        };

        foreach (var sentence in sentences)
        {
            var words = Regex.Split(sentence.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w) && !stopWords.Contains(w));

            foreach (var word in words)
            {
                if (wordFrequency.ContainsKey(word))
                    wordFrequency[word]++;
                else
                    wordFrequency[word] = 1;
            }
        }

        var sentenceScores = new Dictionary<string, double>();

        for (int i = 0; i < sentences.Length; i++)
        {
            var sentence = sentences[i];
            var words = Regex.Split(sentence.ToLower(), @"\W+")
                .Where(w => !string.IsNullOrWhiteSpace(w) && !stopWords.Contains(w));

            var score = words.Sum(word => wordFrequency.ContainsKey(word) ? wordFrequency[word] : 0);

            var positionBonus = i < sentences.Length * 0.2 ? 1.5 : 1.0;
            sentenceScores[sentence] = score * positionBonus;
        }

        return sentenceScores;
    }

    private string FormatAsBullets(List<string> sentences)
    {
        var sb = new StringBuilder();
        foreach (var sentence in sentences)
        {
            sb.AppendLine($"• {sentence}");
        }
        return sb.ToString();
    }
}
