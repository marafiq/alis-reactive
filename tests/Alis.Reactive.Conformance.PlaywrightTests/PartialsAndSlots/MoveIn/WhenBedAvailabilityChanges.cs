using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenBedAvailabilityChanges(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X69_0)]
    public async Task joining_after_the_bed_is_freed_keeps_it_available()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review allocation", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-hold")).ToHaveTextAsync("Bed held by James Reed until 09:45");
        await world.Seed(MoveInChanges.TheWindowBedIsReleased());
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 14, available");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Reason for joining", Exact = true }).FillAsync("A window bed near her family.");
        await world.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Accept waitlist terms", Exact = true }).CheckAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Join waitlist", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#waitlist-result")).ToHaveTextAsync("Helen Ward is number 1 on the waitlist");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 14, available");
        await Expect(world.Page.Locator("#waitlist-record")).ToHaveTextAsync("W100: Helen Ward, Harbor House, Assisted, position 1. A window bed near her family.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X90_0)]
    public async Task a_new_bed_hold_refreshes_the_bed_and_the_service_catalogue()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await Expect(world.Page.Locator("#availability-services")).ToHaveTextAsync("3 services, price revision 3");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X91_0)]
    public async Task older_and_unrelated_bed_messages_leave_the_displayed_revision_alone()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await Expect(world.Page.Locator("#availability-services")).ToHaveTextAsync("3 services, price revision 3");
        await world.Push(CareEndpoints.MoveInAvailabilityStream, "bedChanged", """{"communityId":"C1","bed":{"id":"BED4","apartment":{"id":"A7","number":"7","building":{"id":"B1","name":"Cedar","community":{"id":"C1","name":"Harbor House"}}},"revision":12,"available":true},"priceRevision":3}""");
        await world.Push(CareEndpoints.MoveInAvailabilityStream, "bedChanged", """{"communityId":"C2","bed":{"id":"BED4","apartment":{"id":"A21","number":"21","building":{"id":"B2","name":"East","community":{"id":"C2","name":"Maple House"}}},"revision":16,"available":true},"priceRevision":9}""");
        await world.Push(CareEndpoints.MoveInAvailabilityStream, "bedChanged", """{"communityId":"C1","bed":{"id":"BED5","apartment":{"id":"A7","number":"7","building":{"id":"B1","name":"Cedar","community":{"id":"C1","name":"Harbor House"}}},"revision":19,"available":true},"priceRevision":3}""");
        var dropped = await world.Drop(CareEndpoints.MoveInAvailabilityStream);
        await dropped.NextHeld();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability unconfirmed");
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        Assert.That(world.SentTo(CareEndpoints.MoveInAvailability), Is.EqualTo(2));
        Assert.That(world.SentTo(CareEndpoints.MoveInServices), Is.EqualTo(1));
        Assert.That(world.SentTo(CareEndpoints.MoveInCurrentOffer), Is.EqualTo(1));
        await dropped.Reopen();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability confirmed");
        await world.Seed(MoveInChanges.TheWindowBedIsReleased());
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 14, available");
        Assert.That(world.SentTo(CareEndpoints.MoveInServices), Is.EqualTo(2));
        Assert.That(world.SentTo(CareEndpoints.MoveInAvailability), Is.EqualTo(4));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 14, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X92_0)]
    public async Task lost_community_access_refuses_the_refresh_until_access_returns()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        var armed = await world.Arm(RealtimeGate.Answer, CareEndpoints.MoveInAvailability);
        await world.Seed(MoveInChanges.TheBedIsTaken());
        var answer = await armed.Held();
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(false));
        await answer.Release();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("This coordinator cannot perform this action.");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: This coordinator cannot perform this action.");
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(true));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X93_0)]
    public async Task failed_transport_leaves_availability_unconfirmed_until_the_next_read()
    {
        var world = await NewWorld();
        await world.InstallTheClock();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await world.HoldTheClock();
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInAvailability));
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await world.RunTheClockUntilItSends(CareEndpoints.MoveInAvailability, 5);
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability unconfirmed");
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Seed(MoveInChanges.TheAvailabilityConnectionIsRestored());
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability confirmed");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X94_0)]
    public async Task a_card_updates_while_the_page_and_push_requests_have_independent_holds()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open bed card", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#card-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInCardStream);
        var page = await world.Arm(RealtimeGate.Answer, CareEndpoints.MoveInAvailability);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh availability", Exact = true }).ClickAsync();
        var manual = await page.Held();
        var message = await world.Arm(RealtimeGate.Answer, CareEndpoints.MoveInAvailability);
        await world.Seed(MoveInChanges.TheBedIsTaken());
        var changed = await message.Held();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Confirming reported loss of availability");
        await Expect(world.Page.Locator("#card-bed")).ToHaveTextAsync("BED4 revision 13, unavailable");
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await Expect(world.Page.Locator("#availability-services")).ToHaveTextAsync("3 services, price revision 3");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await changed.Release();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await manual.Release();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
    }
}
