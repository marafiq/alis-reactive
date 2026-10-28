using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;

namespace Alis.Reactive.Playwright.TestHost;

/// <summary>
/// One browser page per test against the app the test assembly runs (<see cref="AppServer"/>): the page is
/// opened the way a user opens it, and when a test fails its trace, a screenshot and the page's console are
/// kept beside the test output. The console is evidence for the reader of a failure, never something a test
/// asserts on.
/// </summary>
public abstract class AppPageTest : PageTest
{
    private const int DefaultTimeoutMs = 60000;
    private const string BootedMarker = "html[data-alis-booted='true']";
    private readonly List<string> _console = new();
    private readonly object _consoleLock = new();
    private Stopwatch? _stopwatch;

    /// <summary>The address of the app the test assembly runs.</summary>
    protected string BaseUrl => AppServer.BaseUrl;

    /// <summary>Opens a page of the app and waits until the runtime has booted it, as a user first meets it.</summary>
    /// <param name="path">The page's path, for example <c>/Members/Properties</c>.</param>
    protected async Task Open(string path)
    {
        await Page.GotoAsync(BaseUrl + path, new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        await Page.Locator(BootedMarker).WaitForAsync(new() { State = WaitForSelectorState.Attached });
    }

    [SetUp]
    public async Task StartPage()
    {
        _stopwatch = Stopwatch.StartNew();
        WriteProgress("start");

        lock (_consoleLock) _console.Clear();
        Page.SetDefaultTimeout(DefaultTimeoutMs);
        Page.SetDefaultNavigationTimeout(DefaultTimeoutMs);
        Page.Console += (_, message) => Keep($"[{message.Type}] {message.Text}");
        Page.PageError += (_, error) => Keep($"[PAGE ERROR] {error}");

        await Context.Tracing.StartAsync(new()
        {
            Title = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.Name}",
            Screenshots = true,
            Snapshots = true,
            Sources = false,
        });
    }

    [TearDown]
    public async Task KeepFailureEvidence()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed;
        var traceDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "TestResults", "playwright-traces");
        var testName = TestContext.CurrentContext.Test.Name;
        var tracePath = failed ? Path.GetFullPath(Path.Combine(traceDir, $"{testName}.zip")) : null;
        var screenshotPath = failed ? Path.GetFullPath(Path.Combine(traceDir, $"{testName}.png")) : null;
        if (failed)
            Directory.CreateDirectory(traceDir);

        await Context.Tracing.StopAsync(new() { Path = tracePath });

        if (failed)
        {
            await Page.ScreenshotAsync(new() { Path = screenshotPath!, FullPage = true });
            TestContext.AddTestAttachment(tracePath!, "Playwright trace");
            TestContext.AddTestAttachment(screenshotPath!, "Screenshot on failure");

            // scripts/trx-failed-tests.mjs reads the failure's console between these two lines.
            var console = ConsoleSnapshot();
            if (console.Count > 0)
            {
                TestContext.Out.WriteLine("=== Page Console Output ===");
                foreach (var line in console)
                    TestContext.Out.WriteLine(line);
                TestContext.Out.WriteLine("=== End Console Output ===");
            }
        }

        WriteProgress("end", tracePath, screenshotPath);
    }

    private void Keep(string line)
    {
        lock (_consoleLock) _console.Add(line);
    }

    private List<string> ConsoleSnapshot()
    {
        lock (_consoleLock) return _console.ToList();
    }

    private void WriteProgress(string phase, string? tracePath = null, string? screenshotPath = null)
    {
        var test = TestContext.CurrentContext.Test;
        var outcome = TestContext.CurrentContext.Result.Outcome;
        var state = phase == "start"
            ? "Running"
            : outcome.Label is { Length: > 0 } ? $"{outcome.Status}:{outcome.Label}" : outcome.Status.ToString();
        var elapsed = _stopwatch is null ? "-" : $"{_stopwatch.Elapsed.TotalSeconds:0.000}s";

        TestContext.Progress.WriteLine(
            $"[playwright:{phase}] {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} {state} elapsed={elapsed} {test.FullName}");
        if (tracePath is { Length: > 0 })
            TestContext.Progress.WriteLine($"[playwright:artifact] trace={tracePath}");
        if (screenshotPath is { Length: > 0 })
            TestContext.Progress.WriteLine($"[playwright:artifact] screenshot={screenshotPath}");
    }
}
