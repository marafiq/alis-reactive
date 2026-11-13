using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepperIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F45.M21_X1026_0)]
    public async Task the_stepper_is_read_at_the_step_the_view_starts_it_on()
    {
        var medications = await NewWorld();
        await medications.Page.GotoAsync(CarePages.AdmissionStepsAtMedications);
        await medications.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which step", Exact = true }).ClickAsync();

        await Expect(medications.Page.Locator("#step-shown")).ToHaveTextAsync("1");

        var room = await NewWorld();
        await room.Page.GotoAsync(CarePages.AdmissionStepsAtRoom);
        await room.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Which step", Exact = true }).ClickAsync();

        await Expect(room.Page.Locator("#step-shown")).ToHaveTextAsync("2");
    }
}
