using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Core.Services;

public interface IRecentToolsService
{
    void AddRecentTool(string toolId);
    IReadOnlyList<string> GetRecentToolIds(int count = 10);
    void Clear();
}

public class RecentToolsService : IRecentToolsService
{
    private readonly List<string> _recentToolIds = new();
    private const int MaxRecentTools = 50;

    public void AddRecentTool(string toolId)
    {
        _recentToolIds.Remove(toolId);
        _recentToolIds.Insert(0, toolId);
        
        if (_recentToolIds.Count > MaxRecentTools)
        {
            _recentToolIds.RemoveAt(_recentToolIds.Count - 1);
        }
    }

    public IReadOnlyList<string> GetRecentToolIds(int count = 10)
        => _recentToolIds.Take(count).ToList();

    public void Clear()
        => _recentToolIds.Clear();
}
