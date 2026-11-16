using System.Diagnostics;
using System.Text.RegularExpressions;
using WindowsToolbox.Core.Interfaces;

namespace WindowsToolbox.Tools.Video;

public class DownloadVideoTool : BaseTool
{
    public override string Id => "download-video";
    public override string Name => "Download Video";
    public override string Description => "Download videos from YouTube, X/Twitter, TikTok, Instagram, Vimeo, etc.";
    public override string Category => "video";
    public override string Icon => "Download";

    private static readonly string YtDlpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "yt-dlp", "yt-dlp.exe");

    public override async Task<ToolResult> ExecuteAsync(ToolParameters parameters, IProgress<ToolProgress>? progress = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var url = parameters.Get<string>("url");
            var quality = parameters.Get<string>("quality") ?? "best"; // best, 1080p, 720p, 480p, 360p
            var audioOnly = parameters.Get<bool?>("audioOnly") ?? false;
            var outputDir = parameters.Get<string>("outputDir") ?? 
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            if (string.IsNullOrEmpty(url))
                return Failure("URL is required");

            if (!await EnsureYtDlp())
                return Failure("yt-dlp not found. Please install yt-dlp and place it in the application directory.");

            Directory.CreateDirectory(outputDir);

            ReportProgress(progress, 0, "Preparing download...");

            var formatSelection = audioOnly ? "bestaudio/best" : quality switch
            {
                "1080p" => "bestvideo[height<=1080]+bestaudio/best[height<=1080]",
                "720p" => "bestvideo[height<=720]+bestaudio/best[height<=720]",
                "480p" => "bestvideo[height<=480]+bestaudio/best[height<=480]",
                "360p" => "bestvideo[height<=360]+bestaudio/best[height<=360]",
                _ => "bestvideo+bestaudio/best"
            };

            var outputTemplate = Path.Combine(outputDir, "%(title)s.%(ext)s");
            
            var arguments = $"-f \"{formatSelection}\" ";
            if (audioOnly)
                arguments += "-x --audio-format mp3 ";
            arguments += $"--newline --no-playlist -o \"{outputTemplate}\" \"{url}\"";

            ReportProgress(progress, 10, "Downloading...");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = YtDlpPath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            var outputFile = "";
            var errorOutput = "";

            process.OutputDataReceived += (sender, e) =>
            {
                if (string.IsNullOrEmpty(e.Data)) return;

                var downloadMatch = Regex.Match(e.Data, @"(\d+\.?\d*)%");
                if (downloadMatch.Success && double.TryParse(downloadMatch.Groups[1].Value, out var percentage))
                {
                    ReportProgress(progress, 10 + (int)(percentage * 0.8), $"Downloading... {percentage:F1}%");
                }

                var destinationMatch = Regex.Match(e.Data, @"\[download\] Destination: (.+)");
                if (destinationMatch.Success)
                {
                    outputFile = destinationMatch.Groups[1].Value.Trim();
                }

                var mergeMatch = Regex.Match(e.Data, @"\[Merger\] Merging formats into ""(.+)""");
                if (mergeMatch.Success)
                {
                    outputFile = mergeMatch.Groups[1].Value.Trim();
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorOutput += e.Data + "\n";
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode == 0)
            {
                ReportProgress(progress, 100, "Download complete!");
                
                if (string.IsNullOrEmpty(outputFile))
                {
                    var files = Directory.GetFiles(outputDir, "*.*", SearchOption.TopDirectoryOnly)
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .FirstOrDefault();
                    outputFile = files ?? outputDir;
                }

                var fileSize = File.Exists(outputFile) ? new FileInfo(outputFile).Length / 1024.0 / 1024.0 : 0;
                return Success($"Video downloaded successfully ({fileSize:F2}MB)",
                    new Dictionary<string, object> { ["outputFile"] = outputFile });
            }

            return Failure($"Download failed: {errorOutput}");
        }
        catch (Exception ex)
        {
            return Failure($"Error downloading video: {ex.Message}", ex);
        }
    }

    private static async Task<bool> EnsureYtDlp()
    {
        if (File.Exists(YtDlpPath))
            return true;

        var directory = Path.GetDirectoryName(YtDlpPath);
        if (directory != null && !Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        return false;
    }
}
