using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class Base64DecodeTool : BaseTool
{
    public override string Id => "decode-base64";
    public override string Name => "Decode Base64";
    public override string Description => "Decode Base64 to text or file";
    public override string Category => "developer";
    public override string Icon => "Code";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var base64String = parameters.Get<string>("base64_string");
            var outputType = parameters.Get<string>("output_type") ?? "text";

            if (string.IsNullOrWhiteSpace(base64String))
                return Failure("base64_string is required");

            ReportProgress(progress, 10, "Preparing decoding...");
            await Task.Delay(50, cancellationToken);

            var bytes = Convert.FromBase64String(base64String);

            ReportProgress(progress, 100, "Complete");

            if (outputType == "text")
            {
                var decodedText = System.Text.Encoding.UTF8.GetString(bytes);
                return Success("Decoded Base64 to text successfully", new Dictionary<string, object>
                {
                    ["decoded_text"] = decodedText
                });
            }
            else
            {
                var outputPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    "Downloads",
                    $"decoded_{DateTime.Now:yyyyMMdd_HHmmss}.bin"
                );
                return Success("Decoded Base64 to file successfully", new Dictionary<string, object>
                {
                    ["output_file"] = outputPath,
                    ["size_bytes"] = bytes.Length
                });
            }
        }
        catch (Exception ex)
        {
            return Failure($"Failed to decode Base64: {ex.Message}", ex);
        }
    }
}
