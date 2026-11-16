using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class UuidGeneratorTool : BaseTool
{
    public override string Id => "uuid-generator";
    public override string Name => "UUID Generator";
    public override string Description => "Generate UUIDs (v4)";
    public override string Category => "developer";
    public override string Icon => "Tag";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var count = parameters.Get<int>("count");
            if (count <= 0) count = 1;
            if (count > 100) count = 100; // Limit to 100 UUIDs

            var uppercase = parameters.Get<bool>("uppercase");
            var withHyphens = parameters.Get<bool>("withHyphens");
            if (!parameters.Values.ContainsKey("withHyphens"))
                withHyphens = true; // Default to with hyphens

            ReportProgress(progress, 30, $"Generating {count} UUID(s)...");

            var uuids = new List<string>();
            for (int i = 0; i < count; i++)
            {
                var uuid = Guid.NewGuid().ToString();
                
                if (!withHyphens)
                    uuid = uuid.Replace("-", "");
                
                if (uppercase)
                    uuid = uuid.ToUpperInvariant();

                uuids.Add(uuid);

                if (i % 10 == 0)
                    ReportProgress(progress, 30 + (int)((i / (double)count) * 60), $"Generated {i + 1}/{count}...");
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Generated {count} UUID(s)", new Dictionary<string, object>
            {
                ["uuids"] = uuids,
                ["count"] = count
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to generate UUIDs: {ex.Message}", ex);
        }
    }
}
