using System.Diagnostics;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.System;

public class DiskSpeedTestTool : BaseTool
{
    public override string Id => "disk-speed-test";
    public override string Name => "Disk Speed Test";
    public override string Description => "Test storage read/write speeds";
    public override string Category => "system";
    public override string Icon => "Performance";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var drivePath = parameters.Get<string>("drivePath") ?? Path.GetTempPath();
            var testSizeMB = parameters.Get<int>("testSizeMB");
            if (testSizeMB == 0) testSizeMB = 100;

            var testFile = Path.Combine(drivePath, $"speedtest_{Guid.NewGuid()}.tmp");

            ReportProgress(progress, 10, "Testing write speed...");
            var writeSpeed = await TestWriteSpeed(testFile, testSizeMB, cancellationToken);

            ReportProgress(progress, 50, "Testing read speed...");
            var readSpeed = await TestReadSpeed(testFile, cancellationToken);

            ReportProgress(progress, 90, "Cleaning up...");
            try { File.Delete(testFile); } catch { }

            var results = new Dictionary<string, object>
            {
                ["writeSpeedMBps"] = Math.Round(writeSpeed, 2),
                ["readSpeedMBps"] = Math.Round(readSpeed, 2),
                ["testSizeMB"] = testSizeMB,
                ["drivePath"] = drivePath
            };

            return Success($"Write: {writeSpeed:F2} MB/s, Read: {readSpeed:F2} MB/s", results);
        }
        catch (Exception ex)
        {
            return Failure($"Error during disk speed test: {ex.Message}", ex);
        }
    }

    private async Task<double> TestWriteSpeed(string filePath, int sizeMB, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 1024];
        new Random().NextBytes(buffer);

        var sw = Stopwatch.StartNew();
        
        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, buffer.Length, FileOptions.WriteThrough))
        {
            for (int i = 0; i < sizeMB && !cancellationToken.IsCancellationRequested; i++)
            {
                await fs.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
            }
            await fs.FlushAsync(cancellationToken);
        }
        
        sw.Stop();

        var speedMBps = sizeMB / sw.Elapsed.TotalSeconds;
        return speedMBps;
    }

    private async Task<double> TestReadSpeed(string filePath, CancellationToken cancellationToken)
    {
        var buffer = new byte[1024 * 1024];
        long totalBytesRead = 0;

        var sw = Stopwatch.StartNew();
        
        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, buffer.Length))
        {
            int bytesRead;
            while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
            {
                totalBytesRead += bytesRead;
            }
        }
        
        sw.Stop();

        var sizeMB = totalBytesRead / (1024.0 * 1024.0);
        var speedMBps = sizeMB / sw.Elapsed.TotalSeconds;
        return speedMBps;
    }
}
