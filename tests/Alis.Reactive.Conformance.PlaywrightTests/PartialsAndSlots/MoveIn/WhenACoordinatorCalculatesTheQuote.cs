using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorCalculatesTheQuote(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X125_0)]
    public async Task the_quote_and_assistance_wait_for_the_independent_catalogue_before_the_final_total()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        var catalogue = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInServices);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        var catalogueHeld = await catalogue.Held();
        await Expect(world.Page.Locator("#quote-assistance")).ToHaveTextAsync("Harbor Mutual contribution: 2432.00");
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Calculating");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: quote-panel");
        await catalogueHeld.Release();
        await Expect(world.Page.Locator("#quote-catalogue")).ToHaveTextAsync("3 available services");
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 3040.00 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 1 calculation on file.");

        var ready = await NewWorld();
        await ready.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await ready.Page.GotoAsync(CarePages.MoveIn);
        await Expect(ready.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 3");
        await ready.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(ready.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 3040.00 USD");
        await ready.Page.GotoAsync(CarePages.MoveIn);
        await Expect(ready.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Accepted revision 3");
        await ready.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Pharmacy support", Exact = true }).UncheckAsync();
        await ready.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(ready.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 2980.00 USD");
        await ready.Page.GotoAsync(CarePages.MoveIn);
        await Expect(ready.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X126_0)]
    public async Task empty_selection_stops_pricing_while_the_catalogue_completes()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Dining", Exact = true }).UncheckAsync();
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Pharmacy support", Exact = true }).UncheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#quote-catalogue")).ToHaveTextAsync("3 available services");
        Assert.That(world.SentTo(CareEndpoints.MoveInQuote), Is.EqualTo(0));
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Choose at least one service.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("No quote on file. 0 calculations on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X127_0)]
    public async Task a_service_with_no_capacity_is_refused_by_the_pricing_desk()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Transport", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pricing-result")).ToHaveTextAsync("Transport has no capacity.");
        await Expect(world.Page.Locator("#quote-catalogue")).ToHaveTextAsync("3 available services");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("No quote on file. 0 calculations on file.");
        await Expect(world.Page.Locator("#transport-capacity")).ToHaveTextAsync("0");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X128_0)]
    public async Task the_catalogue_connection_can_fail_after_a_quote_is_stored()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInServices));
        await world.HoldTheClock();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await world.RunTheClockUntilItSends(CareEndpoints.MoveInServices, 4);
        await Expect(world.Page.Locator("#quote-catalogue")).ToHaveTextAsync("Service catalogue unavailable.");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 1 calculation on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X129_0)]
    public async Task assistance_holds_the_final_price_after_both_main_branches_answer()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        var quote = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInQuote);
        var catalogue = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInServices);
        var assistance = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInAssistance);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        var quoteHeld = await quote.Held();
        var catalogueHeld = await catalogue.Held();
        await quoteHeld.Release();
        var assistanceHeld = await assistance.Held();
        await catalogueHeld.Release();
        await Expect(world.Page.Locator("#quote-catalogue")).ToHaveTextAsync("3 available services");
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Calculating");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: quote-panel");
        await assistanceHeld.Release();
        await Expect(world.Page.Locator("#quote-assistance")).ToHaveTextAsync("Harbor Mutual contribution: 2432.00");
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 3040.00 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 1 calculation on file.");
    }
}
