using System;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsToolbox.Core.Services;

/// <summary>
/// Service for checking and applying application updates.
/// </summary>
public interface IUpdateService
{
    /// <summary>
    /// Gets a value indicating whether an update check is currently in progress.
    /// </summary>
    bool IsCheckingForUpdates { get; }

    /// <summary>
    /// Gets a value indicating whether an update is available.
    /// </summary>
    bool IsUpdateAvailable { get; }

    /// <summary>
    /// Gets the available update information, if any.
    /// </summary>
    UpdateInfo? AvailableUpdate { get; }

    /// <summary>
    /// Event raised when update availability changes.
    /// </summary>
    event EventHandler<UpdateAvailableEventArgs>? UpdateAvailable;

    /// <summary>
    /// Event raised when update download progress changes.
    /// </summary>
    event EventHandler<UpdateProgressEventArgs>? UpdateProgressChanged;

    /// <summary>
    /// Checks for available updates from GitHub Releases.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if an update is available, false otherwise.</returns>
    Task<bool> CheckForUpdatesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads and installs the available update.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if the update was successfully installed, false otherwise.</returns>
    Task<bool> DownloadAndInstallUpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current application version.
    /// </summary>
    /// <returns>The current version.</returns>
    Version GetCurrentVersion();
}

/// <summary>
/// Information about an available update.
/// </summary>
public record UpdateInfo
{
    /// <summary>
    /// Gets the version of the update.
    /// </summary>
    public required Version Version { get; init; }

    /// <summary>
    /// Gets the release notes for the update.
    /// </summary>
    public required string ReleaseNotes { get; init; }

    /// <summary>
    /// Gets the download URL for the update package.
    /// </summary>
    public required string DownloadUrl { get; init; }

    /// <summary>
    /// Gets the size of the update package in bytes.
    /// </summary>
    public long? PackageSize { get; init; }

    /// <summary>
    /// Gets the release date.
    /// </summary>
    public DateTimeOffset? ReleaseDate { get; init; }
}

/// <summary>
/// Event args for update availability.
/// </summary>
public class UpdateAvailableEventArgs : EventArgs
{
    public UpdateAvailableEventArgs(UpdateInfo updateInfo)
    {
        UpdateInfo = updateInfo;
    }

    public UpdateInfo UpdateInfo { get; }
}

/// <summary>
/// Event args for update progress.
/// </summary>
public class UpdateProgressEventArgs : EventArgs
{
    public UpdateProgressEventArgs(double percentage, long bytesDownloaded, long totalBytes)
    {
        Percentage = percentage;
        BytesDownloaded = bytesDownloaded;
        TotalBytes = totalBytes;
    }

    public double Percentage { get; }
    public long BytesDownloaded { get; }
    public long TotalBytes { get; }
}
