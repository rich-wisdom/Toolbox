using System.Runtime.InteropServices;
using Microsoft.Win32;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class MouseFixesTool : BaseTool
{
    public override string Id => "mouse-fixes";
    public override string Name => "Mouse Fixes";
    public override string Description => "Fix mouse acceleration and sensitivity";
    public override string Category => "system";
    public override string Icon => "Mouse";

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);

    private const uint SPI_SETMOUSESPEED = 0x0071;
    private const uint SPI_GETMOUSESPEED = 0x0070;
    private const uint SPIF_UPDATEINIFILE = 0x01;
    private const uint SPIF_SENDCHANGE = 0x02;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var action = parameters.Get<string>("action") ?? "disable-acceleration";

            switch (action.ToLower())
            {
                case "disable-acceleration":
                    ReportProgress(progress, 50, "Disabling mouse acceleration...");
                    await Task.Run(() => DisableMouseAcceleration(), cancellationToken);
                    return Success("Mouse acceleration disabled (Enhance pointer precision turned off)");

                case "enable-acceleration":
                    ReportProgress(progress, 50, "Enabling mouse acceleration...");
                    await Task.Run(() => EnableMouseAcceleration(), cancellationToken);
                    return Success("Mouse acceleration enabled");

                case "reset":
                    ReportProgress(progress, 50, "Resetting mouse settings...");
                    await Task.Run(() => ResetMouseSettings(), cancellationToken);
                    return Success("Mouse settings reset to Windows defaults");

                case "gaming-mode":
                    ReportProgress(progress, 50, "Applying gaming optimizations...");
                    await Task.Run(() => ApplyGamingSettings(), cancellationToken);
                    return Success("Gaming mouse settings applied");

                default:
                    return Failure($"Unknown action: {action}");
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Failure("Access denied. Administrator privileges may be required.");
        }
        catch (Exception ex)
        {
            return Failure($"Error adjusting mouse settings: {ex.Message}", ex);
        }
    }

    private void DisableMouseAcceleration()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
            if (key != null)
            {
                key.SetValue("MouseSpeed", "0");
                key.SetValue("MouseThreshold1", "0");
                key.SetValue("MouseThreshold2", "0");
            }

            var mouseParams = new[] { 0, 0, 0 };
            var ptr = Marshal.AllocHGlobal(mouseParams.Length * sizeof(int));
            try
            {
                Marshal.Copy(mouseParams, 0, ptr, mouseParams.Length);
                SystemParametersInfo(0x0004, 0, ptr, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }
        catch { }
    }

    private void EnableMouseAcceleration()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
            if (key != null)
            {
                key.SetValue("MouseSpeed", "1");
                key.SetValue("MouseThreshold1", "6");
                key.SetValue("MouseThreshold2", "10");
            }
        }
        catch { }
    }

    private void ResetMouseSettings()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
            if (key != null)
            {
                key.SetValue("MouseSpeed", "1");
                key.SetValue("MouseThreshold1", "6");
                key.SetValue("MouseThreshold2", "10");
                key.SetValue("MouseSensitivity", "10");
            }
        }
        catch { }
    }

    private void ApplyGamingSettings()
    {
        DisableMouseAcceleration();
        
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Control Panel\Mouse");
            if (key != null)
            {
                key.SetValue("MouseSensitivity", "10");
            }
        }
        catch { }
    }
}
