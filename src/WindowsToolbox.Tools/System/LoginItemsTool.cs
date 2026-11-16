using Microsoft.Win32;
using System.Diagnostics;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class LoginItemsTool : BaseTool
{
    public override string Id => "login-items";
    public override string Name => "Login & Background Items";
    public override string Description => "Manage startup programs and services";
    public override string Category => "system";
    public override string Icon => "StartUp";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var action = parameters.Get<string>("action") ?? "list";

            switch (action.ToLower())
            {
                case "list":
                    ReportProgress(progress, 50, "Scanning startup items...");
                    var items = await Task.Run(() => GetStartupItems(), cancellationToken);
                    return Success($"Found {items.Count} startup items", 
                        new Dictionary<string, object> { ["startupItems"] = items });

                case "disable":
                    var itemName = parameters.Get<string>("itemName");
                    if (string.IsNullOrEmpty(itemName))
                        return Failure("Item name is required to disable");
                    
                    ReportProgress(progress, 50, $"Disabling {itemName}...");
                    var disabled = await Task.Run(() => DisableStartupItem(itemName), cancellationToken);
                    return disabled 
                        ? Success($"Disabled startup item: {itemName}")
                        : Failure($"Failed to disable {itemName}. Administrator privileges may be required.");

                case "enable":
                    var enableItemName = parameters.Get<string>("itemName");
                    if (string.IsNullOrEmpty(enableItemName))
                        return Failure("Item name is required to enable");
                    
                    return Success("Enabling startup items requires re-adding them through the application or task scheduler");

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
            return Failure($"Error managing startup items: {ex.Message}", ex);
        }
    }

    private List<object> GetStartupItems()
    {
        var items = new List<object>();

        var registryPaths = new[]
        {
            new { Key = Registry.CurrentUser, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", Scope = "User" },
            new { Key = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", Scope = "Machine" },
            new { Key = Registry.CurrentUser, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", Scope = "User" },
            new { Key = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce", Scope = "Machine" }
        };

        foreach (var regPath in registryPaths)
        {
            try
            {
                using var key = regPath.Key.OpenSubKey(regPath.Path);
                if (key != null)
                {
                    foreach (var valueName in key.GetValueNames())
                    {
                        var value = key.GetValue(valueName) as string;
                        if (!string.IsNullOrEmpty(value))
                        {
                            items.Add(new
                            {
                                name = valueName,
                                command = value,
                                location = regPath.Path,
                                scope = regPath.Scope,
                                enabled = true
                            });
                        }
                    }
                }
            }
            catch { }
        }

        var startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
        if (Directory.Exists(startupFolder))
        {
            try
            {
                foreach (var file in Directory.GetFiles(startupFolder))
                {
                    items.Add(new
                    {
                        name = Path.GetFileName(file),
                        command = file,
                        location = "Startup Folder",
                        scope = "User",
                        enabled = true
                    });
                }
            }
            catch { }
        }

        return items;
    }

    private bool DisableStartupItem(string itemName)
    {
        var registryPaths = new[]
        {
            new { Key = Registry.CurrentUser, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run" },
            new { Key = Registry.LocalMachine, Path = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run" }
        };

        foreach (var regPath in registryPaths)
        {
            try
            {
                using var key = regPath.Key.OpenSubKey(regPath.Path, true);
                if (key != null)
                {
                    var valueNames = key.GetValueNames();
                    if (valueNames.Contains(itemName))
                    {
                        key.DeleteValue(itemName);
                        return true;
                    }
                }
            }
            catch { }
        }

        return false;
    }
}
