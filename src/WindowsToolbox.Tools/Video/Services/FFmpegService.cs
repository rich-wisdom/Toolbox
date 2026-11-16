using FFMpegCore;
using FFMpegCore.Enums;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace WindowsToolbox.Tools.Video.Services;

public class FFmpegService
{
    private static bool _isInitialized = false;
    private static readonly object _lock = new();

    public static void EnsureFFmpegPath()
    {
        lock (_lock)
        {
            if (_isInitialized) return;

            // FFMpegCore will auto-detect FFmpeg in PATH or common locations
            // For production, bundle FFmpeg binaries with the app
            var ffmpegPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffmpeg.exe");
            var ffprobePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg", "ffprobe.exe");

            if (File.Exists(ffmpegPath) && File.Exists(ffprobePath))
            {
                GlobalFFOptions.Configure(options => options.BinaryFolder = Path.GetDirectoryName(ffmpegPath));
            }

            _isInitialized = true;
        }
    }

    public static async Task<bool> IsFFmpegAvailable()
    {
        try
        {
            EnsureFFmpegPath();
            var result = await FFMpegArguments.FromInputFiles(true).ProcessAsynchronously(false);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static async Task<IMediaAnalysis?> AnalyzeMedia(string filePath)
    {
        try
        {
            EnsureFFmpegPath();
            return await FFProbe.AnalyseAsync(filePath);
        }
        catch
        {
            return null;
        }
    }

    public static FFMpegArgumentProcessor CreateProcessor()
    {
        EnsureFFmpegPath();
        return FFMpegArguments.FromInputFiles(true);
    }

    public static void ParseProgress(string output, IProgress<int>? progress, TimeSpan? duration)
    {
        if (progress == null || !duration.HasValue) return;

        var timeMatch = Regex.Match(output, @"time=(\d{2}):(\d{2}):(\d{2}\.\d{2})");
        if (timeMatch.Success)
        {
            var hours = int.Parse(timeMatch.Groups[1].Value);
            var minutes = int.Parse(timeMatch.Groups[2].Value);
            var seconds = double.Parse(timeMatch.Groups[3].Value);
            var currentTime = TimeSpan.FromHours(hours) + TimeSpan.FromMinutes(minutes) + TimeSpan.FromSeconds(seconds);
            
            var percentage = (int)((currentTime.TotalSeconds / duration.Value.TotalSeconds) * 100);
            progress.Report(Math.Min(percentage, 100));
        }
    }

    public static HardwareAcceleration DetectHardwareAcceleration()
    {
        // Detect available hardware acceleration
        // In a real implementation, you would check system capabilities
        // For now, return Auto which lets FFmpeg decide
        return HardwareAcceleration.auto;
    }
}

public enum HardwareAcceleration
{
    none,
    auto,
    nvenc,    // NVIDIA
    qsv,      // Intel QuickSync
    amf,      // AMD
    videotoolbox  // Apple (not applicable for Windows)
}
