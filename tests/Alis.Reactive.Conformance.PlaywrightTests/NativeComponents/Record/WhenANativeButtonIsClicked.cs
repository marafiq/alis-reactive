using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANativeButtonIsClicked(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F2.M24_X6_0)]
    public async Task the_click_runs_the_pipeline_the_button_declares()
    {
        var called = await NewWorld();
        await called.Page.GotoAsync(CarePages.BedsideRecord);
        await called.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Call the nurse", Exact = true }).ClickAsync();
        await Expect(called.Page.Locator("#call-status")).ToHaveTextAsync("Called");

        var waiting = await NewWorld();
        await waiting.Page.GotoAsync(CarePages.BedsideRecord);
        await Expect(waiting.Page.Locator("#call-status")).ToHaveTextAsync("Waiting");
    }
}
