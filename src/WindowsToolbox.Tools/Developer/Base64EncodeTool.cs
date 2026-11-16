using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class Base64EncodeTool : BaseTool
{
    public override string Id => "encode-base64";
    public override string Name => "Encode Base64";
    public override string Description => "Encode text or files to Base64";
    public override string Category => "developer";
    public override string Icon => "Code";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var text = parameters.Get<string>("text");
            var filePath = parameters.Get<string>("file_path");

            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(filePath))
                return Failure("Either text or file_path is required");

            ReportProgress(progress, 10, "Preparing encoding...");
            await Task.Delay(50, cancellationToken);

            string base64Result;
            if (!string.IsNullOrWhiteSpace(text))
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(text);
                base64Result = Convert.ToBase64String(bytes);
            }
            else
            {
                if (!System.IO.File.Exists(filePath!))
                    return Failure($"File not found: {filePath}");

                var bytes = await System.IO.File.ReadAllBytesAsync(filePath!, cancellationToken);
                base64Result = Convert.ToBase64String(bytes);
            }

            ReportProgress(progress, 100, "Complete");

            return Success("Encoded to Base64 successfully", new Dictionary<string, object>
            {
                ["base64"] = base64Result,
                ["length"] = base64Result.Length
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to encode Base64: {ex.Message}", ex);
        }
    }
}
