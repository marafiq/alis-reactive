using Alis.Reactive.Playwright.Extensions;
using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;

namespace Alis.Reactive.PlaywrightTests;

public abstract class PlaywrightTestBase : PageTest
{
    private sealed record RequestFailure(string Key, string Message);

    protected string BaseUrl => WebServerFixture.BaseUrl;
    private const string ReactiveBootedSelector = "html";
    private const string ReactiveBootedAttributeName = "data-alis-booted";
    private const string ReactiveBootedAttributeValue = "true";
    private const string ReactiveBootedExpression = "() => document.documentElement.dataset.alisBooted === 'true'";
    private const string ReactiveBootedConsoleMarker = "[alis:boot] booted";
    private static readonly string[] TransientBootRecoveryMarkers =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];
    private static readonly string[] IgnoredConsoleErrorMarkers =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];

    protected readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();
    private readonly List<RequestFailure> _requestFailures = new();
    private readonly HashSet<string> _successfulRequestKeys = new(StringComparer.Ordinal);
    private readonly object _consoleLock = new();

    [SetUp]
    public async Task SetUpConsoleCapture()
    {
        ClearConsoleState();
        Page.SetDefaultTimeout(60000);
        Page.SetDefaultNavigationTimeout(60000);

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

        Page.Response += (_, response) =>
        {
            if (!response.Ok)
                return;

            lock (_consoleLock)
            {
                _successfulRequestKeys.Add(BuildRequestKey(response.Request.Method, response.Url));
            }
        };

        Page.RequestFailed += (_, request) =>
        {
            var failureText = request.Failure is null
                ? "[REQUEST FAILED] unknown failure"
                : $"[REQUEST FAILED] {request.Url} :: {request.Failure}";
            var requestKey = BuildRequestKey(request.Method, request.Url);

            lock (_consoleLock)
            {
                _consoleErrors.Add(failureText);
                _consoleMessages.Add(failureText);
                _requestFailures.Add(new(requestKey, failureText));
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

    protected void AssertV2Plan(string? planJson)
    {
        Assert.That(planJson, Is.Not.Null.And.Not.Empty, "Plan JSON must be present");
        Assert.That(planJson, Does.Contain("\"version\": 2"), "Plan must declare V2");
        Assert.That(planJson, Does.Contain("\"contracts\""), "Plan must contain contracts");
        Assert.That(planJson, Does.Contain("\"objects\""), "Plan must contain objects");
        Assert.That(planJson, Does.Contain("\"bindings\""), "Plan must contain bindings");
        Assert.That(planJson, Does.Contain("\"workflows\""), "Plan must contain workflows");
    }

    protected void AssertV2MemberAction(string? planJson)
    {
        var hasMemberAction =
            planJson?.Contains("\"kind\": \"set\"") == true
            || planJson?.Contains("\"kind\": \"call\"") == true;

        Assert.That(hasMemberAction, Is.True,
            "Plan must contain V2 member actions ('set' or 'call')");
    }

    protected void AssertPlanResolver(string? planJson, string resolver)
    {
        Assert.That(planJson, Does.Contain($"\"resolver\": \"{resolver}\""),
            $"Plan must contain resolver '{resolver}'");
    }

    protected void AssertPlanValueMember(string? planJson, string member)
    {
        Assert.That(planJson, Does.Contain($"\"valueMember\": \"{member}\""),
            $"Plan must contain valueMember '{member}'");
    }

    protected void AssertPlanPathProp(string? planJson, string prop)
    {
        Assert.That(planJson, Does.Contain($"\"prop\": \"{prop}\""),
            $"Plan must contain path prop '{prop}'");
    }

    protected void AssertPlanScalarType(string? planJson, string type)
    {
        Assert.That(planJson, Does.Contain($"\"type\": \"{type}\""),
            $"Plan must contain scalar type '{type}'");
    }

    protected async Task ClickWhenStable(ILocator locator, int timeoutMs = 60000)
        => await locator.ClickWhenStableAsync(Page, timeoutMs);

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
        var successfulRequestKeys = SnapshotSuccessfulRequestKeys();
        var requestFailures = SnapshotRequestFailures();

        return SnapshotConsoleErrors()
            .Where(e => !IgnoredConsoleErrorMarkers.Any(e.Contains))
            .Where(e => !IsResolvedAbortedRequest(e, successfulRequestKeys, requestFailures))
            .ToList();
    }

    private void ClearConsoleState()
    {
        lock (_consoleLock)
        {
            _consoleMessages.Clear();
            _consoleErrors.Clear();
            _requestFailures.Clear();
            _successfulRequestKeys.Clear();
        }
    }

    private async Task<bool> TryRecoverFromTransientBootFailure()
    {
        await Task.Delay(100);

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
        if (await WaitForReactiveBootSignal(timeoutMs))
            return;

        if (await TryRecoverFromTransientBootFailure() && await WaitForReactiveBootSignal(timeoutMs))
            return;

        var messages = SnapshotConsoleMessages();
        Assert.Fail($"Timed out waiting for reactive boot after {timeoutMs}ms. " +
                    $"Got {messages.Count} console messages: [{string.Join(", ", messages.Take(10))}]");
    }

    private async Task<bool> WaitForReactiveBootSignal(int timeoutMs)
    {
        var html = Page.Locator(ReactiveBootedSelector);
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        try
        {
            await Page.WaitForLoadStateAsync(LoadState.Load, new() { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            // Fall through to signal polling so the failure message still reflects the
            // reactive boot contract rather than the browser load state alone.
        }
        catch (PlaywrightException)
        {
            // Ignore transient navigation churn and let the signal polling decide.
        }

        while (DateTime.UtcNow < deadline)
        {
            if (ConsoleContains(ReactiveBootedConsoleMarker))
                return true;

            try
            {
                if (await html.GetAttributeAsync(ReactiveBootedAttributeName) == ReactiveBootedAttributeValue)
                    return true;
            }
            catch (PlaywrightException)
            {
                // Ignore transient execution-context churn while the page is still settling.
            }

            try
            {
                if (await Page.EvaluateAsync<bool>(ReactiveBootedExpression))
                    return true;
            }
            catch (PlaywrightException)
            {
                // Ignore transient execution-context churn while the page is still settling.
            }

            await Page.WaitForTimeoutAsync(100);
        }

        await Page.WaitForTimeoutAsync(200);
        return ConsoleContains(ReactiveBootedConsoleMarker);
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

    private List<RequestFailure> SnapshotRequestFailures()
    {
        lock (_consoleLock)
        {
            return _requestFailures.ToList();
        }
    }

    private HashSet<string> SnapshotSuccessfulRequestKeys()
    {
        lock (_consoleLock)
        {
            return new HashSet<string>(_successfulRequestKeys, StringComparer.Ordinal);
        }
    }

    private static bool IsResolvedAbortedRequest(
        string message,
        HashSet<string> successfulRequestKeys,
        IReadOnlyCollection<RequestFailure> requestFailures)
    {
        if (!message.Contains("ERR_ABORTED", StringComparison.OrdinalIgnoreCase))
            return false;

        return requestFailures.Any(failure =>
            failure.Message == message
            && successfulRequestKeys.Contains(failure.Key));
    }

    private static string BuildRequestKey(string method, string url) => $"{method} {url}";
}
