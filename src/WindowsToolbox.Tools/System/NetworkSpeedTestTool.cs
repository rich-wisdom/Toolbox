using System.Diagnostics;
using System.Net.NetworkInformation;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.SystemUtilities;

public class NetworkSpeedTestTool : BaseTool
{
    public override string Id => "network-speed-test";
    public override string Name => "Network Speed Test";
    public override string Description => "Test internet speed and latency";
    public override string Category => "system";
    public override string Icon => "SpeedTest";

    private const string TestUrl = "http://speedtest.ftp.otenet.gr/files/test100k.db";
    private const string PingHost = "8.8.8.8";

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var results = new Dictionary<string, object>();

            ReportProgress(progress, 10, "Testing latency...");
            var latency = await TestLatency(cancellationToken);
            results["latency"] = latency;

            ReportProgress(progress, 40, "Testing download speed...");
            var downloadSpeed = await TestDownloadSpeed(cancellationToken);
            results["downloadSpeedMbps"] = downloadSpeed;

            ReportProgress(progress, 100, "Test complete");

            return Success($"Speed test complete - Download: {downloadSpeed:F2} Mbps, Latency: {latency} ms", results);
        }
        catch (Exception ex)
        {
            return Failure($"Error during speed test: {ex.Message}", ex);
        }
    }

    private async Task<long> TestLatency(CancellationToken cancellationToken)
    {
        try
        {
            using var ping = new Ping();
            var reply = await ping.SendPingAsync(PingHost, 5000);
            return reply.Status == IPStatus.Success ? reply.RoundtripTime : -1;
        }
        catch
        {
            return -1;
        }
    }

    private async Task<double> TestDownloadSpeed(CancellationToken cancellationToken)
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            var sw = Stopwatch.StartNew();
            var response = await client.GetAsync(TestUrl, cancellationToken);
            var data = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            sw.Stop();

            var sizeInBits = data.Length * 8.0;
            var speedBps = sizeInBits / sw.Elapsed.TotalSeconds;
            var speedMbps = speedBps / 1_000_000.0;

            return Math.Round(speedMbps, 2);
        }
        catch
        {
            return 0;
        }
    }
}
