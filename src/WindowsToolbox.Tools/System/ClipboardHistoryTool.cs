using System.Runtime.InteropServices;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class ClipboardHistoryTool : BaseTool
{
    public override string Id => "clipboard-history";
    public override string Name => "Clipboard History";
    public override string Description => "Manage and search clipboard history";
    public override string Category => "system";
    public override string Icon => "Clipboard";

    private static readonly List<ClipboardEntry> _history = new();
    private const int MaxHistoryItems = 100;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var action = parameters.Get<string>("action") ?? "list";

            switch (action.ToLower())
            {
                case "list":
                    return Success($"Clipboard history contains {_history.Count} items", 
                        new Dictionary<string, object> { ["history"] = _history.Take(50).ToList() });

                case "clear":
                    _history.Clear();
                    return Success("Clipboard history cleared");

                case "search":
                    var query = parameters.Get<string>("query") ?? "";
                    var results = _history
                        .Where(e => e.Content.Contains(query, StringComparison.OrdinalIgnoreCase))
                        .Take(20)
                        .ToList();
                    return Success($"Found {results.Count} matching entries", 
                        new Dictionary<string, object> { ["results"] = results });

                case "enable":
                    return Success("Clipboard monitoring is a background service that needs to be started separately");

                default:
                    return Failure($"Unknown action: {action}");
            }
        }
        catch (Exception ex)
        {
            return Failure($"Error managing clipboard history: {ex.Message}", ex);
        }
    }

    public static void AddToHistory(string content, string contentType = "text")
    {
        if (string.IsNullOrWhiteSpace(content) || content.Length > 10000)
            return;

        var entry = new ClipboardEntry
        {
            Id = Guid.NewGuid().ToString(),
            Content = content,
            ContentType = contentType,
            Timestamp = DateTime.Now
        };

        _history.Insert(0, entry);

        if (_history.Count > MaxHistoryItems)
        {
            _history.RemoveAt(_history.Count - 1);
        }
    }

    private class ClipboardEntry
    {
        public string Id { get; set; } = "";
        public string Content { get; set; } = "";
        public string ContentType { get; set; } = "";
        public DateTime Timestamp { get; set; }
    }
}
