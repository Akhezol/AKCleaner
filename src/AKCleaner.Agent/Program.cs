using System.Text.Json;
using AKCleaner.Core;
var auditSink = new FileAuditSink(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AKCleaner", "logs", "activity.log"));
var engine = new CleanupEngine(auditSink);
IReadOnlyList<CleanupScanResult> lastScan = [];
IReadOnlyList<CleanupExecutionResult> history = [];

var stdin = Console.In;
var stdout = Console.Out;

while (true)
{
    var input = await stdin.ReadLineAsync();
    if (string.IsNullOrWhiteSpace(input))
    {
        continue;
    }

    var request = JsonSerializer.Deserialize<AgentRequest>(input);
    if (request is null)
    {
        await WriteAsync(stdout, new AgentResponse("error", new { message = "invalid-json" }));
        continue;
    }

    if (request.Action == "shutdown")
    {
        await WriteAsync(stdout, new AgentResponse("ok", new { message = "bye" }));
        break;
    }

    try
    {
        switch (request.Action)
        {
            case "startScan":
                var targets = engine.DefaultTargets();
                lastScan = await engine.ScanAsync(targets, request.Options ?? new CleanupOptions(), CancellationToken.None);
                await WriteAsync(stdout, new AgentResponse("ok", lastScan));
                break;
            case "getScanProgress":
                await WriteAsync(stdout, new AgentResponse("ok", new { percent = 100, scannedTargets = lastScan.Count }));
                break;
            case "applyCleanup":
                var payload = request.ScanResults ?? [];
                history = await engine.CleanAsync(payload, request.Options ?? new CleanupOptions { DryRun = false }, CancellationToken.None);
                await WriteAsync(stdout, new AgentResponse("ok", history));
                break;
            case "getHistory":
                await WriteAsync(stdout, new AgentResponse("ok", history));
                break;
            default:
                await WriteAsync(stdout, new AgentResponse("error", new { message = $"unsupported-action:{request.Action}" }));
                break;
        }
    }
    catch (Exception ex)
    {
        await WriteAsync(stdout, new AgentResponse("error", new { message = ex.Message }));
    }
}

static Task WriteAsync(TextWriter writer, AgentResponse response)
{
    var json = JsonSerializer.Serialize(response);
    return writer.WriteLineAsync(json);
}

internal sealed record AgentRequest(string Action, CleanupOptions? Options, IReadOnlyList<CleanupScanResult>? ScanResults);
internal sealed record AgentResponse(string Status, object Data);
