using Microsoft.Playwright.NUnit;

namespace Alis.Reactive.PlaywrightTests;

public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl => WebServerFixture.BaseUrl;

    protected readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();

    [SetUp]
    public async Task SetUpConsoleCapture()
    {
        _consoleMessages.Clear();
        _consoleErrors.Clear();

        Page.Console += (_, msg) =>
        {
            _consoleMessages.Add($"[{msg.Type}] {msg.Text}");
            if (msg.Type == "error")
                _consoleErrors.Add(msg.Text);
        };

        Page.PageError += (_, error) =>
        {
            _consoleErrors.Add($"[PAGE ERROR] {error}");
            _consoleMessages.Add($"[PAGE ERROR] {error}");
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

            if (_consoleMessages.Count > 0)
            {
                TestContext.Out.WriteLine("=== Browser Console Output ===");
                foreach (var msg in _consoleMessages)
                    TestContext.Out.WriteLine(msg);
                TestContext.Out.WriteLine("=== End Console Output ===");
            }
        }
    }

    protected async Task NavigateTo(string path)
    {
        await Page.GotoAsync($"{BaseUrl}{path}");
    }

    protected void AssertNoConsoleErrors()
    {
        Assert.That(_consoleErrors, Is.Empty, "Expected no console errors");
    }

    protected void AssertNoConsoleErrorsExcept(params string[] allowedPatterns)
    {
        var unexpected = _consoleErrors
            .Where(e => !allowedPatterns.Any(p => e.Contains(p)))
            .ToList();
        Assert.That(unexpected, Is.Empty, "Expected no unexpected console errors");
    }

    protected async Task WaitForTraceMessage(string containing, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (_consoleMessages.Any(m => m.Contains(containing)))
                return;
            await Task.Delay(100);
        }

        Assert.Fail($"Timed out waiting for console message containing '{containing}'. " +
                     $"Got {_consoleMessages.Count} messages: [{string.Join(", ", _consoleMessages.Take(10))}]");
    }

    protected void AssertTraceContains(string scope, string text)
    {
        var match = _consoleMessages.Any(m => m.Contains($"[alis:{scope}]") && m.Contains(text));
        Assert.That(match, Is.True, $"Expected trace [{scope}] to contain '{text}'. " +
                                     $"Messages: [{string.Join(", ", _consoleMessages.Take(10))}]");
    }
}
