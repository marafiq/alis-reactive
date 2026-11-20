using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACoordinatorSubmitsAdmission(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X220_0)]
    public async Task a_ready_draft_commits_the_admission_and_reads_its_checklist()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-status")).ToHaveTextAsync("Helen Ward admitted to apartment 7");
        await Expect(world.Page.Locator("#admission-receipt")).ToHaveTextAsync("ADM100, monthly total 3040 USD");
        await Expect(world.Page.Locator("#arrival-checklist")).ToHaveTextAsync("Welcome Helen Ward to apartment 7, Window bed.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Submitted");
        await Expect(world.Page.Locator("#admission-on-file")).ToHaveTextAsync("ADM100 for R101 in BED4. 1 bed allocation. 1 admission history entry.");
        await Expect(world.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 13, occupied by Helen Ward");

        var changed = await NewWorld();
        await changed.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await changed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await changed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await changed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await changed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("50");
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check medications", Exact = true }).ClickAsync();
        await Expect(changed.Page.Locator("#pharmacy-status")).ToHaveTextAsync("Pharmacy cleared: CL100");
        await changed.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Dose", Exact = true }).FillAsync("75");
        await changed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(changed.Page.Locator("#admission-status")).ToHaveTextAsync("The admission medications need current pharmacy clearance.");
        await changed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(changed.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(changed.Page.Locator("#admission-on-file")).ToHaveTextAsync("No admission on file.");

        var closed = await NewWorld();
        await closed.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await closed.Page.GotoAsync(CarePages.MoveIn);
        await closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await closed.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await closed.Seed(MoveInChanges.TheBuildingIsClosed("B1"));
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#admission-status")).ToHaveTextAsync("The selected accommodation is no longer eligible.");
        await closed.Page.GotoAsync(CarePages.MoveIn);
        await Expect(closed.Page.Locator("#admission-on-file")).ToHaveTextAsync("No admission on file.");
        await Expect(closed.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X221_0)]
    public async Task a_missing_bed_stops_submission_before_sending()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#move-in_summary")).ToHaveTextAsync("Choose a bed.");
        Assert.That(world.SentTo(CareEndpoints.MoveInAdmissions), Is.EqualTo(0));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#admission-on-file")).ToHaveTextAsync("No admission on file.");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#saved-location")).ToHaveTextAsync("C1 / B1 / A7 / BED4 revision 12");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X222_0)]
    public async Task a_bed_taken_after_selection_offers_the_current_holders_waitlist()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-status")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await Expect(world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true })).ToHaveCountAsync(1);
        await Expect(world.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("Family requests a window bed.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-result")).ToHaveTextAsync("Helen Ward is number 1 on the waitlist");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await Expect(world.Page.Locator("#admission-on-file")).ToHaveTextAsync("No admission on file.");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. Family requests a window bed.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X223_0)]
    public async Task a_lost_admission_post_leaves_the_draft_open()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInAdmissions));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: Admission could not be submitted.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Open");
        await Expect(world.Page.Locator("#admission-on-file")).ToHaveTextAsync("No admission on file.");
        await Expect(world.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X224_0)]
    public async Task the_checklist_tail_excludes_another_command_until_the_explicit_replay()
    {
        var world = await NewWorld();
        await world.Seed(MoveInChanges.TheDraftIsReadyForAdmission());
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await Expect(world.Page.Locator("#bed-status")).ToHaveTextAsync("Window bed available");
        var checklist = await world.Arm(LanesGate.SettleTheTail, CareEndpoints.MoveInArrivalChecklist);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        var tail = await checklist.Held();
        await Expect(world.Page.Locator("#admission-status")).ToHaveTextAsync("Helen Ward admitted to apartment 7");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInAdmissions), Is.EqualTo(1));
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await tail.Release();
        await Expect(world.Page.Locator("#arrival-checklist")).ToHaveTextAsync("Welcome Helen Ward to apartment 7, Window bed.");
        Assert.That(world.SentTo(CareEndpoints.MoveInAdmissions), Is.EqualTo(1));
        Assert.That(world.SentTo(CarePages.MoveIn), Is.EqualTo(1));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit admission", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#admission-status")).ToHaveTextAsync("Existing admission ADM100");
        Assert.That(world.SentTo(CareEndpoints.MoveInAdmissions), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#admission-on-file")).ToHaveTextAsync("ADM100 for R101 in BED4. 1 bed allocation. 1 admission history entry.");
        await Expect(world.Page.Locator("#allocation-bed")).ToHaveTextAsync("BED4 revision 13, occupied by Helen Ward");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save draft", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: This draft is closed.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#waitlist-draft")).ToHaveTextAsync("Submitted");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("8");
    }
}
