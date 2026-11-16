using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class LoremIpsumTool : BaseTool
{
    public override string Id => "lorem-ipsum";
    public override string Name => "Lorem Ipsum Generator";
    public override string Description => "Generate Lorem Ipsum placeholder text";
    public override string Category => "developer";
    public override string Icon => "TextGrammarError";

    private static readonly string[] Words = new[]
    {
        "lorem", "ipsum", "dolor", "sit", "amet", "consectetur", "adipiscing", "elit",
        "sed", "do", "eiusmod", "tempor", "incididunt", "ut", "labore", "et", "dolore",
        "magna", "aliqua", "enim", "ad", "minim", "veniam", "quis", "nostrud", "exercitation",
        "ullamco", "laboris", "nisi", "aliquip", "ex", "ea", "commodo", "consequat",
        "duis", "aute", "irure", "in", "reprehenderit", "voluptate", "velit", "esse",
        "cillum", "fugiat", "nulla", "pariatur", "excepteur", "sint", "occaecat", "cupidatat",
        "non", "proident", "sunt", "culpa", "qui", "officia", "deserunt", "mollit", "anim",
        "id", "est", "laborum"
    };

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var unit = parameters.Get<string>("unit") ?? "paragraphs"; // paragraphs, words, or characters
            var count = parameters.Get<int>("count");
            if (count <= 0) count = 1;
            if (count > 1000) count = 1000; // Limit

            var startWithLorem = parameters.Get<bool>("startWithLorem");
            if (!parameters.Values.ContainsKey("startWithLorem"))
                startWithLorem = true;

            ReportProgress(progress, 30, $"Generating {count} {unit}...");

            string output;
            switch (unit.ToLowerInvariant())
            {
                case "paragraphs":
                    output = GenerateParagraphs(count, startWithLorem);
                    break;
                case "words":
                    output = GenerateWords(count, startWithLorem);
                    break;
                case "characters":
                    output = GenerateCharacters(count, startWithLorem);
                    break;
                default:
                    return Failure($"Invalid unit: {unit}. Must be 'paragraphs', 'words', or 'characters'");
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Generated {count} {unit}", new Dictionary<string, object>
            {
                ["output"] = output,
                ["count"] = count,
                ["unit"] = unit
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to generate Lorem Ipsum: {ex.Message}", ex);
        }
    }

    private string GenerateParagraphs(int count, bool startWithLorem)
    {
        var random = new Random();
        var paragraphs = new List<string>();

        for (int i = 0; i < count; i++)
        {
            var sentenceCount = random.Next(3, 8);
            var sentences = new List<string>();

            for (int j = 0; j < sentenceCount; j++)
            {
                var wordCount = random.Next(5, 15);
                var words = new List<string>();

                for (int k = 0; k < wordCount; k++)
                {
                    if (i == 0 && j == 0 && k == 0 && startWithLorem)
                        words.Add("Lorem");
                    else if (i == 0 && j == 0 && k == 1 && startWithLorem)
                        words.Add("ipsum");
                    else
                        words.Add(Words[random.Next(Words.Length)]);
                }

                var sentence = string.Join(" ", words);
                sentence = char.ToUpper(sentence[0]) + sentence.Substring(1) + ".";
                sentences.Add(sentence);
            }

            paragraphs.Add(string.Join(" ", sentences));
        }

        return string.Join("\n\n", paragraphs);
    }

    private string GenerateWords(int count, bool startWithLorem)
    {
        var random = new Random();
        var words = new List<string>();

        for (int i = 0; i < count; i++)
        {
            if (i == 0 && startWithLorem)
                words.Add("Lorem");
            else if (i == 1 && startWithLorem)
                words.Add("ipsum");
            else
                words.Add(Words[random.Next(Words.Length)]);
        }

        return string.Join(" ", words);
    }

    private string GenerateCharacters(int count, bool startWithLorem)
    {
        var text = GenerateWords(count / 5 + 10, startWithLorem);
        return text.Substring(0, Math.Min(count, text.Length));
    }
}
