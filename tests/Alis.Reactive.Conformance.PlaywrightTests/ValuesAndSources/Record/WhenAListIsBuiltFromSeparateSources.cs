using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListIsBuiltFromSeparateSources(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F1.X7_21_0)]
    public async Task the_endpoint_binds_each_sources_value_in_order_and_a_list_of_one_as_a_list()
    {
        var benOnCall = await NewWorld();
        await benOnCall.Seed(Seed.Carer("Ben"));
        await benOnCall.Page.GotoAsync(CarePages.CarerHandover);
        await benOnCall.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send handover", Exact = true }).ClickAsync();
        await Expect(benOnCall.Page.Locator("#handover")).ToHaveTextAsync("Handover recorded: Ada, Ben");

        var dayAlone = await NewWorld();
        await dayAlone.Page.GotoAsync(CarePages.CarerHandover);
        await dayAlone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send day handover", Exact = true }).ClickAsync();
        await Expect(dayAlone.Page.Locator("#handover")).ToHaveTextAsync("Handover recorded: Ada");
    }
}
