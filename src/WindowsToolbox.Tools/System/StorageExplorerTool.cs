using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class StorageExplorerTool : BaseTool
{
    public override string Id => "storage-explorer";
    public override string Name => "Storage Explorer";
    public override string Description => "Analyze disk usage and find large files";
    public override string Category => "system";
    public override string Icon => "HardDrive";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(Failure("Not yet implemented"));
    }
}
