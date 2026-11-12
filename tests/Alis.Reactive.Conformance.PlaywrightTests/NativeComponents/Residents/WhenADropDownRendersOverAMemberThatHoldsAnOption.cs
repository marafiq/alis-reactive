using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenADropDownRendersOverAMemberThatHoldsAnOption(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F5.M24_X8_0)]
    public async Task the_option_the_member_holds_is_selected()
    {
        var east = await NewWorld();
        await east.Page.GotoAsync(CarePages.ResidentsOfTheEastWing);
        await Expect(east.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Wing", Exact = true })).ToHaveValueAsync("East");

        var west = await NewWorld();
        await west.Page.GotoAsync(CarePages.ResidentsOfTheWestWing);
        await Expect(west.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Wing", Exact = true })).ToHaveValueAsync("West");
    }
}
