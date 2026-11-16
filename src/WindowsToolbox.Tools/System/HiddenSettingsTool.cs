using Microsoft.Win32;
using System.Diagnostics;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class HiddenSettingsTool : BaseTool
{
    public override string Id => "hidden-settings";
    public override string Name => "Hidden Settings";
    public override string Description => "Access Windows hidden features and tweaks";
    public override string Category => "system";
    public override string Icon => "Settings";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var setting = parameters.Get<string>("setting") ?? "list";

            switch (setting.ToLower())
            {
                case "list":
                    var settings = GetAvailableSettings();
                    return Success($"Found {settings.Count} hidden settings", 
                        new Dictionary<string, object> { ["settings"] = settings });

                case "show-file-extensions":
                    ReportProgress(progress, 50, "Enabling file extensions...");
                    await Task.Run(() => SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0), cancellationToken);
                    return Success("File extensions are now visible. Restart Explorer to see changes.");

                case "hide-file-extensions":
                    ReportProgress(progress, 50, "Hiding file extensions...");
                    await Task.Run(() => SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 1), cancellationToken);
                    return Success("File extensions are now hidden. Restart Explorer to see changes.");

                case "show-hidden-files":
                    ReportProgress(progress, 50, "Showing hidden files...");
                    await Task.Run(() => SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1), cancellationToken);
                    return Success("Hidden files are now visible. Restart Explorer to see changes.");

                case "hide-hidden-files":
                    ReportProgress(progress, 50, "Hiding hidden files...");
                    await Task.Run(() => SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 2), cancellationToken);
                    return Success("Hidden files are now hidden. Restart Explorer to see changes.");

                case "disable-bing-search":
                    ReportProgress(progress, 50, "Disabling Bing in Start Menu...");
                    await Task.Run(() => SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 0), cancellationToken);
                    return Success("Bing search in Start Menu disabled. Restart Explorer to see changes.");

                case "enable-bing-search":
                    ReportProgress(progress, 50, "Enabling Bing in Start Menu...");
                    await Task.Run(() => SetRegistryValue(@"Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled", 1), cancellationToken);
                    return Success("Bing search in Start Menu enabled. Restart Explorer to see changes.");

                case "classic-context-menu":
                    if (IsWindows11())
                    {
                        ReportProgress(progress, 50, "Enabling classic context menu...");
                        await EnableClassicContextMenu(cancellationToken);
                        return Success("Classic context menu enabled. Restart Explorer to see changes.");
                    }
                    return Success("Classic context menu tweak is only available on Windows 11");

                case "god-mode":
                    ReportProgress(progress, 50, "Creating God Mode folder...");
                    var godModePath = await CreateGodModeFolder(cancellationToken);
                    return Success($"God Mode folder created at: {godModePath}");

                default:
                    return Failure($"Unknown setting: {setting}");
            }
        }
        catch (UnauthorizedAccessException)
        {
            return Failure("Access denied. Administrator privileges may be required for some settings.");
        }
        catch (Exception ex)
        {
            return Failure($"Error changing setting: {ex.Message}", ex);
        }
    }

    private List<object> GetAvailableSettings()
    {
        return new List<object>
        {
            new { id = "show-file-extensions", name = "Show File Extensions", category = "Explorer" },
            new { id = "hide-file-extensions", name = "Hide File Extensions", category = "Explorer" },
            new { id = "show-hidden-files", name = "Show Hidden Files", category = "Explorer" },
            new { id = "hide-hidden-files", name = "Hide Hidden Files", category = "Explorer" },
            new { id = "disable-bing-search", name = "Disable Bing in Start Menu", category = "Search" },
            new { id = "enable-bing-search", name = "Enable Bing in Start Menu", category = "Search" },
            new { id = "classic-context-menu", name = "Classic Context Menu (Win11)", category = "Explorer" },
            new { id = "god-mode", name = "Create God Mode Folder", category = "System" }
        };
    }

    private void SetRegistryValue(string keyPath, string valueName, int value)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(keyPath);
            if (key != null)
            {
                key.SetValue(valueName, value, RegistryValueKind.DWord);
            }
        }
        catch { }
    }

    private bool IsWindows11()
    {
        var version = Environment.OSVersion.Version;
        return version.Major >= 10 && version.Build >= 22000;
    }

    private async Task EnableClassicContextMenu(CancellationToken cancellationToken)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = @"add ""HKCU\Software\Classes\CLSID\{86ca1aa0-34aa-4e8b-a509-50c905bae2a2}\InprocServer32"" /f /ve",
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

    private async Task<string> CreateGodModeFolder(CancellationToken cancellationToken)
    {
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var godModeFolderName = "GodMode.{ED7BA470-8E54-465E-825C-99712043E01C}";
        var godModePath = Path.Combine(desktopPath, godModeFolderName);

        await Task.Run(() =>
        {
            try
            {
                if (!Directory.Exists(godModePath))
                {
                    Directory.CreateDirectory(godModePath);
                }
            }
            catch { }
        }, cancellationToken);

        return godModePath;
    }
}
