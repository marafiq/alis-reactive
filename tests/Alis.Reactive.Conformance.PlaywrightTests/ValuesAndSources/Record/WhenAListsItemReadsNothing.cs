using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListsItemReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_22_0)]
    public async Task the_endpoint_receives_the_item_as_null_and_an_item_that_reads_a_carer_as_the_carer()
    {
        var nobodyOnCall = await NewWorld();
        await nobodyOnCall.Page.GotoAsync(CarePages.CarerHandover);
        await nobodyOnCall.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send handover", Exact = true }).ClickAsync();
        await Expect(nobodyOnCall.Page.Locator("#handover")).ToHaveTextAsync("Handover recorded: Ada, no carer on call");

        var benOnCall = await NewWorld();
        await benOnCall.Seed(Seed.Carer("Ben"));
        await benOnCall.Page.GotoAsync(CarePages.CarerHandover);
        await benOnCall.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send handover", Exact = true }).ClickAsync();
        await Expect(benOnCall.Page.Locator("#handover")).ToHaveTextAsync("Handover recorded: Ada, Ben");
    }
}
