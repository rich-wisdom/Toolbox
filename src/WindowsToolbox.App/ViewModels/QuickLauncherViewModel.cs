using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Core.Services;

namespace WindowsToolbox.App.ViewModels;

public partial class QuickLauncherViewModel : ObservableObject
{
    private readonly IToolRegistry _toolRegistry;
    private readonly IRecentToolsService _recentToolsService;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    public ObservableCollection<ITool> FilteredTools { get; } = new();

    public QuickLauncherViewModel(IToolRegistry toolRegistry, IRecentToolsService recentToolsService)
    {
        _toolRegistry = toolRegistry;
        _recentToolsService = recentToolsService;
        RefreshRecentTools();
    }

    partial void OnSearchQueryChanged(string value)
    {
        UpdateFilteredTools();
    }

    public void RefreshRecentTools()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            UpdateFilteredTools();
        }
    }

    private void UpdateFilteredTools()
    {
        FilteredTools.Clear();

        IEnumerable<ITool> tools;
        if (string.IsNullOrWhiteSpace(SearchQuery))
        {
            var recentToolIds = _recentToolsService.GetRecentToolIds(10);
            tools = recentToolIds
                .Select(id => _toolRegistry.GetToolById(id))
                .Where(t => t != null)
                .Cast<ITool>();
            
            if (!tools.Any())
            {
                tools = _toolRegistry.GetAllTools().Take(10);
            }
        }
        else
        {
            tools = _toolRegistry.SearchTools(SearchQuery).Take(20);
        }

        foreach (var tool in tools)
        {
            FilteredTools.Add(tool);
        }
    }

    public void LaunchTool(ITool tool)
    {
        _recentToolsService.AddRecentTool(tool.Id);
    }
}
