using Microsoft.Playwright.NUnit;

namespace Alis.Reactive.PlaywrightTests;

public abstract class PlaywrightTestBase : PageTest
{
    protected string BaseUrl => WebServerFixture.BaseUrl;

    protected readonly List<string> _consoleMessages = new();
    private readonly List<string> _consoleErrors = new();

    [SetUp]
    public void SetUpConsoleCapture()
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
    }

    [TearDown]
    public void DumpLogsOnFailure()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed
            && _consoleMessages.Count > 0)
        {
            TestContext.Out.WriteLine("=== Browser Console Output ===");
            foreach (var msg in _consoleMessages)
                TestContext.Out.WriteLine(msg);
            TestContext.Out.WriteLine("=== End Console Output ===");
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
