using Microsoft.VisualBasic.FileIO;

namespace AKCleaner.Core;

public interface IAuditSink
{
    Task WriteAsync(string message, CancellationToken cancellationToken);
}

public sealed class FileAuditSink(string filePath) : IAuditSink
{
    public async Task WriteAsync(string message, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
        await File.AppendAllTextAsync(filePath, $"{DateTimeOffset.UtcNow:u} {message}{Environment.NewLine}", cancellationToken);
    }
}

public sealed class CleanupEngine(IAuditSink auditSink)
{
    private static readonly HashSet<string> ProtectedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        Environment.GetFolderPath(Environment.SpecialFolder.System)
    };

    public IReadOnlyList<CleanupTarget> DefaultTargets() =>
    [
        new("user-temp", "User Temp", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Temp")),
        new("windows-temp", "Windows Temp", @"C:\Windows\Temp", true),
        new("recent", "Recent Items", Environment.GetFolderPath(Environment.SpecialFolder.Recent))
    ];

    public async Task<IReadOnlyList<CleanupScanResult>> ScanAsync(IEnumerable<CleanupTarget> targets, CleanupOptions options, CancellationToken ct)
    {
        var results = new List<CleanupScanResult>();
        foreach (var target in targets)
        {
            ct.ThrowIfCancellationRequested();
            var items = new List<CleanupItem>();
            if (!Directory.Exists(target.Path))
            {
                results.Add(new CleanupScanResult(target, items, 0));
                continue;
            }

            foreach (var file in Directory.EnumerateFiles(target.Path, "*", System.IO.SearchOption.AllDirectories))
            {
                if (!ShouldInclude(file, options))
                {
                    continue;
                }

                try
                {
                    var info = new FileInfo(file);
                    items.Add(new CleanupItem(file, false, info.Exists ? info.Length : 0));
                }
                catch (Exception ex)
                {
                    items.Add(new CleanupItem(file, false, 0, Classify(ex), ex.Message));
                }
            }

            long total = items.Where(x => x.FailureReason == CleanupFailureReason.None).Sum(x => x.SizeBytes);
            results.Add(new CleanupScanResult(target, items, total));
            await auditSink.WriteAsync($"scan target={target.Id} count={items.Count} bytes={total}", ct);
        }

        return results;
    }

    public async Task<IReadOnlyList<CleanupExecutionResult>> CleanAsync(IEnumerable<CleanupScanResult> scanResults, CleanupOptions options, CancellationToken ct)
    {
        var results = new List<CleanupExecutionResult>();
        foreach (var scan in scanResults)
        {
            ct.ThrowIfCancellationRequested();
            int deleted = 0;
            long freed = 0;
            var failed = new List<CleanupItem>();

            foreach (var item in scan.Candidates.Where(x => x.FailureReason == CleanupFailureReason.None))
            {
                if (options.DryRun)
                {
                    deleted++;
                    freed += item.SizeBytes;
                    continue;
                }

                try
                {
                    if (options.UseRecycleBin)
                    {
                        FileSystem.DeleteFile(item.Path, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                    }
                    else
                    {
                        File.Delete(item.Path);
                    }

                    deleted++;
                    freed += item.SizeBytes;
                }
                catch (Exception ex)
                {
                    failed.Add(item with { FailureReason = Classify(ex), FailureMessage = ex.Message });
                }
            }

            var result = new CleanupExecutionResult(scan.Target, deleted, freed, failed);
            results.Add(result);
            await auditSink.WriteAsync($"clean target={scan.Target.Id} deleted={deleted} bytes={freed} failed={failed.Count}", ct);
            await auditSink.WriteAsync($"info target={scan.Target.Id} message=clean-complete deleted={deleted}", ct);
        }

        return results;
    }

    private static bool ShouldInclude(string path, CleanupOptions options)
    {
        if (ProtectedPaths.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (options.IncludePaths.Count > 0 && !options.IncludePaths.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        if (options.ExcludePaths.Any(x => path.StartsWith(x, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }

    private static CleanupFailureReason Classify(Exception ex) => ex switch
    {
        UnauthorizedAccessException => CleanupFailureReason.AccessDenied,
        FileNotFoundException or DirectoryNotFoundException => CleanupFailureReason.NotFound,
        IOException => CleanupFailureReason.FileLocked,
        _ => CleanupFailureReason.Unknown
    };
}
