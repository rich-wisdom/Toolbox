using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Developer;

public class HashGeneratorTool : BaseTool
{
    public override string Id => "generate-hash";
    public override string Name => "Generate Hash";
    public override string Description => "Generate cryptographic hashes";
    public override string Category => "developer";
    public override string Icon => "Code";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var text = parameters.Get<string>("text");
            var filePath = parameters.Get<string>("file_path");
            var algorithmsObj = parameters.Get<object>("algorithms");

            if (string.IsNullOrWhiteSpace(text) && string.IsNullOrWhiteSpace(filePath))
                return Failure("Either text or file_path is required");

            var algorithms = ParseAlgorithmsArray(algorithmsObj);
            if (algorithms.Count == 0)
                algorithms = new List<string> { "sha256" };

            ReportProgress(progress, 10, "Preparing data...");
            await Task.Delay(50, cancellationToken);

            byte[] bytes;
            if (!string.IsNullOrWhiteSpace(text))
            {
                bytes = Encoding.UTF8.GetBytes(text);
            }
            else
            {
                if (!System.IO.File.Exists(filePath!))
                    return Failure($"File not found: {filePath}");

                bytes = await System.IO.File.ReadAllBytesAsync(filePath!, cancellationToken);
            }

            ReportProgress(progress, 50, "Computing hashes...");
            
            var hashes = new Dictionary<string, string>();
            foreach (var algorithm in algorithms)
            {
                var hash = ComputeHash(bytes, algorithm.ToLowerInvariant());
                if (hash != null)
                    hashes[algorithm] = hash;
            }

            ReportProgress(progress, 100, "Complete");

            return Success($"Generated {hashes.Count} hashes", new Dictionary<string, object>
            {
                ["hashes"] = hashes
            });
        }
        catch (Exception ex)
        {
            return Failure($"Failed to generate hash: {ex.Message}", ex);
        }
    }

    private string? ComputeHash(byte[] bytes, string algorithm)
    {
        try
        {
            HashAlgorithm? hashAlgorithm = algorithm switch
            {
                "md5" => MD5.Create(),
                "sha1" => SHA1.Create(),
                "sha256" => SHA256.Create(),
                "sha512" => SHA512.Create(),
                _ => null
            };

            if (hashAlgorithm == null) return null;

            var hashBytes = hashAlgorithm.ComputeHash(bytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
        catch
        {
            return null;
        }
    }

    private List<string> ParseAlgorithmsArray(object? obj)
    {
        if (obj is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
        {
            return jsonElement.EnumerateArray()
                .Select(e => e.GetString() ?? "")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
        }
        return new List<string>();
    }
}
