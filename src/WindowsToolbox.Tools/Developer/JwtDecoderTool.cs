using System.Text;
using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class JwtDecoderTool : BaseTool
{
    public override string Id => "jwt-decoder";
    public override string Name => "JWT Decoder";
    public override string Description => "Decode JWT tokens";
    public override string Category => "developer";
    public override string Icon => "Key";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var token = parameters.Get<string>("token");
            if (string.IsNullOrWhiteSpace(token))
                return Failure("JWT token is required");

            ReportProgress(progress, 30, "Parsing JWT...");

            var parts = token.Split('.');
            if (parts.Length != 3)
                return Failure("Invalid JWT format. Expected 3 parts separated by dots.");

            ReportProgress(progress, 50, "Decoding header...");
            var header = DecodeBase64Url(parts[0]);
            
            ReportProgress(progress, 70, "Decoding payload...");
            var payload = DecodeBase64Url(parts[1]);

            var signature = parts[2];

            ReportProgress(progress, 90, "Parsing expiration...");
            string? expirationInfo = null;
            try
            {
                var payloadDoc = JsonDocument.Parse(payload);
                if (payloadDoc.RootElement.TryGetProperty("exp", out var expElement))
                {
                    var exp = expElement.GetInt64();
                    var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp);
                    var now = DateTimeOffset.UtcNow;
                    var isExpired = expirationDate < now;
                    var timeRemaining = expirationDate - now;
                    
                    expirationInfo = $"{expirationDate:yyyy-MM-dd HH:mm:ss} UTC";
                    if (isExpired)
                        expirationInfo += $" (expired {Math.Abs(timeRemaining.TotalDays):F1} days ago)";
                    else
                        expirationInfo += $" (expires in {timeRemaining.TotalDays:F1} days)";
                }
            }
            catch
            {
                // Ignore expiration parsing errors
            }

            ReportProgress(progress, 100, "Complete");

            var result = new Dictionary<string, object>
            {
                ["header"] = header,
                ["payload"] = payload,
                ["signature"] = signature
            };

            if (expirationInfo != null)
                result["expiration"] = expirationInfo;

            return Success("JWT decoded successfully", result);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to decode JWT: {ex.Message}", ex);
        }
    }

    private string DecodeBase64Url(string base64Url)
    {
        var base64 = base64Url.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        var bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}
