using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class WiFiPasswordTool : BaseTool
{
    public override string Id => "wifi-password";
    public override string Name => "Wi-Fi Password & QR";
    public override string Description => "View saved Wi-Fi passwords and generate QR codes";
    public override string Category => "system";
    public override string Icon => "Wifi";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var networkName = parameters.Get<string>("networkName");

            ReportProgress(progress, 20, "Retrieving Wi-Fi networks...");

            if (string.IsNullOrEmpty(networkName))
            {
                var networks = await GetSavedNetworks(cancellationToken);
                return Success($"Found {networks.Count} saved networks", new Dictionary<string, object> { ["networks"] = networks });
            }

            ReportProgress(progress, 50, $"Retrieving password for {networkName}...");

            var password = await GetNetworkPassword(networkName, cancellationToken);
            
            if (string.IsNullOrEmpty(password))
            {
                return Failure($"Could not retrieve password for network '{networkName}'. Make sure the network name is correct.");
            }

            var result = new Dictionary<string, object>
            {
                ["networkName"] = networkName,
                ["password"] = password,
                ["qrData"] = $"WIFI:T:WPA;S:{networkName};P:{password};;"
            };

            ReportProgress(progress, 100, "Complete");

            return Success($"Password retrieved for {networkName}", result);
        }
        catch (Exception ex)
        {
            return Failure($"Error retrieving Wi-Fi information: {ex.Message}", ex);
        }
    }

    private async Task<List<string>> GetSavedNetworks(CancellationToken cancellationToken)
    {
        var networks = new List<string>();
        
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = "wlan show profiles",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                var regex = new Regex(@"All User Profile\s*:\s*(.+)");
                var matches = regex.Matches(output);

                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        networks.Add(match.Groups[1].Value.Trim());
                    }
                }
            }
        }
        catch { }

        return networks;
    }

    private async Task<string?> GetNetworkPassword(string networkName, CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = $"wlan show profile name=\"{networkName}\" key=clear",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                await process.WaitForExitAsync(cancellationToken);

                var regex = new Regex(@"Key Content\s*:\s*(.+)");
                var match = regex.Match(output);

                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value.Trim();
                }
            }
        }
        catch { }

        return null;
    }
}
