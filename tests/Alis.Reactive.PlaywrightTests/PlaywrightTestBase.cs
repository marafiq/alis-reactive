namespace Alis.Reactive.PlaywrightTests;

/// <summary>
/// A page test against the sandbox: the shared page lifecycle (<see cref="AppPageTest"/>) plus what the
/// sandbox's pages need, the Syncfusion runtime served from node_modules with its animations off and the
/// layout's web fonts left out, and the sandbox suites' own helpers over the page's console.
/// </summary>
public abstract class PlaywrightTestBase : AppPageTest
{
    private const int BootPollIntervalMs = 50;
    private const int ReactiveBootTimeoutMs = 60000;
    private const int NavigationTimeoutBootRecoveryMs = 1000;
    private const string ReactiveBootedTrace = "[alis:boot] booted";
    private const string ReactiveBootedMarkerSelector = "html[data-alis-booted='true']";
    private const string ReactiveBootedPagePredicate = "() => document.documentElement.dataset.alisBooted === 'true'";
    private static readonly string[] IgnoredNavigationErrors =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];

    private readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();
    private readonly object _consoleLock = new();

    /// <summary>The page's console so far, copied under the lock the console handler writes under.</summary>
    protected IReadOnlyList<string> ConsoleMessages => SnapshotConsoleMessages();

    [SetUp]
    public async Task PrepareSandboxPage()
    {
        ClearConsoleState();
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

        await RouteExternalFontsToLocalFallback();
        await RouteLocalSyncfusionRuntime();
        await DisableSyncfusionScriptAnimations();
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

    // Playwright's "stable" actionability check fights animated bounding boxes, so tests
    // disable EJ2 script animations here — at the harness boundary — leaving the sandbox
    // itself animated and truthful for eyes-first verification.
    private async Task DisableSyncfusionScriptAnimations()
    {
        await Context.AddInitScriptAsync(
            "window.addEventListener('DOMContentLoaded', () => {" +
            " window.ej?.base?.setGlobalAnimation?.(window.ej.base.GlobalAnimationMode.Disable);" +
            " });");
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
