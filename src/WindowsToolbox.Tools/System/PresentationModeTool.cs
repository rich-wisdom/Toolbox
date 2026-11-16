using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class PresentationModeTool : BaseTool
{
    public override string Id => "presentation-mode";
    public override string Name => "Presentation Mode";
    public override string Description => "Quick presentation setup with one click";
    public override string Category => "system";
    public override string Icon => "Presentation";

    [DllImport("kernel32.dll")]
    private static extern uint SetThreadExecutionState(uint esFlags);

    private const uint ES_CONTINUOUS = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    private static bool _isActive = false;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var enable = parameters.Get<bool>("enable");

            if (enable)
            {
                ReportProgress(progress, 20, "Enabling presentation mode...");

                SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);

                ReportProgress(progress, 40, "Disabling notifications...");
                await SetFocusAssist(true, cancellationToken);

                ReportProgress(progress, 60, "Setting high performance power plan...");
                await SetHighPerformancePowerPlan(cancellationToken);

                _isActive = true;
                return Success("Presentation mode enabled");
            }
            else
            {
                ReportProgress(progress, 30, "Disabling presentation mode...");

                SetThreadExecutionState(ES_CONTINUOUS);

                ReportProgress(progress, 60, "Restoring notifications...");
                await SetFocusAssist(false, cancellationToken);

                _isActive = false;
                return Success("Presentation mode disabled");
            }
        }
        catch (Exception ex)
        {
            return Failure($"Error toggling presentation mode: {ex.Message}", ex);
        }
    }

    private async Task SetFocusAssist(bool enable, CancellationToken cancellationToken)
    {
        try
        {
            var keyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\CloudStore\Store\Cache\DefaultAccount\$quiethoursprofile$windows.data.notifications.quiethoursprofile";
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                key.SetValue("Data", enable ? new byte[] { 2 } : new byte[] { 0 });
            }
        }
        catch { }
        await Task.CompletedTask;
    }

    private async Task SetHighPerformancePowerPlan(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powercfg",
                Arguments = "/setactive 8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync(cancellationToken);
            }
        }
        catch { }
    }
}
