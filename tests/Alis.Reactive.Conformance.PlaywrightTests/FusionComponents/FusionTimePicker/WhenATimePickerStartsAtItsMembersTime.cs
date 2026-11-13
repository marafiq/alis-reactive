using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATimePickerStartsAtItsMembersTime(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F50.M21_X1031_0)]
    public async Task the_box_shows_the_time_the_member_holds()
    {
        var set = await NewWorld();
        await set.Page.GotoAsync(CarePages.NextDoseSet);

        await Expect(set.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Next dose", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2:30 PM");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.NextDoseOpen);

        await Expect(open.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Next dose", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("");
    }
}
