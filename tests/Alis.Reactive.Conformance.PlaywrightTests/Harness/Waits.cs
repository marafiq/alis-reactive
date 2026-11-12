using System.Globalization;
using System.IO.Compression;
using System.Text.Json;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

internal static class Waits
{
    private const string StepsOfAMeasuredRun = "ALIS_MEASURED_STEPS";
    private const string Measured = "measured.json";
    private const string ActionWait = "actionWaitMilliseconds";
    private const string NavigationWait = "navigationWaitMilliseconds";
    private const string CallsOfTheTrace = "trace.trace";

    private static readonly string StepsFile = Environment.GetEnvironmentVariable(StepsOfAMeasuredRun) ?? "";
    private static readonly Lazy<(float Action, float Navigation)> Held = new(ReadTheMeasured);
    private static readonly Lock WritingSteps = new();

    private static bool TheRunIsMeasured => StepsFile.Length > 0;

    internal static void SetOnEveryExpectation()
    {
        if (!TheRunIsMeasured) Assertions.SetDefaultExpectTimeout(Held.Value.Action);
    }

    internal static async Task Open(IBrowserContext context)
    {
        if (TheRunIsMeasured)
        {
            await context.Tracing.StartAsync();
            return;
        }

        context.SetDefaultTimeout(Held.Value.Action);
        context.SetDefaultNavigationTimeout(Held.Value.Navigation);
    }

    internal static async Task Close(IBrowserContext context)
    {
        if (TheRunIsMeasured) await WriteTheStepsOf(context);
        await context.CloseAsync();
    }

    private static async Task WriteTheStepsOf(IBrowserContext context)
    {
        var trace = Path.Combine(Path.GetTempPath(), $"alis-reactive-steps-{Guid.NewGuid():N}.zip");
        await context.Tracing.StopAsync(new TracingStopOptions { Path = trace });
        var steps = CallsIn(trace, TestContext.CurrentContext.Test.FullName);
        File.Delete(trace);
        lock (WritingSteps) File.AppendAllLines(StepsFile, steps);
    }

    private static List<string> CallsIn(string trace, string test)
    {
        using var archive = ZipFile.OpenRead(trace);
        var calls = archive.GetEntry(CallsOfTheTrace)
                    ?? throw new InvalidOperationException($"Playwright's trace at {trace} holds no {CallsOfTheTrace}, so the steps of {test} were not read.");
        using var lines = new StreamReader(calls.Open());
        var started = new Dictionary<string, (string Holder, string Method, double At)>(StringComparer.Ordinal);
        var steps = new List<string>();
        while (lines.ReadLine() is { } line)
        {
            using var read = JsonDocument.Parse(line);
            var entry = read.RootElement;
            var type = entry.GetProperty("type").GetString();
            if (type == "before")
            {
                started[Text(entry, "callId")] = (Text(entry, "class"), Text(entry, "method"), entry.GetProperty("startTime").GetDouble());
            }
            else if (type == "after" && started.Remove(Text(entry, "callId"), out var call))
            {
                var milliseconds = entry.GetProperty("endTime").GetDouble() - call.At;
                steps.Add(string.Join('\t', test, call.Holder, call.Method, milliseconds.ToString("0.###", CultureInfo.InvariantCulture)));
            }
        }

        return steps;
    }

    private static string Text(JsonElement entry, string name)
        => entry.GetProperty(name).GetString() ?? throw new InvalidOperationException($"A call of Playwright's trace holds no text in {name}: {entry.GetRawText()}");

    private static (float Action, float Navigation) ReadTheMeasured()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, nameof(Harness), Measured);
        using var measured = JsonDocument.Parse(File.ReadAllText(path));
        return (Wait(measured.RootElement, ActionWait, path), Wait(measured.RootElement, NavigationWait, path));
    }

    private static float Wait(JsonElement measured, string name, string path)
        => measured.TryGetProperty(name, out var wait)
            ? wait.GetSingle()
            : throw new InvalidOperationException($"{path} holds no {name}: a wait is measured, never chosen, and scripts/playwright.sh --measure writes it.");
}
