namespace Alis.Reactive.PlaywrightTests;

public abstract class PlaywrightTestBase : AppPageTest
{
    private const int ReactiveBootTimeoutMs = 60000;
    private const string ReactiveBootedMarkerSelector = "html[data-alis-booted='true']";
    private static readonly string[] IgnoredNavigationErrors =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];

    private readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();
    private readonly List<(string Containing, TaskCompletionSource Arrived)> _traceWaiters = new();
    private readonly object _consoleLock = new();

    protected IReadOnlyList<string> ConsoleMessages => SnapshotConsoleMessages();

    [SetUp]
    public async Task PrepareSandboxPage()
    {
        ClearConsoleState();
        Page.Console += (_, msg) =>
        {
            lock (_consoleLock)
            {
                var line = $"[{msg.Type}] {msg.Text}";
                _consoleMessages.Add(line);
                if (msg.Type == "error")
                    _consoleErrors.Add(msg.Text);
                foreach (var waiter in _traceWaiters.Where(waiter => line.Contains(waiter.Containing)).ToList())
                {
                    waiter.Arrived.TrySetResult();
                    _traceWaiters.Remove(waiter);
                }
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

    private async Task DisableSyncfusionScriptAnimations()
    {
        await Context.AddInitScriptAsync(
            "window.addEventListener('DOMContentLoaded', () => {" +
            " window.ej?.base?.setGlobalAnimation?.(window.ej.base.GlobalAnimationMode.Disable);" +
            " });");
    }

    protected async Task NavigateTo(string path)
        => await Page.GotoAsync($"{BaseUrl}{path}", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 60000 });

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

        var waiter = (Containing: containing, Arrived: new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously));
        lock (_consoleLock)
        {
            if (_consoleMessages.Any(m => m.Contains(containing)))
                return;
            _traceWaiters.Add(waiter);
        }

        try
        {
            await waiter.Arrived.Task.WaitAsync(TimeSpan.FromMilliseconds(timeoutMs));
        }
        catch (TimeoutException)
        {
            lock (_consoleLock) _traceWaiters.Remove(waiter);
            var messages = SnapshotConsoleMessages();
            Assert.Fail($"Timed out waiting for console message containing '{containing}'. " +
                         $"Got {messages.Count} messages: [{string.Join(", ", messages.Take(10))}]");
        }
    }

    protected void AssertTraceContains(string scope, string text)
    {
        var messages = SnapshotConsoleMessages();
        var match = messages.Any(m => m.Contains($"[alis:{scope}]") && m.Contains(text));
        Assert.That(match, Is.True, $"Expected trace [{scope}] to contain '{text}'. " +
                                     $"Messages: [{string.Join(", ", messages.Take(10))}]");
    }

    protected async Task ClickWhenStable(ILocator locator, int timeoutMs = 60000)
        => await locator.ClickAsync(new() { Timeout = timeoutMs });

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

    private List<string> SnapshotUnexpectedConsoleErrors()
        => SnapshotConsoleErrors()
            .Where(error => !IgnoredNavigationErrors.Any(error.Contains))
            .ToList();

    private void ClearConsoleState()
    {
        lock (_consoleLock)
        {
            _consoleMessages.Clear();
            _consoleErrors.Clear();
            _traceWaiters.Clear();
        }
    }

    private async Task WaitForReactiveBoot(int timeoutMs)
        => await Page.Locator(ReactiveBootedMarkerSelector).WaitForAsync(new()
        {
            State = WaitForSelectorState.Attached,
            Timeout = Math.Max(timeoutMs, ReactiveBootTimeoutMs),
        });
}
