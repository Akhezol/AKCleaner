using AKCleaner.Core;
using Microsoft.Extensions.Logging.Abstractions;

namespace AKCleaner.Core.Tests;

public class CleanupEngineTests
{
    [Fact]
    public async Task ScanAsync_ReturnsResults_ForExistingTarget()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"akcleaner-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);
        await File.WriteAllTextAsync(Path.Combine(tempDir, "foo.tmp"), "hello");

        var sink = new InMemoryAuditSink();
        var engine = new CleanupEngine(new NullLogger<CleanupEngine>(), sink);
        var targets = new[] { new CleanupTarget("test", "Test", tempDir) };

        var results = await engine.ScanAsync(targets, new CleanupOptions(), CancellationToken.None);

        Assert.Single(results);
        Assert.NotEmpty(results[0].Candidates);
        Directory.Delete(tempDir, true);
    }

    private sealed class InMemoryAuditSink : IAuditSink
    {
        public List<string> Messages { get; } = [];
        public Task WriteAsync(string message, CancellationToken cancellationToken)
        {
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
