using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TriggersAndPipelines;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenMarkupRaisesATypedEventWithNoPayload(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M1_F2.M1_X5_0)]
    public async Task the_handler_runs_and_each_member_of_the_missing_payload_reads_nothing()
    {
        var gate = await NewWorld();
        await gate.Page.GotoAsync(CarePages.PartyBell);
        await gate.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring the bell", Exact = true }).ClickAsync();
        await Expect(gate.Page.Locator("#bell-status")).ToHaveTextAsync("Rung");
        await Expect(gate.Page.Locator("#party-size")).ToHaveTextAsync("");

        var party = await NewWorld();
        await party.Page.GotoAsync(CarePages.PartyBell);
        await party.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ring for three", Exact = true }).ClickAsync();
        await Expect(party.Page.Locator("#party-size")).ToHaveTextAsync("3");
    }
}
