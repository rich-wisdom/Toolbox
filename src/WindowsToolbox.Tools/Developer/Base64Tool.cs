using System.Text;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class Base64Tool : BaseTool
{
    public override string Id => "base64";
    public override string Name => "Base64 Encoder/Decoder";
    public override string Description => "Encode and decode Base64 strings";
    public override string Category => "developer";
    public override string Icon => "Code";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = parameters.Get<string>("input");
            if (string.IsNullOrWhiteSpace(input))
                return Failure("Input is required");

            var mode = parameters.Get<string>("mode") ?? "encode"; // "encode" or "decode"

            ReportProgress(progress, 30, $"{(mode == "encode" ? "Encoding" : "Decoding")}...");

            string output;
            if (mode == "encode")
            {
                var bytes = Encoding.UTF8.GetBytes(input);
                output = Convert.ToBase64String(bytes);
            }
            else
            {
                var bytes = Convert.FromBase64String(input);
                output = Encoding.UTF8.GetString(bytes);
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Base64 {mode} completed successfully", new Dictionary<string, object>
            {
                ["output"] = output
            });
        }
        catch (FormatException ex)
        {
            return Failure("Invalid Base64 string", ex);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to process Base64: {ex.Message}", ex);
        }
    }
}
