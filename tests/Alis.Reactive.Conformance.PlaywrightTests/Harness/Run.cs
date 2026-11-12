using Microsoft.Playwright;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[SetUpFixture]
public sealed class Run
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    internal static IBrowser Browser
        => _browser ?? throw new InvalidOperationException("No browser is open: a test of this project runs inside the project's run, which opens one.");

    [OneTimeSetUp]
    public async Task OpenTheBrowser()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        Waits.SetOnEveryExpectation();
    }

    [OneTimeTearDown]
    public async Task CloseTheBrowserAndStopEveryHost()
    {
        ConformanceHost.StopAll();
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
        _browser = null;
        _playwright = null;
    }
}
