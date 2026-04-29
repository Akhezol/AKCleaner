namespace AKCleaner.Core;

public enum CleanupFailureReason
{
    None,
    AccessDenied,
    FileLocked,
    NotFound,
    Unknown
}

public sealed record CleanupTarget(
    string Id,
    string DisplayName,
    string Path,
    bool RequiresAdmin = false);

public sealed record CleanupItem(
    string Path,
    bool IsDirectory,
    long SizeBytes,
    CleanupFailureReason FailureReason = CleanupFailureReason.None,
    string? FailureMessage = null);

public sealed record CleanupScanResult(
    CleanupTarget Target,
    IReadOnlyList<CleanupItem> Candidates,
    long TotalBytes);

public sealed record CleanupExecutionResult(
    CleanupTarget Target,
    int DeletedCount,
    long FreedBytes,
    IReadOnlyList<CleanupItem> FailedItems);

public sealed class CleanupOptions
{
    public bool DryRun { get; init; } = true;
    public bool UseRecycleBin { get; init; } = true;
    public HashSet<string> IncludePaths { get; init; } = new(StringComparer.OrdinalIgnoreCase);
    public HashSet<string> ExcludePaths { get; init; } = new(StringComparer.OrdinalIgnoreCase);
}
