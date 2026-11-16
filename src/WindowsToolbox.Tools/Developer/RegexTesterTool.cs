using System.Text.RegularExpressions;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class RegexTesterTool : BaseTool
{
    public override string Id => "regex-tester";
    public override string Name => "Regex Tester";
    public override string Description => "Test regular expressions and find matches";
    public override string Category => "developer";
    public override string Icon => "Filter";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var pattern = parameters.Get<string>("pattern");
            if (string.IsNullOrWhiteSpace(pattern))
                return Failure("Regex pattern is required");

            var text = parameters.Get<string>("text") ?? "";
            var ignoreCase = parameters.Get<bool>("ignoreCase");
            var multiline = parameters.Get<bool>("multiline");
            var singleline = parameters.Get<bool>("singleline");

            ReportProgress(progress, 30, "Compiling regex...");

            var options = RegexOptions.None;
            if (ignoreCase) options |= RegexOptions.IgnoreCase;
            if (multiline) options |= RegexOptions.Multiline;
            if (singleline) options |= RegexOptions.Singleline;

            Regex regex;
            try
            {
                regex = new Regex(pattern, options);
            }
            catch (ArgumentException ex)
            {
                return Failure($"Invalid regex pattern: {ex.Message}", ex);
            }

            ReportProgress(progress, 60, "Finding matches...");

            var matches = regex.Matches(text);
            var matchList = new List<Dictionary<string, object>>();

            foreach (Match match in matches)
            {
                var groups = new List<Dictionary<string, object>>();
                for (int i = 0; i < match.Groups.Count; i++)
                {
                    var group = match.Groups[i];
                    groups.Add(new Dictionary<string, object>
                    {
                        ["index"] = i,
                        ["value"] = group.Value,
                        ["start"] = group.Index,
                        ["length"] = group.Length
                    });
                }

                matchList.Add(new Dictionary<string, object>
                {
                    ["value"] = match.Value,
                    ["index"] = match.Index,
                    ["length"] = match.Length,
                    ["groups"] = groups
                });
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Found {matches.Count} match(es)", new Dictionary<string, object>
            {
                ["matches"] = matchList,
                ["count"] = matches.Count,
                ["pattern"] = pattern
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to test regex: {ex.Message}", ex);
        }
    }
}
