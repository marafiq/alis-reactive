using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAdmissionHandsARoomToThePayer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X76_0)]
    public async Task the_room_handoff_runs_admission_then_payer_and_records_one_room_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        var armed = await world.Arm(LanesGate.Respond, CareEndpoints.MoveInRoomReviews);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        var review = await armed.Held();
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.Log).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Desk Admission Payer" });
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
        await review.Release();
        await Expect(world.Page.Locator("#room-result")).ToHaveTextAsync("Room review accepted: ROOM7");
        await Expect(world.Page.Locator("#room-payer-contact")).ToHaveTextAsync("Alice Ward is the authorised payer contact for POL-100.");
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");

        var alone = await NewWorld();
        await alone.Page.GotoAsync(CarePages.MoveIn);
        await alone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(alone.Page.Locator("#room-handoff").GetByRole(AriaRole.Log).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Desk" });
        await Expect(alone.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X77_0)]
    public async Task a_short_search_leaves_the_prior_names_and_the_room_history_in_place()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Cedar");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "7 — Cedar", "12 — Cedar" });
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Ce");
        await Expect(world.Page.Locator("#room-search-result")).ToHaveTextAsync("Enter at least three characters.");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "7 — Cedar", "12 — Cedar" });
        Assert.That(world.SentTo(CareEndpoints.MoveInRoomSearch), Is.EqualTo(1));
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X78_0)]
    public async Task a_locked_room_handoff_records_no_review()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Seed(MoveInChanges.TheRoomHandoffIsLocked());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#room-result")).ToHaveTextAsync("Room handoff is locked.");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.Log).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Desk Admission Payer" });
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
        await Expect(world.Page.Locator("#room-lock")).ToHaveTextAsync("Room handoff is locked.");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(answered.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X79_0)]
    public async Task a_failed_search_keeps_the_names_until_the_directory_answers_again()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Willow");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "9 — Willow" });
        await world.Seed(MoveInChanges.TheRoomDirectoryIsAvailable(false));
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Ced");
        await Expect(world.Page.Locator("#room-search-result")).ToHaveTextAsync("The room directory is unavailable.");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "9 — Willow" });
        await world.Seed(MoveInChanges.TheRoomDirectoryIsAvailable(true));
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInRoomSearch));
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Cedar");
        await Expect(world.Page.Locator("#room-search-result")).ToHaveTextAsync("The room search failed.");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "9 — Willow" });
        await world.Seed(MoveInChanges.TheRoomSearchConnectionIsRestored());
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Ceda");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "7 — Cedar", "12 — Cedar" });
        await world.Page.Locator("#room-handoff").GetByRole(AriaRole.Searchbox, new LocatorGetByRoleOptions { Name = "Search", Exact = true }).FillAsync("Cedar");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.List, new LocatorGetByRoleOptions { Name = "Names", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "7 — Cedar", "12 — Cedar" });
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("No room reviews.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X80_0)]
    public async Task replacement_and_restart_leave_the_new_handoff_with_only_its_current_owner()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Replace the callback", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeHiddenAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.Log).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "script" });
        await world.Restart("room-handoff");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeHiddenAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.Log).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Desk Admission" });
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X81_0)]
    public async Task a_throwing_vendor_callback_still_delivers_room_seven_to_both_live_owners()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Allocation assessment", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(world.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Make it throw", Exact = true }).ClickAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Error", Exact = true })).ToHaveTextAsync("The callbacks' own function threw.");
        await Expect(world.Page.Locator("#room-handoff").GetByRole(AriaRole.Log).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Desk Admission Payer" });
        await Expect(world.Page.Locator("#room-result")).ToHaveTextAsync("Room review accepted: ROOM7");
        await Expect(world.Page.Locator("#room-payer-contact")).ToHaveTextAsync("Alice Ward is the authorised payer contact for POL-100.");
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await Expect(world.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");

        var answered = await NewWorld();
        await answered.Page.GotoAsync(CarePages.MoveIn);
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open allocation assessment", Exact = true }).ClickAsync();
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open payer handoff", Exact = true }).ClickAsync();
        await Expect(answered.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Payer handoff", Exact = true })).ToBeVisibleAsync();
        await answered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise room 7", Exact = true }).ClickAsync();
        await Expect(answered.Page.Locator("#room-history")).ToHaveTextAsync("ROOM7: Room 7, accepted by Maya Chen.");
    }
}
