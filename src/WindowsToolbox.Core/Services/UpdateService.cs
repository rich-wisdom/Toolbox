using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WindowsToolbox.Core.Services;

/// <summary>
/// Implementation of the update service that checks GitHub Releases for updates.
/// </summary>
public class UpdateService : IUpdateService
{
    private const string GitHubApiUrl = "https://api.github.com/repos/rich-wisdom/Toolbox/releases/latest";
    private const string UserAgent = "WindowsToolbox";

    private readonly HttpClient _httpClient;
    private readonly ILogger<UpdateService> _logger;
    private bool _isCheckingForUpdates;
    private UpdateInfo? _availableUpdate;

    public UpdateService(ILogger<UpdateService> logger)
    {
        _logger = logger;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
    }

    public bool IsCheckingForUpdates => _isCheckingForUpdates;
    public bool IsUpdateAvailable => _availableUpdate != null;
    public UpdateInfo? AvailableUpdate => _availableUpdate;

    public event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;
    public event EventHandler<UpdateProgressEventArgs>? UpdateProgressChanged;

    public async Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        if (_isCheckingForUpdates)
        {
            return false;
        }

        try
        {
            _isCheckingForUpdates = true;
            _logger.LogInformation("Checking for updates...");

            var response = await _httpClient.GetAsync(GitHubApiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var release = await response.Content.ReadFromJsonAsync<GitHubRelease>(cancellationToken);
            if (release == null)
            {
                _logger.LogWarning("Failed to parse release information");
                return false;
            }

            // Parse version from tag (e.g., "v1.0.0" -> "1.0.0")
            var versionString = release.TagName?.TrimStart('v');
            if (!Version.TryParse(versionString, out var releaseVersion))
            {
                _logger.LogWarning("Failed to parse version from tag: {Tag}", release.TagName);
                return false;
            }

            var currentVersion = GetCurrentVersion();
            if (releaseVersion > currentVersion)
            {
                // Find the MSIX bundle asset
                var asset = release.Assets?
                    .FirstOrDefault(a => a.Name?.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase) == true
                                      || a.Name?.EndsWith(".msix", StringComparison.OrdinalIgnoreCase) == true);

                if (asset == null)
                {
                    _logger.LogWarning("No MSIX package found in release");
                    return false;
                }

                _availableUpdate = new UpdateInfo
                {
                    Version = releaseVersion,
                    ReleaseNotes = release.Body ?? "No release notes available",
                    DownloadUrl = asset.BrowserDownloadUrl ?? string.Empty,
                    PackageSize = asset.Size,
                    ReleaseDate = release.PublishedAt
                };

                _logger.LogInformation("Update available: v{Version}", releaseVersion);
                UpdateAvailable?.Invoke(this, new UpdateAvailableEventArgs(_availableUpdate));
                return true;
            }

            _logger.LogInformation("No updates available. Current version: v{Version}", currentVersion);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for updates");
            return false;
        }
        finally
        {
            _isCheckingForUpdates = false;
        }
    }

    public async Task<bool> DownloadAndInstallUpdateAsync(CancellationToken cancellationToken = default)
    {
        if (_availableUpdate == null)
        {
            _logger.LogWarning("No update available to install");
            return false;
        }

        try
        {
            _logger.LogInformation("Downloading update from {Url}", _availableUpdate.DownloadUrl);

            // Download to temp folder
            var tempPath = Path.Combine(Path.GetTempPath(), "WindowsToolbox-Update.msix");
            
            using (var response = await _httpClient.GetAsync(_availableUpdate.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
            {
                response.EnsureSuccessStatusCode();
                var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                var canReportProgress = totalBytes != -1;

                using var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

                var buffer = new byte[8192];
                long totalBytesRead = 0;
                int bytesRead;

                while ((bytesRead = await contentStream.ReadAsync(buffer, cancellationToken)) > 0)
                {
                    await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                    totalBytesRead += bytesRead;

                    if (canReportProgress)
                    {
                        var percentage = (double)totalBytesRead / totalBytes * 100;
                        UpdateProgressChanged?.Invoke(this, new UpdateProgressEventArgs(percentage, totalBytesRead, totalBytes));
                    }
                }
            }

            _logger.LogInformation("Update downloaded to {Path}", tempPath);

            // Install the MSIX package using PowerShell
            _logger.LogInformation("Installing update...");
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Command \"Add-AppxPackage -Path '{tempPath}'\"",
                UseShellExecute = true,
                Verb = "runas" // Request elevation
            };

            var process = Process.Start(psi);
            if (process != null)
            {
                await process.WaitForExitAsync(cancellationToken);
                
                if (process.ExitCode == 0)
                {
                    _logger.LogInformation("Update installed successfully");
                    
                    // Clean up
                    try
                    {
                        File.Delete(tempPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete temporary update file");
                    }

                    return true;
                }
                else
                {
                    _logger.LogError("Update installation failed with exit code {ExitCode}", process.ExitCode);
                }
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading and installing update");
            return false;
        }
    }

    public Version GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        return version ?? new Version(1, 0, 0, 0);
    }

    // GitHub API response models
    private class GitHubRelease
    {
        public string? TagName { get; set; }
        public string? Body { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public GitHubAsset[]? Assets { get; set; }
    }

    private class GitHubAsset
    {
        public string? Name { get; set; }
        public string? BrowserDownloadUrl { get; set; }
        public long Size { get; set; }
    }
}
