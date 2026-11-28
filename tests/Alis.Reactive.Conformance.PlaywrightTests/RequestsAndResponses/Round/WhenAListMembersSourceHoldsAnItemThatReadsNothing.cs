using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListMembersSourceHoldsAnItemThatReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_23_0)]
    public async Task an_empty_field_is_sent_in_its_place_and_the_endpoint_binds_a_null_item()
    {
        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.StaffRosterWithAnOpenShift);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send roster", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#roster-echo")).ToHaveTextAsync("3 items: Day, none, Night");

        var filled = await NewWorld();
        await filled.Page.GotoAsync(CarePages.StaffRosterWithEveryShiftFilled);
        await filled.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send roster", Exact = true }).ClickAsync();
        await Expect(filled.Page.Locator("#roster-echo")).ToHaveTextAsync("2 items: Day, Night");
    }
}
