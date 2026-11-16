using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools;

public abstract class BaseTool : ITool
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract string Category { get; }
    public virtual string Icon { get; } = "Tool";
    public virtual bool IsAvailable => true;
    public virtual bool IsPremium => false;

    public abstract Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default);

    public virtual bool CanExecute(ToolParameters parameters) => true;

    protected void ReportProgress(IProgress<ToolProgress>? progress, int percentage, string status, string? currentFile = null)
    {
        progress?.Report(new ToolProgress(percentage, status, currentFile));
    }

    protected ToolResult Success(string message, Dictionary<string, object>? data = null)
        => ToolResult.Succeeded(message, data);

    protected ToolResult Failure(string message, Exception? error = null)
        => ToolResult.Failed(message, error);
}
