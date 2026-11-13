using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACheckBoxStartsAtItsMembersValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F10.M21_X1006_0)]
    public async Task the_box_shows_ticked_when_the_member_holds_true()
    {
        var given = await NewWorld();
        await given.Page.GotoAsync(CarePages.PhotoConsentGiven);

        await Expect(given.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Photo consent", Exact = true }).GetByRole(AriaRole.Checkbox)).ToBeCheckedAsync();

        var withheld = await NewWorld();
        await withheld.Page.GotoAsync(CarePages.PhotoConsentWithheld);

        await Expect(withheld.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Photo consent", Exact = true }).GetByRole(AriaRole.Checkbox)).Not.ToBeCheckedAsync();
    }
}
