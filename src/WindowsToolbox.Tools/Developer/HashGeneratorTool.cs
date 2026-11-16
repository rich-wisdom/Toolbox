using System.Security.Cryptography;
using System.Text;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class HashGeneratorTool : BaseTool
{
    public override string Id => "hash-generator";
    public override string Name => "Hash Generator";
    public override string Description => "Generate MD5, SHA-1, SHA-256, SHA-512 hashes";
    public override string Category => "developer";
    public override string Icon => "Fingerprint";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var input = parameters.Get<string>("input");
            if (string.IsNullOrWhiteSpace(input))
                return Failure("Input is required");

            ReportProgress(progress, 20, "Computing hashes...");

            var bytes = Encoding.UTF8.GetBytes(input);
            var hashes = new Dictionary<string, object>();

            ReportProgress(progress, 30, "Computing MD5...");
            hashes["md5"] = ComputeHash(MD5.Create(), bytes);

            ReportProgress(progress, 50, "Computing SHA-1...");
            hashes["sha1"] = ComputeHash(SHA1.Create(), bytes);

            ReportProgress(progress, 70, "Computing SHA-256...");
            hashes["sha256"] = ComputeHash(SHA256.Create(), bytes);

            ReportProgress(progress, 90, "Computing SHA-512...");
            hashes["sha512"] = ComputeHash(SHA512.Create(), bytes);

            ReportProgress(progress, 100, "Complete");

            return Success("Hashes generated successfully", hashes);
        }
        catch (Exception ex)
        {
            return Failure($"Failed to generate hashes: {ex.Message}", ex);
        }
    }

    private string ComputeHash(HashAlgorithm algorithm, byte[] bytes)
    {
        using (algorithm)
        {
            var hash = algorithm.ComputeHash(bytes);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
