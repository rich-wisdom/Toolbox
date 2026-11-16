using System.Text.Json;

namespace WindowsToolbox.MCP.Services;

public class AIAppSetup
{
    private readonly string _mcpExePath;

    public AIAppSetup(string? mcpExePath = null)
    {
        _mcpExePath = mcpExePath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),
            "Windows Toolbox",
            "WindowsToolbox.MCP.exe"
        );
    }

    public List<AIAppInfo> DetectInstalledApps()
    {
        var apps = new List<AIAppInfo>();

        var cursorConfigPath = GetCursorConfigPath();
        if (!string.IsNullOrEmpty(cursorConfigPath))
        {
            apps.Add(new AIAppInfo
            {
                Name = "Cursor",
                DisplayName = "Cursor IDE",
                ConfigPath = cursorConfigPath,
                IsInstalled = File.Exists(cursorConfigPath) || Directory.Exists(Path.GetDirectoryName(cursorConfigPath)!),
                Icon = "💻"
            });
        }

        var claudeConfigPath = GetClaudeDesktopConfigPath();
        if (!string.IsNullOrEmpty(claudeConfigPath))
        {
            apps.Add(new AIAppInfo
            {
                Name = "Claude Desktop",
                DisplayName = "Claude Desktop",
                ConfigPath = claudeConfigPath,
                IsInstalled = File.Exists(claudeConfigPath) || Directory.Exists(Path.GetDirectoryName(claudeConfigPath)!),
                Icon = "🤖"
            });
        }

        var vscodeConfigPath = GetVSCodeConfigPath();
        if (!string.IsNullOrEmpty(vscodeConfigPath))
        {
            apps.Add(new AIAppInfo
            {
                Name = "VS Code",
                DisplayName = "Visual Studio Code",
                ConfigPath = vscodeConfigPath,
                IsInstalled = File.Exists(vscodeConfigPath) || Directory.Exists(Path.GetDirectoryName(vscodeConfigPath)!),
                Icon = "📝"
            });
        }

        return apps;
    }

    public SetupResult InstallForApp(AIAppInfo app)
    {
        try
        {
            var configDir = Path.GetDirectoryName(app.ConfigPath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir!);
            }

            var config = new McpServerConfig
            {
                Command = _mcpExePath,
                Args = new List<string>()
            };

            Dictionary<string, object> fullConfig;

            if (File.Exists(app.ConfigPath))
            {
                var existingJson = File.ReadAllText(app.ConfigPath);
                fullConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(existingJson) 
                    ?? new Dictionary<string, object>();
            }
            else
            {
                fullConfig = new Dictionary<string, object>();
            }

            if (!fullConfig.ContainsKey("mcpServers"))
            {
                fullConfig["mcpServers"] = new Dictionary<string, object>();
            }

            var mcpServers = fullConfig["mcpServers"] as Dictionary<string, object> 
                ?? new Dictionary<string, object>();
            
            mcpServers["windows-toolbox"] = config;
            fullConfig["mcpServers"] = mcpServers;

            var json = JsonSerializer.Serialize(fullConfig, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(app.ConfigPath, json);

            return new SetupResult
            {
                Success = true,
                Message = $"Successfully configured {app.DisplayName}. Please restart the application.",
                ConfigPath = app.ConfigPath
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Failed to configure {app.DisplayName}: {ex.Message}",
                Error = ex
            };
        }
    }

    public SetupResult UninstallFromApp(AIAppInfo app)
    {
        try
        {
            if (!File.Exists(app.ConfigPath))
            {
                return new SetupResult
                {
                    Success = true,
                    Message = $"{app.DisplayName} configuration not found (already removed)."
                };
            }

            var existingJson = File.ReadAllText(app.ConfigPath);
            var fullConfig = JsonSerializer.Deserialize<Dictionary<string, object>>(existingJson);

            if (fullConfig != null && fullConfig.ContainsKey("mcpServers"))
            {
                var mcpServers = fullConfig["mcpServers"] as Dictionary<string, object>;
                if (mcpServers != null && mcpServers.ContainsKey("windows-toolbox"))
                {
                    mcpServers.Remove("windows-toolbox");
                    
                    var json = JsonSerializer.Serialize(fullConfig, new JsonSerializerOptions
                    {
                        WriteIndented = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    });

                    File.WriteAllText(app.ConfigPath, json);
                }
            }

            return new SetupResult
            {
                Success = true,
                Message = $"Successfully removed Windows Toolbox from {app.DisplayName}. Please restart the application."
            };
        }
        catch (Exception ex)
        {
            return new SetupResult
            {
                Success = false,
                Message = $"Failed to remove configuration from {app.DisplayName}: {ex.Message}",
                Error = ex
            };
        }
    }

    private string? GetCursorConfigPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var cursorDir = Path.Combine(appData, "Cursor", "User");
        return File.Exists(Path.Combine(cursorDir, "settings.json")) 
            ? Path.Combine(cursorDir, "mcp_settings.json")
            : null;
    }

    private string? GetClaudeDesktopConfigPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var claudeDir = Path.Combine(appData, "Claude");
        return Directory.Exists(claudeDir) || File.Exists(Path.Combine(claudeDir, "claude_desktop_config.json"))
            ? Path.Combine(claudeDir, "claude_desktop_config.json")
            : null;
    }

    private string? GetVSCodeConfigPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var vscodeDir = Path.Combine(appData, "Code", "User");
        return File.Exists(Path.Combine(vscodeDir, "settings.json"))
            ? Path.Combine(vscodeDir, "mcp_settings.json")
            : null;
    }
}

public class AIAppInfo
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string ConfigPath { get; set; } = "";
    public bool IsInstalled { get; set; }
    public string Icon { get; set; } = "";
}

public class SetupResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public string? ConfigPath { get; set; }
    public Exception? Error { get; set; }
}

public class McpServerConfig
{
    public string Command { get; set; } = "";
    public List<string> Args { get; set; } = new();
    public Dictionary<string, string>? Env { get; set; }
}
