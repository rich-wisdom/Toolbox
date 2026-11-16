using System.Runtime.InteropServices;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class KeepAwakeTool : BaseTool
{
    public override string Id => "keep-awake";
    public override string Name => "Keep Awake";
    public override string Description => "Prevent system sleep and screen off";
    public override string Category => "system";
    public override string Icon => "Lightbulb";

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

    [Flags]
    private enum EXECUTION_STATE : uint
    {
        ES_CONTINUOUS = 0x80000000,
        ES_SYSTEM_REQUIRED = 0x00000001,
        ES_DISPLAY_REQUIRED = 0x00000002,
        ES_AWAYMODE_REQUIRED = 0x00000040
    }

    private static bool _isActive = false;
    private static CancellationTokenSource? _cancellationTokenSource;

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var enable = parameters.Get<bool>("enable");
            var durationMinutes = parameters.Get<int>("durationMinutes");
            var preventDisplaySleep = parameters.Get<bool>("preventDisplaySleep");
            
            if (preventDisplaySleep && enable)
                preventDisplaySleep = true;

            if (enable)
            {
                ReportProgress(progress, 50, "Enabling keep awake...");

                var flags = EXECUTION_STATE.ES_CONTINUOUS | EXECUTION_STATE.ES_SYSTEM_REQUIRED;
                if (preventDisplaySleep)
                {
                    flags |= EXECUTION_STATE.ES_DISPLAY_REQUIRED;
                }

                SetThreadExecutionState(flags);
                _isActive = true;

                if (durationMinutes > 0)
                {
                    _cancellationTokenSource?.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(TimeSpan.FromMinutes(durationMinutes), _cancellationTokenSource.Token);
                            SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                            _isActive = false;
                        }
                        catch (TaskCanceledException) { }
                    });

                    return Success($"Keep awake enabled for {durationMinutes} minutes");
                }

                return Success("Keep awake enabled indefinitely");
            }
            else
            {
                ReportProgress(progress, 50, "Disabling keep awake...");

                _cancellationTokenSource?.Cancel();
                SetThreadExecutionState(EXECUTION_STATE.ES_CONTINUOUS);
                _isActive = false;

                return Success("Keep awake disabled");
            }
        }
        catch (Exception ex)
        {
            return Failure($"Error controlling keep awake: {ex.Message}", ex);
        }
    }
}
