using System.Diagnostics;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Alis.Reactive.PlaywrightTests;

public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl => WebServerFixture.BaseUrl;
    private const int BootPollIntervalMs = 50;
    private const int ReactiveBootTimeoutMs = 60000;
    private const int NavigationTimeoutBootRecoveryMs = 1000;
    private const string ReactiveBootedTrace = "[alis:boot] booted";
    private const string ReactiveBootedMarkerSelector = "html[data-alis-booted='true']";
    private const string ReactiveBootedPagePredicate =
        "() => document.documentElement.dataset.alisBooted === 'true' || window.__alisReactiveBoot?.booted === true";
    private static readonly string[] IgnoredNavigationErrors =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];

    protected readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();
    private readonly object _consoleLock = new();
    private Stopwatch? _testStopwatch;

    [SetUp]
    public async Task SetUpConsoleCapture()
    {
        _testStopwatch = Stopwatch.StartNew();
        WriteProgress("start");

        ClearConsoleState();
        Page.SetDefaultTimeout(60000);
        Page.SetDefaultNavigationTimeout(60000);
        await RouteExternalFontsToLocalFallback();
        await RouteLocalSyncfusionRuntime();

        Page.Console += (_, msg) =>
        {
            lock (_consoleLock)
            {
                _consoleMessages.Add($"[{msg.Type}] {msg.Text}");
                if (msg.Type == "error")
                    _consoleErrors.Add(msg.Text);
            }
        };

        Page.PageError += (_, error) =>
        {
            lock (_consoleLock)
            {
                _consoleErrors.Add($"[PAGE ERROR] {error}");
                _consoleMessages.Add($"[PAGE ERROR] {error}");
            }
        };

        // Failure traces capture screenshots, DOM snapshots, and network in a Playwright .zip.
        await Context.Tracing.StartAsync(new()
        {
            Title = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.Name}",
            Screenshots = true,
            Snapshots = true,
            Sources = false
        });
    }

    private async Task RouteExternalFontsToLocalFallback()
    {
        await Context.RouteAsync("https://fonts.googleapis.com/**", async route =>
            await route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "text/css",
                Body = ""
            }));

        await Context.RouteAsync("https://fonts.gstatic.com/**", async route =>
            await route.FulfillAsync(new() { Status = 204 }));
    }

    private async Task RouteLocalSyncfusionRuntime()
    {
        var syncfusionRuntimePath = FindRequiredFile(
            Path.Combine("node_modules", "@syncfusion", "ej2", "dist", "ej2.min.js"));

        await Context.RouteAsync("**/vendor/syncfusion/dist/ej2.min.js", async route =>
            await route.FulfillAsync(new()
            {
                Status = 200,
                ContentType = "application/javascript",
                Path = syncfusionRuntimePath
            }));
    }

    [TearDown]
    public async Task TearDown()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status
            == NUnit.Framework.Interfaces.TestStatus.Failed;
        var tracePath = default(string);
        var screenshotPath = default(string);

        // Keep failure artifacts beside the test project so wrapper diagnostics use stable paths.
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));
        var traceDir = Path.Combine(projectDir, "TestResults", "playwright-traces");
        Directory.CreateDirectory(traceDir);

        var testName = TestContext.CurrentContext.Test.Name;
        tracePath = failed ? Path.Combine(traceDir, $"{testName}.zip") : null;
        screenshotPath = failed ? Path.Combine(traceDir, $"{testName}.png") : null;

        await Context.Tracing.StopAsync(new() { Path = tracePath });

        if (failed)
        {
            await Page.ScreenshotAsync(new() { Path = screenshotPath!, FullPage = true });
            TestContext.AddTestAttachment(tracePath!, "Playwright trace");
            TestContext.AddTestAttachment(screenshotPath!, "Screenshot on failure");

            var messages = SnapshotConsoleMessages();
            if (messages.Count > 0)
            {
                TestContext.Out.WriteLine("=== Page Console Output ===");
                foreach (var msg in messages)
                    TestContext.Out.WriteLine(msg);
                TestContext.Out.WriteLine("=== End Console Output ===");
            }
        }

        WriteProgress("end", tracePath, screenshotPath);
    }

    protected async Task NavigateTo(string path)
    {
        try
        {
            await Page.GotoAsync($"{BaseUrl}{path}", new()
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 60000
            });
        }
        catch (TimeoutException)
        {
            if (!await PageReachedReactiveBootAfterNavigationTimeout())
                throw;

            TestContext.Out.WriteLine(
                "Navigation timed out after reactive boot was observed; continuing with reactive boot as readiness.");
        }
    }

    protected async Task NavigateToAndWaitForTextSignal(
        string path,
        string selector,
        string placeholder = "\u2014",
        int timeoutMs = 10000)
    {
        await NavigateTo(path);
        await WaitForReactiveBoot(timeoutMs);
        await Expect(Page.Locator(selector))
            .Not.ToHaveTextAsync(placeholder, new() { Timeout = timeoutMs });
    }

    protected async Task NavigateToAndWaitForBoot(
        string path,
        int timeoutMs = 10000)
    {
        await NavigateTo(path);
        await WaitForReactiveBoot(timeoutMs);
    }

    protected async Task NavigateToAndWaitForVisibleSignal(
        string path,
        string selector,
        int timeoutMs = 10000)
    {
        await NavigateTo(path);
        await WaitForReactiveBoot(timeoutMs);
        await Expect(Page.Locator(selector)).ToBeVisibleAsync(new() { Timeout = timeoutMs });
    }

    protected void AssertNoConsoleErrors()
    {
        var unexpected = SnapshotUnexpectedConsoleErrors();
        Assert.That(unexpected, Is.Empty, "Expected no console errors");
    }

    protected void AssertNoConsoleErrorsExcept(params string[] allowedPatterns)
    {
        var unexpected = SnapshotUnexpectedConsoleErrors()
            .Where(e => !allowedPatterns.Any(p => e.Contains(p)))
            .ToList();
        Assert.That(unexpected, Is.Empty, "Expected no unexpected console errors");
    }

    protected async Task WaitForTraceMessage(string containing, int timeoutMs = 5000)
    {
        if (containing == "booted")
        {
            await WaitForReactiveBoot(timeoutMs);
            return;
        }

        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (true)
        {
            if (ConsoleContains(containing))
                return;

            if (DateTime.UtcNow >= deadline)
                break;

            await Task.Delay(100);
        }

        var messages = SnapshotConsoleMessages();
        Assert.Fail($"Timed out waiting for console message containing '{containing}'. " +
                     $"Got {messages.Count} messages: [{string.Join(", ", messages.Take(10))}]");
    }

    protected void AssertTraceContains(string scope, string text)
    {
        var messages = SnapshotConsoleMessages();
        var match = messages.Any(m => m.Contains($"[alis:{scope}]") && m.Contains(text));
        Assert.That(match, Is.True, $"Expected trace [{scope}] to contain '{text}'. " +
                                     $"Messages: [{string.Join(", ", messages.Take(10))}]");
    }

    protected async Task ClickWhenStable(ILocator locator, int timeoutMs = 60000)
    {
        await Expect(locator).ToBeVisibleAsync(new() { Timeout = timeoutMs });
        await Expect(locator).ToBeEnabledAsync(new() { Timeout = timeoutMs });

        try
        {
            await locator.ClickAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine(
                $"ClickWhenStable retrying after timeout for locator '{locator}': {ex.Message}");
            // TODO: Replace this fixed retry pause with a behavior-focused locator stability signal.
            await Page.WaitForTimeoutAsync(250);
            await locator.ClickAsync(new() { Timeout = timeoutMs });
        }
    }

    private bool ConsoleContains(string containing)
    {
        lock (_consoleLock)
        {
            return _consoleMessages.Any(m => m.Contains(containing));
        }
    }

    private List<string> SnapshotConsoleMessages()
    {
        lock (_consoleLock)
        {
            return _consoleMessages.ToList();
        }
    }

    private List<string> SnapshotConsoleErrors()
    {
        lock (_consoleLock)
        {
            return _consoleErrors.ToList();
        }
    }

    private List<string> SnapshotUnexpectedConsoleErrors() =>
        SnapshotConsoleErrors()
            .Where(error => !IgnoredNavigationErrors.Any(error.Contains))
            .ToList();

    private void ClearConsoleState()
    {
        lock (_consoleLock)
        {
            _consoleMessages.Clear();
            _consoleErrors.Clear();
        }
    }

    private async Task WaitForReactiveBoot(int timeoutMs)
    {
        var bootTimeoutMs = Math.Max(timeoutMs, ReactiveBootTimeoutMs);
        if (ConsoleContains(ReactiveBootedTrace))
            return;

        if (await PageReportsReactiveBoot(bootTimeoutMs))
            return;

        if (ConsoleContains(ReactiveBootedTrace) || await PageHasReactiveBootMarker())
            return;

        WriteConsoleMessages(
            "=== Page Console Output While Waiting For Boot ===",
            SnapshotConsoleMessages());
        throw new TimeoutException($"Timed out waiting {bootTimeoutMs}ms for reactive boot.");
    }

    private async Task<bool> PageReportsReactiveBoot(int timeoutMs)
    {
        try
        {
            await Page.WaitForFunctionAsync(
                ReactiveBootedPagePredicate,
                null,
                new() { Timeout = timeoutMs, PollingInterval = BootPollIntervalMs });
            return true;
        }
        catch (TimeoutException)
        {
            return false;
        }
        catch (PlaywrightException ex) when (PageExecutionContextChangedWhileWaiting(ex))
        {
            return false;
        }
    }

    private async Task<bool> PageReachedReactiveBootAfterNavigationTimeout()
    {
        if (ConsoleContains(ReactiveBootedTrace))
            return true;

        if (await PageHasReactiveBootMarker())
            return true;

        return await PageReportsReactiveBoot(NavigationTimeoutBootRecoveryMs);
    }

    private static bool PageExecutionContextChangedWhileWaiting(PlaywrightException ex) =>
        ex.Message.Contains("Execution context was destroyed", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("Target page, context or browser has been closed", StringComparison.OrdinalIgnoreCase);

    private async Task<bool> PageHasReactiveBootMarker()
    {
        try
        {
            return await Page.Locator(ReactiveBootedMarkerSelector).CountAsync() > 0;
        }
        catch (PlaywrightException)
        {
            return false;
        }
    }

    private static void WriteConsoleMessages(string header, IReadOnlyCollection<string> messages)
    {
        if (messages.Count == 0)
            return;

        TestContext.Out.WriteLine(header);
        foreach (var message in messages)
            TestContext.Out.WriteLine(message);
        TestContext.Out.WriteLine("=== End Console Output ===");
    }

    private void WriteProgress(string phase, string? tracePath = null, string? screenshotPath = null)
    {
        var test = TestContext.CurrentContext.Test;
        var result = TestContext.CurrentContext.Result;
        var outcome = phase == "start"
            ? "Running"
            : result.Outcome.Label is { Length: > 0 }
            ? $"{result.Outcome.Status}:{result.Outcome.Label}"
            : result.Outcome.Status.ToString();
        var elapsed = _testStopwatch?.Elapsed;
        var elapsedText = elapsed.HasValue ? $"{elapsed.Value.TotalSeconds:0.000}s" : "-";

        TestContext.Progress.WriteLine(
            $"[playwright:{phase}] {DateTimeOffset.Now:yyyy-MM-dd HH:mm:ss.fff zzz} " +
            $"{outcome} elapsed={elapsedText} {test.FullName}");

        if (tracePath is { Length: > 0 })
            TestContext.Progress.WriteLine($"[playwright:artifact] trace={tracePath}");

        if (screenshotPath is { Length: > 0 })
            TestContext.Progress.WriteLine($"[playwright:artifact] screenshot={screenshotPath}");
    }

    private static string FindRequiredFile(string relativePath)
    {
        var dir = TestContext.CurrentContext.TestDirectory;
        while (dir != null)
        {
            var candidate = Path.Combine(dir, relativePath);
            if (File.Exists(candidate))
                return candidate;

            dir = Path.GetDirectoryName(dir);
        }

        throw new FileNotFoundException($"Required test asset '{relativePath}' was not found.");
    }
}
