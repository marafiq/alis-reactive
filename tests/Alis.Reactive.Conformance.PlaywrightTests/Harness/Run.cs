using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Microsoft.Playwright;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

[assembly: Parallelizable(ParallelScope.Fixtures)]
[assembly: AroundTheRun]

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

[AttributeUsage(AttributeTargets.Assembly)]
public sealed class AroundTheRunAttribute : Attribute, ITestAction
{
    public ActionTargets Targets => ActionTargets.Suite;

    public void BeforeTest(ITest test) => Run.OpenTheBrowser().GetAwaiter().GetResult();

    public void AfterTest(ITest test) => Run.CloseTheBrowserAndStopEveryHost().GetAwaiter().GetResult();
}

internal static class Run
{
    private static IPlaywright? _playwright;
    private static IBrowser? _browser;

    internal static IBrowser Browser
        => _browser ?? throw new InvalidOperationException("No browser is open: a test of this project runs inside the project's run, which opens one.");

    internal static async Task OpenTheBrowser()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync();
        Waits.SetOnEveryExpectation();
    }

    internal static async Task CloseTheBrowserAndStopEveryHost()
    {
        ConformanceHost.StopAll();
        if (_browser is not null) await _browser.CloseAsync();
        _playwright?.Dispose();
        _browser = null;
        _playwright = null;
    }
}
