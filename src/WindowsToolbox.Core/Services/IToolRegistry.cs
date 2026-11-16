using WindowsToolbox.Core.Interfaces;
using WindowsToolbox.Core.Models;

namespace WindowsToolbox.Core.Services;

public interface IToolRegistry
{
    IReadOnlyList<ITool> GetAllTools();
    IReadOnlyList<ITool> GetToolsByCategory(string categoryId);
    ITool? GetToolById(string toolId);
    IReadOnlyList<ToolCategory> GetCategories();
    IReadOnlyList<ITool> SearchTools(string query);
}

public class ToolRegistry : IToolRegistry
{
    private readonly Dictionary<string, ITool> _tools = new();
    private readonly List<ITool> _toolsList = new();

    public ToolRegistry(IEnumerable<ITool> tools)
    {
        foreach (var tool in tools)
        {
            _tools[tool.Id] = tool;
            _toolsList.Add(tool);
        }
        
        UpdateCategoryCounts();
    }

    public IReadOnlyList<ITool> GetAllTools() => _toolsList;

    public IReadOnlyList<ITool> GetToolsByCategory(string categoryId)
        => _toolsList.Where(t => t.Category == categoryId).ToList();

    public ITool? GetToolById(string toolId)
        => _tools.TryGetValue(toolId, out var tool) ? tool : null;

    public IReadOnlyList<ToolCategory> GetCategories() => ToolCategory.All;

    public IReadOnlyList<ITool> SearchTools(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return _toolsList;

        var searchTerms = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        return _toolsList
            .Select(tool => new
            {
                Tool = tool,
                Score = CalculateSearchScore(tool, searchTerms)
            })
            .Where(x => x.Score > 0)
            .OrderByDescending(x => x.Score)
            .Select(x => x.Tool)
            .ToList();
    }

    private int CalculateSearchScore(ITool tool, string[] searchTerms)
    {
        var score = 0;
        var toolName = tool.Name.ToLowerInvariant();
        var toolDesc = tool.Description.ToLowerInvariant();

        foreach (var term in searchTerms)
        {
            if (toolName.Contains(term))
                score += 10;
            if (toolDesc.Contains(term))
                score += 5;
            if (tool.Category.ToLowerInvariant().Contains(term))
                score += 3;
        }

        return score;
    }

    private void UpdateCategoryCounts()
    {
        foreach (var category in ToolCategory.All)
        {
            category.ToolCount = GetToolsByCategory(category.Id).Count;
        }
    }
}
