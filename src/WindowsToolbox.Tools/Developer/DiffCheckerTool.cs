using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class DiffCheckerTool : BaseTool
{
    public override string Id => "diff-checker";
    public override string Name => "Diff Checker";
    public override string Description => "Compare two texts and highlight differences";
    public override string Category => "developer";
    public override string Icon => "Compare";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var text1 = parameters.Get<string>("text1") ?? "";
            var text2 = parameters.Get<string>("text2") ?? "";

            ReportProgress(progress, 30, "Analyzing differences...");

            var lines1 = text1.Split('\n');
            var lines2 = text2.Split('\n');

            var differences = new List<Dictionary<string, object>>();
            var lcs = ComputeLCS(lines1, lines2);

            ReportProgress(progress, 60, "Computing diff...");

            int i = 0, j = 0;
            foreach (var (line, inLcs) in lcs)
            {
                if (inLcs)
                {
                    differences.Add(new Dictionary<string, object>
                    {
                        ["type"] = "unchanged",
                        ["line1"] = i + 1,
                        ["line2"] = j + 1,
                        ["text"] = line
                    });
                    i++; j++;
                }
                else if (i < lines1.Length && lines1[i] == line)
                {
                    differences.Add(new Dictionary<string, object>
                    {
                        ["type"] = "removed",
                        ["line1"] = i + 1,
                        ["text"] = line
                    });
                    i++;
                }
                else
                {
                    differences.Add(new Dictionary<string, object>
                    {
                        ["type"] = "added",
                        ["line2"] = j + 1,
                        ["text"] = line
                    });
                    j++;
                }
            }

            ReportProgress(progress, 90, "Calculating statistics...");

            var stats = new Dictionary<string, object>
            {
                ["additions"] = differences.Count(d => d["type"].ToString() == "added"),
                ["deletions"] = differences.Count(d => d["type"].ToString() == "removed"),
                ["unchanged"] = differences.Count(d => d["type"].ToString() == "unchanged")
            };

            ReportProgress(progress, 100, "Complete");

            return Success("Diff computed successfully", new Dictionary<string, object>
            {
                ["differences"] = differences,
                ["statistics"] = stats
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to compute diff: {ex.Message}", ex);
        }
    }

    private List<(string line, bool inLcs)> ComputeLCS(string[] lines1, string[] lines2)
    {
        var m = lines1.Length;
        var n = lines2.Length;
        var dp = new int[m + 1, n + 1];

        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {
                if (lines1[i - 1] == lines2[j - 1])
                    dp[i, j] = dp[i - 1, j - 1] + 1;
                else
                    dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
            }
        }

        var result = new List<(string, bool)>();
        int x = m, y = n;
        var lcsLines = new HashSet<string>();

        while (x > 0 && y > 0)
        {
            if (lines1[x - 1] == lines2[y - 1])
            {
                lcsLines.Add(lines1[x - 1]);
                x--; y--;
            }
            else if (dp[x - 1, y] > dp[x, y - 1])
                x--;
            else
                y--;
        }

        var seen = new HashSet<string>();
        foreach (var line in lines1)
        {
            result.Add((line, lcsLines.Contains(line) && !seen.Contains(line)));
            if (lcsLines.Contains(line))
                seen.Add(line);
        }

        foreach (var line in lines2)
        {
            if (!lcsLines.Contains(line) || seen.Contains(line))
                result.Add((line, false));
        }

        return result;
    }
}
