using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class TimestampConverterTool : BaseTool
{
    public override string Id => "timestamp-converter";
    public override string Name => "Timestamp Converter";
    public override string Description => "Convert between Unix timestamps and date/time";
    public override string Category => "developer";
    public override string Icon => "Clock";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var mode = parameters.Get<string>("mode") ?? "toDate"; // "toDate" or "toTimestamp"
            
            ReportProgress(progress, 30, "Converting...");

            Dictionary<string, object> result;

            if (mode == "toDate")
            {
                var timestamp = parameters.Get<long>("timestamp");
                if (timestamp == 0)
                    return Failure("Timestamp is required");

                // Auto-detect milliseconds or seconds
                var isMilliseconds = timestamp > 10000000000;
                var dateTime = isMilliseconds 
                    ? DateTimeOffset.FromUnixTimeMilliseconds(timestamp)
                    : DateTimeOffset.FromUnixTimeSeconds(timestamp);

                result = new Dictionary<string, object>
                {
                    ["utc"] = dateTime.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["local"] = dateTime.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["iso8601"] = dateTime.ToString("o"),
                    ["unix"] = dateTime.ToUnixTimeSeconds(),
                    ["unixMs"] = dateTime.ToUnixTimeMilliseconds(),
                    ["format"] = isMilliseconds ? "milliseconds" : "seconds"
                };
            }
            else
            {
                var dateString = parameters.Get<string>("dateTime");
                if (string.IsNullOrWhiteSpace(dateString))
                    return Failure("Date/time is required");

                DateTimeOffset dateTime;
                if (dateString.ToLowerInvariant() == "now")
                {
                    dateTime = DateTimeOffset.UtcNow;
                }
                else if (!DateTimeOffset.TryParse(dateString, out dateTime))
                {
                    return Failure("Invalid date/time format");
                }

                result = new Dictionary<string, object>
                {
                    ["unix"] = dateTime.ToUnixTimeSeconds(),
                    ["unixMs"] = dateTime.ToUnixTimeMilliseconds(),
                    ["utc"] = dateTime.UtcDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    ["iso8601"] = dateTime.ToString("o")
                };
            }

            ReportProgress(progress, 100, "Complete");

            return Success("Conversion completed successfully", result);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to convert timestamp: {ex.Message}", ex);
        }
    }
}
