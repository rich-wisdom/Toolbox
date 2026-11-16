using System.Runtime.InteropServices;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class DisplayControlTool : BaseTool
{
    public override string Id => "display-control";
    public override string Name => "Display Control";
    public override string Description => "Control external monitor settings";
    public override string Category => "system";
    public override string Icon => "Monitor";

    [DllImport("dxva2.dll", SetLastError = true)]
    private static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumProc lpfnEnum, IntPtr dwData);

    private delegate bool MonitorEnumProc(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var action = parameters.Get<string>("action") ?? "list";

            switch (action.ToLower())
            {
                case "list":
                    ReportProgress(progress, 50, "Detecting monitors...");
                    var monitorCount = await Task.Run(() => GetMonitorCount(), cancellationToken);
                    return Success($"Detected {monitorCount} monitors", 
                        new Dictionary<string, object> { ["monitorCount"] = monitorCount });

                case "brightness":
                    return Failure("Brightness control is not implemented; DDC/CI support is required.");

                default:
                    return Failure($"Unknown action: {action}");
            }
        }
        catch (Exception ex)
        {
            return Failure($"Error controlling displays: {ex.Message}", ex);
        }
    }

    private int GetMonitorCount()
    {
        var count = 0;
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
            (IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData) =>
            {
                count++;
                return true;
            },
            IntPtr.Zero);
        return count;
    }
}
