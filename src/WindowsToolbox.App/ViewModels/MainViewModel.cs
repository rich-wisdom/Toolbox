using CommunityToolkit.Mvvm.ComponentModel;
using WindowsToolbox.Core.Services;

namespace WindowsToolbox.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IToolRegistry _toolRegistry;
    private readonly IRecentToolsService _recentToolsService;

    public MainViewModel(IToolRegistry toolRegistry, IRecentToolsService recentToolsService)
    {
        _toolRegistry = toolRegistry;
        _recentToolsService = recentToolsService;
    }
}
