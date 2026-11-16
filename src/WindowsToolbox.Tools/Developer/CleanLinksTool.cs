using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class CleanLinksTool : BaseTool
{
    public override string Id => "clean-links";
    public override string Name => "Clean Links";
    public override string Description => "Remove tracking parameters from URLs";
    public override string Category => "developer";
    public override string Icon => "Link";

    private static readonly string[] TrackingParameters = new[]
    {
        "utm_source", "utm_medium", "utm_campaign", "utm_term", "utm_content",
        "fbclid", "gclid", "msclkid", "mc_cid", "mc_eid",
        "_ga", "_gl", "ref", "referrer"
    };

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = parameters.Get<string>("input");
            if (string.IsNullOrWhiteSpace(input))
                return Failure("URL(s) required");

            var customParams = parameters.Get<string[]>("customParams") ?? Array.Empty<string>();
            var paramsToRemove = TrackingParameters.Concat(customParams).ToHashSet(StringComparer.OrdinalIgnoreCase);

            ReportProgress(progress, 30, "Cleaning URLs...");

            var urls = input.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            var cleanedUrls = new List<Dictionary<string, object>>();

            for (int i = 0; i < urls.Length; i++)
            {
                var url = urls[i].Trim();
                if (string.IsNullOrWhiteSpace(url))
                    continue;

                var cleaned = CleanUrl(url, paramsToRemove);
                cleanedUrls.Add(new Dictionary<string, object>
                {
                    ["original"] = url,
                    ["cleaned"] = cleaned,
                    ["changed"] = url != cleaned
                });

                if (i % 10 == 0)
                    ReportProgress(progress, 30 + (int)((i / (double)urls.Length) * 60), $"Cleaned {i + 1}/{urls.Length}...");
            }

            ReportProgress(progress, 100, "Complete");

            var changedCount = cleanedUrls.Count(u => (bool)u["changed"]);
            return Success($"Cleaned {cleanedUrls.Count} URL(s), {changedCount} modified", new Dictionary<string, object>
            {
                ["urls"] = cleanedUrls,
                ["total"] = cleanedUrls.Count,
                ["changed"] = changedCount
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to clean URLs: {ex.Message}", ex);
        }
    }

    private string CleanUrl(string url, HashSet<string> paramsToRemove)
    {
        try
        {
            var uri = new Uri(url);
            
            if (string.IsNullOrEmpty(uri.Query))
                return url;

            var query = uri.Query.TrimStart('?');
            var parameters = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
            
            var cleanedParams = parameters
                .Where(p =>
                {
                    var key = p.Split('=')[0];
                    return !paramsToRemove.Contains(key);
                })
                .ToList();

            var builder = new UriBuilder(uri)
            {
                Query = cleanedParams.Count > 0 ? string.Join("&", cleanedParams) : ""
            };

            return builder.Uri.ToString();
        }
        catch
        {
            return url; // Return original if parsing fails
        }
    }
}
