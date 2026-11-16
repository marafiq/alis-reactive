using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFinallysMethodThrowsBeforeItsLastStep(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_25_2)]
    public async Task no_later_step_of_the_finally_or_after_the_request_runs_and_its_busy_target_hides()
    {
        var signedOff = await NewWorld();
        await signedOff.Page.GotoAsync(CarePages.MedicationSignOff);
        var armed = await signedOff.Arm(LanesGate.Respond);
        await signedOff.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("4");
        await signedOff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(signedOff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: review-busy");
        await held.Release();
        await Expect(signedOff.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(signedOff.Page.Locator("#review-history")).ToHaveTextAsync("Reviewed");
        Assert.That(signedOff.ConsoleErrors, Is.EqualTo(new[] { "There is no held item at 4." }));

        var acted = await NewWorld();
        await acted.Page.GotoAsync(CarePages.MedicationSignOff);
        var armedToAct = await acted.Arm(LanesGate.Respond);
        await acted.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("1");
        await acted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var heldToAct = await armedToAct.Held();
        await heldToAct.Release();
        await Expect(acted.Page.Locator("#review-history")).ToHaveTextAsync("ReviewedFinishedClosed");
        await Expect(acted.Page.Locator("#scanner").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Item Dose A", Exact = true })).ToHaveTextAsync("Dose A, acted");
        Assert.That(acted.ConsoleErrors, Is.Empty);
    }
}
