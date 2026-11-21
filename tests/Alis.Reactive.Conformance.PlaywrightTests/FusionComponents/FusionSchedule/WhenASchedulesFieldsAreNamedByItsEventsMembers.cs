using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASchedulesFieldsAreNamedByItsEventsMembers(string host) : FusionFixture(host)
{
    private const string AdasShift = "Ada Brooks Begin From Monday, November 9, 2026 at 9:00:00 AM GMT-08:00 Ends At Monday, November 9, 2026 at 5:00:00 PM GMT-08:00";

    private const string LenasShift = "Lena Ortiz Begin From Tuesday, November 10, 2026 at 9:00:00 AM GMT-08:00 Ends At Tuesday, November 10, 2026 at 5:00:00 PM GMT-08:00";

    [Test]
    [FailsFor(Promises.M21_F39.M21_X1054_0)]
    public async Task each_shift_shows_its_carer_and_hours_by_the_names_its_members_carry()
    {
        var rota = await NewWorld();
        await rota.Page.GotoAsync(CarePages.NamedShifts);

        await Expect(rota.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = AdasShift, Exact = true })).ToBeVisibleAsync();
        await Expect(rota.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = LenasShift, Exact = true })).ToBeVisibleAsync();
    }
}
