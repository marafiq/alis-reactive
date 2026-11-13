using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARoutesRequestReadsAPathValueTheAnswerLacks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X4_4_0)]
    public async Task the_request_is_not_sent_and_its_finally_still_runs()
    {
        var full = await NewWorld();
        await full.Page.GotoAsync(CarePages.FreeBedOnAFullWard);
        await full.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find a free bed", Exact = true }).ClickAsync();
        await Expect(full.Page.Locator("#bed-status")).ToHaveTextAsync("Closed");
        await Expect(full.Page.Locator("#bed-busy")).ToBeHiddenAsync();
        await Expect(full.Page.Locator("#bed-name")).ToHaveTextAsync("");

        var eastFourFree = await NewWorld();
        await eastFourFree.Page.GotoAsync(CarePages.FreeBedOnAWardWithABedFree);
        await eastFourFree.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find a free bed", Exact = true }).ClickAsync();
        await Expect(eastFourFree.Page.Locator("#bed-name")).ToHaveTextAsync("East 4");
        await Expect(eastFourFree.Page.Locator("#bed-status")).ToHaveTextAsync("Closed");
        await Expect(eastFourFree.Page.Locator("#bed-busy")).ToBeHiddenAsync();
    }
}
