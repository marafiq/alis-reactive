using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorAsksForAnAllocationReview(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X25_0)]
    public async Task a_review_is_posted_once_readiness_answers_and_its_reply_fills_the_panel_and_the_instructions()
    {
        var reviewed = await NewWorld();
        await reviewed.Page.GotoAsync(CarePages.MoveIn);
        await reviewed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await reviewed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await reviewed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(reviewed.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        var armed = await reviewed.Arm(RunGate.Settle, CareEndpoints.MoveInReviewCheck);
        await reviewed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        var readiness = await armed.Held();
        await Expect(reviewed.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        Assert.That(reviewed.SentTo(CareEndpoints.MoveInAllocationReview), Is.EqualTo(0));
        await readiness.Release();
        await Expect(reviewed.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED4 revision 12");
        await Expect(reviewed.Page.Locator("#allocation-panel").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Cleared");
        await Expect(reviewed.Page.Locator("#allocation-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Write-only", Exact = true })).ToHaveTextAsync("Written Harbor House");
        await Expect(reviewed.Page.Locator("#receipt-ALLOC100")).ToHaveTextAsync("North allocation desk: prepare BED4 revision 12 for Helen Ward.");
        await reviewed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(reviewed.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "AR100: BED4 revision 12, Cleared, North allocation desk" });

        var single = await NewWorld();
        await single.Page.GotoAsync(CarePages.MoveIn);
        await single.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true }).SelectOptionAsync("C2");
        await single.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B3");
        await single.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A21");
        await single.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED21");
        await Expect(single.Page.Locator("#bed-status")).ToHaveTextAsync("Bed 21 bed available");
        await single.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(single.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED21 revision 6");
        await Expect(single.Page.Locator("#allocation-panel").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Cleared");
        await Expect(single.Page.Locator("#allocation-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Write-only", Exact = true })).ToHaveTextAsync("Written Maple Court");
        Assert.That(single.SentTo(CareEndpoints.MoveInReceipts), Is.EqualTo(0));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X26_0)]
    public async Task a_panel_that_is_not_ready_asks_the_server_for_nothing()
    {
        var off = await NewWorld();
        await off.Seed(MoveInChanges.TheCoordinatorMayNotAdmit("CO1"));
        await off.Page.GotoAsync(CarePages.MoveIn);
        await off.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await off.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await off.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(off.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await off.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(off.Page.Locator("#allocation-status")).ToHaveTextAsync("Allocation is not ready");
        Assert.That(off.SentTo(CareEndpoints.MoveInReviewCheck), Is.EqualTo(0));
        await off.Page.GotoAsync(CarePages.MoveIn);
        await Expect(off.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var on = await NewWorld();
        await on.Page.GotoAsync(CarePages.MoveIn);
        await on.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await on.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await on.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(on.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await on.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(on.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED4 revision 12");
        Assert.That(on.SentTo(CareEndpoints.MoveInReviewCheck), Is.EqualTo(1));
        await on.Page.GotoAsync(CarePages.MoveIn);
        await Expect(on.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "AR100: BED4 revision 12, Cleared, North allocation desk" });
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X27_0)]
    public async Task a_bed_taken_since_it_was_listed_shows_who_holds_it_and_no_review_is_kept()
    {
        var taken = await NewWorld();
        await taken.Page.GotoAsync(CarePages.MoveIn);
        await Expect(taken.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await taken.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await taken.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await taken.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(taken.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await taken.Seed(MoveInChanges.TheBedIsTaken());
        await taken.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(taken.Page.Locator("#allocation-status")).ToHaveTextAsync("Bed held by James Reed; review reference Unassigned");
        await Expect(taken.Page.Locator("#allocation-waitlist")).ToHaveTextAsync("A waitlist place can be offered");
        await taken.Page.GotoAsync(CarePages.MoveIn);
        await Expect(taken.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await Expect(taken.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var free = await NewWorld();
        await free.Page.GotoAsync(CarePages.MoveIn);
        await free.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await free.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await free.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(free.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await free.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(free.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED4 revision 12");
        await Expect(free.Page.Locator("#allocation-waitlist")).ToHaveTextAsync("");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X28_0)]
    public async Task a_readiness_call_that_never_answers_ends_the_review_and_posts_nothing()
    {
        var failed = await NewWorld();
        await failed.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInReviewCheck));
        await failed.Page.GotoAsync(CarePages.MoveIn);
        await failed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await failed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await failed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(failed.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await failed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(failed.Page.Locator("#allocation-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Refused");
        await Expect(failed.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await Expect(failed.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        Assert.That(failed.SentTo(CareEndpoints.MoveInAllocationReview), Is.EqualTo(0));
        await failed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(failed.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await answered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await answered.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(answered.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED4 revision 12");
        await Expect(answered.Page.Locator("#allocation-panel").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Settle", Exact = true })).ToHaveTextAsync("Answered");
        Assert.That(answered.SentTo(CareEndpoints.MoveInAllocationReview), Is.EqualTo(1));
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X29_0)]
    public async Task a_review_a_newer_selection_replaced_posts_nothing_when_its_readiness_answers()
    {
        var replaced = await NewWorld();
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(replaced.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        var armed = await replaced.Arm(RunGate.Settle, CareEndpoints.MoveInReviewCheck);
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        var awaiting = await armed.Held();
        await Expect(replaced.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Community", Exact = true }).SelectOptionAsync("C2");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B3");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A21");
        await replaced.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED21");
        await Expect(replaced.Page.Locator("#bed-status")).ToHaveTextAsync("Bed 21 bed available");
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED21 revision 6");
        await awaiting.Release();
        await replaced.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(replaced.Page.Locator("#save-status")).ToHaveTextAsync("Draft saved: version 8");
        await Expect(replaced.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED21 revision 6");
        await replaced.Page.GotoAsync(CarePages.MoveIn);
        await Expect(replaced.Page.Locator("#saved-location")).ToHaveTextAsync("C2 / B3 / A21 / BED21 revision 6");
        await Expect(replaced.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "AR100: BED21 revision 6, Cleared, North allocation desk" });

        var alone = await NewWorld();
        await alone.Page.GotoAsync(CarePages.MoveIn);
        await alone.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await alone.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await alone.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(alone.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        var only = await alone.Arm(RunGate.Settle, CareEndpoints.MoveInReviewCheck);
        await alone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        var held = await only.Held();
        await Expect(alone.Page.Locator("#allocation-status")).ToHaveTextAsync("Not reviewed");
        await held.Release();
        await Expect(alone.Page.Locator("#allocation-status")).ToHaveTextAsync("Cleared: BED4 revision 12");
        await alone.Page.GotoAsync(CarePages.MoveIn);
        await Expect(alone.Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "Allocation reviews", Exact = true }).GetByRole(AriaRole.Listitem))
            .ToHaveTextAsync(new[] { "AR100: BED4 revision 12, Cleared, North allocation desk" });
    }
}
