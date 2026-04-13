using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Alis.Reactive.PlaywrightTests;

public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl => WebServerFixture.BaseUrl;
    private const string ReactiveBootedExpression = "() => document.documentElement.dataset.alisBooted === 'true'";
    private static readonly string[] TransientBootRecoveryMarkers =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED",
        "ReferenceError: ej is not defined",
        "ReferenceError: ejs is not defined",
        "Cannot read properties of undefined (reading 'popups')"
    ];
    private static readonly string[] IgnoredConsoleErrorMarkers =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];

    protected readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();
    private readonly object _consoleLock = new();

    [SetUp]
    public async Task SetUpConsoleCapture()
    {
        ClearConsoleState();
        Page.SetDefaultTimeout(60000);
        Page.SetDefaultNavigationTimeout(60000);

        Page.Console += (_, msg) =>
        {
            // Strip %c CSS format markers — Playwright captures them literally
            // in msg.Text, but real Chrome DevTools renders the styles correctly.
            // Keeping them in the captured text would break every substring
            // assertion like `[alis:execute]` or `reaction.dispatch`.
            var text = msg.Text.Replace("%c", "");
            lock (_consoleLock)
            {
                _consoleMessages.Add($"[{msg.Type}] {text}");
                if (msg.Type == "error")
                    _consoleErrors.Add(text);
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

        // Start tracing — captures screenshots, DOM snapshots, and network.
        // On failure, saved as a .zip trace viewable at https://trace.playwright.dev
        await Context.Tracing.StartAsync(new()
        {
            Title = $"{TestContext.CurrentContext.Test.ClassName}.{TestContext.CurrentContext.Test.Name}",
            Screenshots = true,
            Snapshots = true,
            Sources = false
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        var failed = TestContext.CurrentContext.Result.Outcome.Status
            == NUnit.Framework.Interfaces.TestStatus.Failed;

        // Save trace + screenshot on failure to a FIXED path under the test project.
        // Traces: `npx playwright show-trace <path>` or https://trace.playwright.dev
        // Screenshots: PNG files viewable directly
        // Output next to the test .cs files — not buried in bin/Debug/net10.0/
        var projectDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));
        var traceDir = Path.Combine(projectDir, "TestResults", "playwright-traces");
        Directory.CreateDirectory(traceDir);

        var testName = TestContext.CurrentContext.Test.Name;
        var tracePath = failed ? Path.Combine(traceDir, $"{testName}.zip") : null;
        var screenshotPath = failed ? Path.Combine(traceDir, $"{testName}.png") : null;

        await Context.Tracing.StopAsync(new() { Path = tracePath });

        if (failed)
        {
            await Page.ScreenshotAsync(new() { Path = screenshotPath!, FullPage = true });
            TestContext.AddTestAttachment(tracePath!, "Playwright trace");
            TestContext.AddTestAttachment(screenshotPath!, "Screenshot on failure");

            var messages = SnapshotConsoleMessages();
            if (messages.Count > 0)
            {
                TestContext.Out.WriteLine("=== Browser Console Output ===");
                foreach (var msg in messages)
                    TestContext.Out.WriteLine(msg);
                TestContext.Out.WriteLine("=== End Console Output ===");
            }
        }
    }

    protected async Task NavigateTo(string path)
    {
        await Page.GotoAsync($"{BaseUrl}{path}", new()
        {
            WaitUntil = WaitUntilState.Commit,
            Timeout = 60000
        });
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
        var unexpected = FilterUnexpectedConsoleErrors();
        Assert.That(unexpected, Is.Empty, "Expected no console errors");
    }

    protected void AssertNoConsoleErrorsExcept(params string[] allowedPatterns)
    {
        var unexpected = FilterUnexpectedConsoleErrors()
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
        await locator.ScrollIntoViewIfNeededAsync();
        await Expect(locator).ToBeVisibleAsync();
        await Expect(locator).ToBeEnabledAsync();

        try
        {
            await locator.ClickAsync(new() { Timeout = timeoutMs });
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine(
                $"ClickWhenStable retrying after timeout for locator '{locator}': {ex.Message}");
            await locator.ScrollIntoViewIfNeededAsync();
            await Page.WaitForTimeoutAsync(250);
            await locator.ClickAsync(new() { Timeout = timeoutMs });
        }
    }

    private bool HasTransientBootFailure()
    {
        lock (_consoleLock)
        {
            return _consoleMessages.Any(m => TransientBootRecoveryMarkers.Any(m.Contains))
                   || _consoleErrors.Any(m => TransientBootRecoveryMarkers.Any(m.Contains));
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

    private List<string> FilterUnexpectedConsoleErrors()
    {
        return SnapshotConsoleErrors()
            .Where(e => !IgnoredConsoleErrorMarkers.Any(e.Contains))
            .ToList();
    }

    private void ClearConsoleState()
    {
        lock (_consoleLock)
        {
            _consoleMessages.Clear();
            _consoleErrors.Clear();
        }
    }

    private async Task<bool> TryRecoverFromTransientBootFailure()
    {
        if (!HasTransientBootFailure())
            return false;

        WriteConsoleMessages("=== Browser Console Output Before Boot Retry ===", SnapshotConsoleMessages());
        ClearConsoleState();
        await Page.ReloadAsync(new()
        {
            WaitUntil = WaitUntilState.Commit,
            Timeout = 60000
        });
        return true;
    }

    private async Task WaitForReactiveBoot(int timeoutMs)
    {
        try
        {
            await Page.WaitForFunctionAsync(ReactiveBootedExpression, null, new() { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            if (!await TryRecoverFromTransientBootFailure())
                throw;

            await Page.WaitForFunctionAsync(ReactiveBootedExpression, null, new() { Timeout = timeoutMs });
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
}
