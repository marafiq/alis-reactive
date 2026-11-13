using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASwitchIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F46.M21_X1027_0)]
    public async Task the_switch_starts_on_or_off_as_the_member_holds()
    {
        var up = await NewWorld();
        await up.Page.GotoAsync(CarePages.BedRailsUp);

        await Expect(up.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed rails", Exact = true }).GetByRole(AriaRole.Switch)).ToBeCheckedAsync();

        var down = await NewWorld();
        await down.Page.GotoAsync(CarePages.BedRailsDown);

        await Expect(down.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed rails", Exact = true }).GetByRole(AriaRole.Switch)).ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });
    }
}
