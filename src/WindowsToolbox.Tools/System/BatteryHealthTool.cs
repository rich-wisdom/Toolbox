using System.Diagnostics;
using System.Management;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class BatteryHealthTool : BaseTool
{
    public override string Id => "battery-health";
    public override string Name => "Battery Health";
    public override string Description => "Monitor battery health and generate reports";
    public override string Category => "system";
    public override string Icon => "Battery";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            ReportProgress(progress, 10, "Checking battery information...");

            var batteryInfo = await Task.Run(() => GetBatteryInfo(), cancellationToken);
            
            if (batteryInfo == null)
            {
                return Failure("No battery detected. This device may not have a battery.");
            }

            ReportProgress(progress, 50, "Generating battery report...");

            var reportPath = Path.Combine(Path.GetTempPath(), $"battery-report-{DateTime.Now:yyyyMMdd-HHmmss}.html");
            var generateReport = parameters.Get<bool>("generateReport");
            
            if (generateReport)
            {
                await GenerateBatteryReport(reportPath, cancellationToken);
                batteryInfo["reportPath"] = reportPath;
            }

            ReportProgress(progress, 100, "Complete");

            return Success("Battery health information retrieved successfully", batteryInfo);
        }
        catch (Exception ex)
        {
            return Failure($"Error retrieving battery health: {ex.Message}", ex);
        }
    }

    private Dictionary<string, object>? GetBatteryInfo()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Battery");
            var collection = searcher.Get();

            if (collection.Count == 0)
                return null;

            foreach (ManagementObject obj in collection)
            {
                var designCapacity = Convert.ToInt32(obj["DesignCapacity"] ?? 0);
                var fullChargeCapacity = Convert.ToInt32(obj["FullChargeCapacity"] ?? 0);
                var estimatedChargeRemaining = Convert.ToInt32(obj["EstimatedChargeRemaining"] ?? 0);
                var batteryStatus = Convert.ToUInt16(obj["BatteryStatus"] ?? 0);
                var estimatedRunTime = Convert.ToUInt32(obj["EstimatedRunTime"] ?? 0);

                var wearPercentage = designCapacity > 0 
                    ? Math.Round((1 - (double)fullChargeCapacity / designCapacity) * 100, 2)
                    : 0;

                return new Dictionary<string, object>
                {
                    ["designCapacity"] = designCapacity,
                    ["fullChargeCapacity"] = fullChargeCapacity,
                    ["currentCharge"] = estimatedChargeRemaining,
                    ["wearPercentage"] = wearPercentage,
                    ["status"] = GetBatteryStatus(batteryStatus),
                    ["estimatedRunTime"] = estimatedRunTime,
                    ["chemistry"] = obj["Chemistry"]?.ToString() ?? "Unknown"
                };
            }
        }
        catch { }

        return null;
    }

    private async Task GenerateBatteryReport(string reportPath, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powercfg",
            Arguments = $"/batteryreport /output \"{reportPath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var process = Process.Start(psi);
        if (process != null)
        {
            await process.WaitForExitAsync(cancellationToken);
        }
    }

    private string GetBatteryStatus(ushort status)
    {
        return status switch
        {
            1 => "Discharging",
            2 => "AC Power",
            3 => "Fully Charged",
            4 => "Low",
            5 => "Critical",
            6 => "Charging",
            7 => "Charging and High",
            8 => "Charging and Low",
            9 => "Charging and Critical",
            _ => "Unknown"
        };
    }
}
