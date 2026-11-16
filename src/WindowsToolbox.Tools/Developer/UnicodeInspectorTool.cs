using System.Globalization;
using System.Text;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class UnicodeInspectorTool : BaseTool
{
    public override string Id => "unicode-inspector";
    public override string Name => "Unicode Inspector";
    public override string Description => "Inspect Unicode characters and code points";
    public override string Category => "developer";
    public override string Icon => "TextFont";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = parameters.Get<string>("input");
            if (string.IsNullOrEmpty(input))
                return Failure("Input text is required");

            ReportProgress(progress, 30, "Analyzing characters...");

            var characters = new List<Dictionary<string, object>>();

            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                var charInfo = new Dictionary<string, object>
                {
                    ["char"] = c.ToString(),
                    ["codePoint"] = $"U+{((int)c):X4}",
                    ["decimal"] = (int)c,
                    ["html"] = $"&#{(int)c};",
                    ["utf8"] = BitConverter.ToString(Encoding.UTF8.GetBytes(c.ToString())),
                    ["utf16"] = BitConverter.ToString(Encoding.Unicode.GetBytes(c.ToString())),
                    ["category"] = CharUnicodeInfo.GetUnicodeCategory(c).ToString(),
                    ["isControl"] = char.IsControl(c),
                    ["isDigit"] = char.IsDigit(c),
                    ["isLetter"] = char.IsLetter(c),
                    ["isWhiteSpace"] = char.IsWhiteSpace(c)
                };

                try
                {
                    charInfo["name"] = char.GetUnicodeCategory(c) == UnicodeCategory.Control
                        ? $"<control-{(int)c:X4}>"
                        : c.ToString();
                }
                catch
                {
                    charInfo["name"] = "Unknown";
                }

                characters.Add(charInfo);

                if (i % 10 == 0)
                    ReportProgress(progress, 30 + (int)((i / (double)input.Length) * 60), $"Analyzed {i + 1}/{input.Length}...");
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Analyzed {characters.Count} character(s)", new Dictionary<string, object>
            {
                ["characters"] = characters,
                ["count"] = characters.Count,
                ["byteCount"] = Encoding.UTF8.GetByteCount(input)
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to inspect Unicode: {ex.Message}", ex);
        }
    }
}
