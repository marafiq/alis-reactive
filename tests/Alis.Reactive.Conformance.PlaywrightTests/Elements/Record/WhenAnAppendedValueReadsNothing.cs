using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAppendedValueReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X4_0)]
    public async Task the_content_keeps_its_text_and_the_step_after_it_runs()
    {
        var nobody = await NewWorld();
        await nobody.Page.GotoAsync(CarePages.ArrivalsBoard);
        await nobody.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Someone arrived", Exact = true }).ClickAsync();
        await Expect(nobody.Page.Locator("#arrival-status")).ToHaveTextAsync("Noted");
        await Expect(nobody.Page.Locator("#arrivals")).ToHaveTextAsync("Arrivals:");

        var bea = await NewWorld();
        await bea.Page.GotoAsync(CarePages.ArrivalsBoard);
        await bea.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Bea arrived", Exact = true }).ClickAsync();
        await Expect(bea.Page.Locator("#arrival-status")).ToHaveTextAsync("Noted");
        await Expect(bea.Page.Locator("#arrivals")).ToHaveTextAsync("Arrivals: Bea");
    }
}
