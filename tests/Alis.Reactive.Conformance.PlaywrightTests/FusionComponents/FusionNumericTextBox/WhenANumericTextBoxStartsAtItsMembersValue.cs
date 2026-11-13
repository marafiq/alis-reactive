using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANumericTextBoxStartsAtItsMembersValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F32.M21_X1019_0)]
    public async Task the_box_shows_the_dose_the_member_holds()
    {
        var given = await NewWorld();
        await given.Page.GotoAsync(CarePages.DoseRecordGiven);

        await Expect(given.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Dose given", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("12.50");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.DoseRecordBlank);

        await Expect(blank.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Dose given", Exact = true }).GetByRole(AriaRole.Spinbutton)).ToHaveValueAsync("");
    }
}
