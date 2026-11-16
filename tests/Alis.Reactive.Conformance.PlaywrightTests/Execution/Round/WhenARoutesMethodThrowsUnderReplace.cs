using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Execution;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARoutesMethodThrowsUnderReplace(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M17_F1.X11_25_1)]
    public async Task the_error_is_logged_once_its_finally_runs_its_busy_target_hides_and_the_next_raise_runs_the_handler_again()
    {
        var reviewed = await NewWorld();
        await reviewed.Page.GotoAsync(CarePages.MedicationReviewUnderReplace);
        var armed = await reviewed.Arm(LanesGate.Respond);
        await reviewed.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("4");
        await reviewed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(reviewed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: review-busy");
        await held.Release();
        await Expect(reviewed.Page.Locator("#review-history")).ToHaveTextAsync("Finished");
        await Expect(reviewed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(reviewed.ConsoleErrors, Is.EqualTo(new[] { "There is no held item at 4." }));
        var armedAgain = await reviewed.Arm(LanesGate.Respond);
        await reviewed.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("1");
        await reviewed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var heldAgain = await armedAgain.Held();
        await heldAgain.Release();
        await Expect(reviewed.Page.Locator("#review-history")).ToHaveTextAsync("FinishedFinished");
        await Expect(reviewed.Page.Locator("#scanner").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Item Dose A", Exact = true }))
            .ToHaveTextAsync(new[] { "Dose A, not acted", "Dose A, acted" });

        var acted = await NewWorld();
        await acted.Page.GotoAsync(CarePages.MedicationReviewUnderReplace);
        var armedToAct = await acted.Arm(LanesGate.Respond);
        await acted.Page.Locator("#scanner").GetByRole(AriaRole.Searchbox).FillAsync("1");
        await acted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        var heldToAct = await armedToAct.Held();
        await heldToAct.Release();
        await Expect(acted.Page.Locator("#review-history")).ToHaveTextAsync("Finished");
        await Expect(acted.Page.Locator("#scanner").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Item Dose A", Exact = true })).ToHaveTextAsync("Dose A, acted");
        Assert.That(acted.ConsoleErrors, Is.Empty);
    }
}
