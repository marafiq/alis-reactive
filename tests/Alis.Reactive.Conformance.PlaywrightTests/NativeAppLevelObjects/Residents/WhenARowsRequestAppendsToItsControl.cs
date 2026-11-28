using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsRequestAppendsToItsControl(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_38_2)]
    public async Task only_the_clicked_rows_control_shows_busy_and_then_reads_its_whole_text()
    {
        var bea = await NewWorld();
        await bea.Page.GotoAsync(CarePages.DischargeSteps);
        var armed = await bea.Arm(LanesGate.Respond, CareEndpoints.BeasDischarge);
        await bea.Page.Locator("#residents-1-1").ClickAsync();
        var held = await armed.Held();
        await Expect(bea.Page.Locator("#residents-1-1")).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(bea.Page.Locator("#residents-0-1")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await Expect(bea.Page.Locator("#residents-2-1")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await Expect(bea.Page.Locator("#residents-1-0")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await Expect(bea.Page.Locator("#residents-1-2")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await held.Release();
        await Expect(bea.Page.Locator("#residents-1-1")).ToHaveTextAsync("Notify sent");
        await Expect(bea.Page.Locator("#residents-1-1")).Not.ToHaveAttributeAsync("aria-busy", "true");

        var ada = await NewWorld();
        await ada.Page.GotoAsync(CarePages.DischargeSteps);
        var armedAda = await ada.Arm(LanesGate.Respond, CareEndpoints.AdasDischarge);
        await ada.Page.Locator("#residents-1-0").ClickAsync();
        var heldAda = await armedAda.Held();
        await Expect(ada.Page.Locator("#residents-1-0")).ToHaveAttributeAsync("aria-busy", "true");
        await Expect(ada.Page.Locator("#residents-1-1")).Not.ToHaveAttributeAsync("aria-busy", "true");
        await heldAda.Release();
        await Expect(ada.Page.Locator("#residents-1-0")).ToHaveTextAsync("Notify sent");
    }
}
