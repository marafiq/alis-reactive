using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorReviewsTheServices(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X140_0)]
    public async Task the_live_service_summary_and_both_awaited_answers_are_saved_with_the_quote()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        var choices = world.Page.Locator("#service-panel").GetByRole(AriaRole.Listbox);
        await choices.PressAsync("ArrowDown");
        await choices.PressAsync("Enter");
        await Expect(world.Page.Locator("#review-change")).ToHaveTextAsync("DIN to PHA. Current: Dining (0). 2 matching. North allocation desk.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review services", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#service-summary")).ToHaveTextAsync("2 active services, 3 visits: Dining, Pharmacy support");
        var visits = await world.Arm(RunGate.Settle, CareEndpoints.MoveInServiceCount);
        var record = await world.Arm(RunGate.Settle, CareEndpoints.MoveInServiceRecord);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        var visitsHeld = await visits.Held();
        Assert.That(world.SentTo(CareEndpoints.MoveInQuote), Is.EqualTo(0));
        await visitsHeld.Release();
        var recordHeld = await record.Held();
        Assert.That(world.SentTo(CareEndpoints.MoveInQuote), Is.EqualTo(0));
        await recordHeld.Release();
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 3040.00 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#review-on-file")).ToHaveTextAsync("2 services, 3 visits. DIN, PHA: Dining, Pharmacy support.");
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 1 calculation on file.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Pharmacy support", Exact = true }).UncheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 2980.00 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 4: 2980.00 USD, assistance 2384.00 USD. 2 calculations on file.");
        await Expect(world.Page.Locator("#review-on-file")).ToHaveTextAsync("1 service, 2 visits. DIN: Dining.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X141_0)]
    public async Task deactivating_every_review_item_leaves_the_previous_quote_in_place()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.Locator("#service-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Deactivate last", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Order by visits", Exact = true }).ClickAsync();
        await world.Page.Locator("#service-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Deactivate last", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review services", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#service-summary")).ToHaveTextAsync("Choose an active service.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pricing-result")).ToHaveTextAsync("Choose an active service.");
        Assert.That(world.SentTo(CareEndpoints.MoveInQuote), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 0 calculations on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X142_0)]
    public async Task capacity_lost_after_review_refuses_the_service_on_the_next_quote()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.Locator("#service-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#service-panel").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Answers" }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Dining", "Pharmacy support" });
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review services", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#service-summary")).ToHaveTextAsync("2 active services, 3 visits: Dining, Pharmacy support");
        await world.Page.Locator("#service-panel").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Ask", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#service-panel").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Answers" }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Dining count: 2", "Pharmacy support count: 1" });
        await world.Seed(MoveInChanges.PharmacyCapacityIsExhausted());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#pricing-result")).ToHaveTextAsync("Pharmacy support has no capacity.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#capacity-PHA")).ToHaveTextAsync("0");
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("No quote on file. 0 calculations on file.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X143_0)]
    public async Task a_refused_component_record_releases_busy_and_leaves_the_draft_unquoted()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Seed(MoveInChanges.TheServiceRecordIsUnavailable());
        await world.Page.GotoAsync(CarePages.MoveIn);
        var record = await world.Arm(RunGate.Settle, CareEndpoints.MoveInServiceRecord);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        var recordHeld = await record.Held();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: quote-panel");
        await recordHeld.Release();
        await Expect(world.Page.Locator("#service-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(world.SentTo(CareEndpoints.MoveInQuote), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("No quote on file. 0 calculations on file.");
        await Expect(world.Page.Locator("#quote-acceptance")).ToHaveTextAsync("Unaccepted");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X144_0)]
    public async Task replacing_a_waiting_review_prices_only_the_new_panels_services()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftAwaitsItsFirstQuote());
        await world.Page.GotoAsync(CarePages.MoveIn);
        var record = await world.Arm(RunGate.Settle, CareEndpoints.MoveInServiceRecord);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        var recordHeld = await record.Held();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: quote-panel");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Pharmacy support", Exact = true }).UncheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review services", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#service-summary")).ToHaveTextAsync("1 active service, 2 visits: Dining");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 2980.00 USD");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read saved service review", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#saved-review")).ToHaveTextAsync("1 service, 2 visits. DIN: Dining. 1 calculation on file.");
        await recordHeld.Release();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Read saved service review", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#saved-review")).ToHaveTextAsync("1 service, 2 visits. DIN: Dining. 1 calculation on file.");
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 2980.00 USD");
        await Expect(world.Page.Locator("#pricing-result")).ToHaveTextAsync("Q100: 2980 USD");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#review-on-file")).ToHaveTextAsync("1 service, 2 visits. DIN: Dining.");
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 2980.00 USD, assistance 2384.00 USD. 1 calculation on file.");
        Assert.That(world.SentTo(CareEndpoints.MoveInQuote), Is.EqualTo(1));
    }
}
