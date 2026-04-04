using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Support.Infrastructure;

internal sealed class BrowserDiagnosticsSession
{
    private static readonly string[] IgnoredConsoleErrorMarkers =
    [
        "ERR_NETWORK_CHANGED",
        "net::ERR_NETWORK_CHANGED",
        "ERR_NETWORK_IO_SUSPENDED"
    ];

    private readonly IPage _page;
    private readonly IBrowserContext _context;
    private readonly string _testName;
    private readonly object _consoleLock = new();
    private readonly List<string> _consoleMessages = [];
    private readonly List<string> _consoleErrors = [];

    internal BrowserDiagnosticsSession(IPage page, IBrowserContext context, string testName)
    {
        _page = page;
        _context = context;
        _testName = testName;
    }

    internal async Task StartAsync()
    {
        Clear();

        _page.Console += (_, msg) =>
        {
            lock (_consoleLock)
            {
                _consoleMessages.Add($"[{msg.Type}] {msg.Text}");
                if (msg.Type == "error")
                    _consoleErrors.Add(msg.Text);
            }
        };

        _page.PageError += (_, error) =>
        {
            lock (_consoleLock)
            {
                _consoleErrors.Add($"[PAGE ERROR] {error}");
                _consoleMessages.Add($"[PAGE ERROR] {error}");
            }
        };

        _page.RequestFailed += (_, request) =>
        {
            var failureText = request.Failure is null
                ? "[REQUEST FAILED] unknown failure"
                : $"[REQUEST FAILED] {request.Url} :: {request.Failure}";

            lock (_consoleLock)
            {
                _consoleErrors.Add(failureText);
                _consoleMessages.Add(failureText);
            }
        };

        _page.Response += async (_, response) =>
        {
            if (response.Status < 500)
                return;

            var failureText = $"[HTTP {response.Status}] {response.Url}";

            try
            {
                var body = await response.TextAsync();
                if (!string.IsNullOrWhiteSpace(body))
                    failureText += $" :: {Truncate(body, 400)}";
            }
            catch
            {
            }

            lock (_consoleLock)
            {
                _consoleErrors.Add(failureText);
                _consoleMessages.Add(failureText);
            }
        };

        await _context.Tracing.StartAsync(new()
        {
            Title = _testName,
            Screenshots = true,
            Snapshots = true,
            Sources = false
        });
    }

    internal async Task CompleteAsync(bool failed)
    {
        var traceDir = Path.Combine(ProjectDirectory(), "TestResults", "playwright-traces");
        Directory.CreateDirectory(traceDir);

        var tracePath = failed ? Path.Combine(traceDir, $"{_testName}.zip") : null;
        var screenshotPath = failed ? Path.Combine(traceDir, $"{_testName}.png") : null;

        await _context.Tracing.StopAsync(new() { Path = tracePath });

        if (!failed)
            return;

        await _page.ScreenshotAsync(new() { Path = screenshotPath!, FullPage = true });
        TestContext.AddTestAttachment(tracePath!, "Playwright trace");
        TestContext.AddTestAttachment(screenshotPath!, "Screenshot on failure");

        var messages = SnapshotMessages();
        if (messages.Count > 0)
        {
            TestContext.Out.WriteLine("=== Browser Console Output ===");
            foreach (var msg in messages)
                TestContext.Out.WriteLine(msg);
            TestContext.Out.WriteLine("=== End Console Output ===");
        }

        var serverLines = WebServerFixture.RecentServerOutput;
        if (serverLines.Count == 0)
            return;

        TestContext.Out.WriteLine("=== Sandbox Server Output ===");
        foreach (var line in serverLines)
            TestContext.Out.WriteLine(line);
        TestContext.Out.WriteLine("=== End Sandbox Server Output ===");
    }

    internal void AssertNoConsoleErrors()
    {
        Assert.That(FilterUnexpectedConsoleErrors(), Is.Empty, "Expected no console errors");
    }

    internal void AssertNoConsoleErrorsExcept(params string[] allowedPatterns)
    {
        var unexpected = FilterUnexpectedConsoleErrors()
            .Where(error => !allowedPatterns.Any(pattern => error.Contains(pattern)))
            .ToList();

        Assert.That(unexpected, Is.Empty, "Expected no unexpected console errors");
    }

    private string ProjectDirectory() =>
        Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", ".."));

    private List<string> FilterUnexpectedConsoleErrors() =>
        SnapshotErrors()
            .Where(error => !IgnoredConsoleErrorMarkers.Any(error.Contains))
            .ToList();

    private List<string> SnapshotMessages()
    {
        lock (_consoleLock)
        {
            return _consoleMessages.ToList();
        }
    }

    private List<string> SnapshotErrors()
    {
        lock (_consoleLock)
        {
            return _consoleErrors.ToList();
        }
    }

    private void Clear()
    {
        lock (_consoleLock)
        {
            _consoleMessages.Clear();
            _consoleErrors.Clear();
        }
    }

    private static string Truncate(string value, int maxLength)
    {
        if (value.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength) + "...";
    }
}
