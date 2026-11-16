using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.UI.Xaml;
using WindowsToolbox.App.Services;
using WindowsToolbox.App.ViewModels;
using WindowsToolbox.App.Views;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Core.Services;
using WindowsToolbox.Tools.Developer;
using WindowsToolbox.Tools.File;
using WindowsToolbox.Tools.System;

namespace WindowsToolbox.App;

public partial class App : Application
{
    private IHost? _host;
    private Window? _mainWindow;

    public static IServiceProvider Services => ((App)Current)._host!.Services;

    public App()
    {
        InitializeComponent();
        _host = CreateHostBuilder().Build();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        _mainWindow = Services.GetRequiredService<MainWindow>();
        _mainWindow.Activate();

        var hotKeyService = Services.GetRequiredService<IGlobalHotKeyService>();
        hotKeyService.RegisterHotKey(ModifierKeys.Control | ModifierKeys.Alt, VirtualKey.Space, OnQuickLauncherInvoked);
    }

    private void OnQuickLauncherInvoked()
    {
        var quickLauncher = Services.GetRequiredService<QuickLauncherWindow>();
        quickLauncher.ShowAndActivate();
    }

    private static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddSingleton<IToolRegistry>(sp =>
                {
                    var tools = new ITool[]
                    {
                        new JsonFormatterTool(),
                        new QRCodeTool(),
                        new StorageExplorerTool(),
                        new BatteryHealthTool(),
                        new NetworkSpeedTestTool(),
                        new WiFiPasswordTool(),
                        new KeepAwakeTool(),
                        new UninstallAppTool(),
                        new ClipboardHistoryTool(),
                        new DiskSpeedTestTool(),
                        new PresentationModeTool(),
                        new CleanKeyboardTool(),
                        new DisplayControlTool(),
                        new MouseFixesTool(),
                        new DeadPixelTestTool(),
                        new EjectAllDisksTool(),
                        new LoginItemsTool(),
                        new FixAppDamagedTool(),
                        new ResetPermissionsTool(),
                        new HiddenSettingsTool(),
                    };
                    return new ToolRegistry(tools);
                });

                services.AddSingleton<IRecentToolsService, RecentToolsService>();
                
                services.AddSingleton<ISettingsService>(sp =>
                {
                    var appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "WindowsToolbox",
                        "settings.json");
                    return new SettingsService(appDataPath);
                });

                services.AddSingleton<IGlobalHotKeyService, GlobalHotKeyService>();

                services.AddTransient<MainWindow>();
                services.AddTransient<QuickLauncherWindow>();
                
                services.AddTransient<MainViewModel>();
                services.AddTransient<QuickLauncherViewModel>();
            });
    }
}

[Flags]
public enum ModifierKeys
{
    None = 0,
    Alt = 1,
    Control = 2,
    Shift = 4,
    Windows = 8
}

public enum VirtualKey
{
    Space = 0x20
}
