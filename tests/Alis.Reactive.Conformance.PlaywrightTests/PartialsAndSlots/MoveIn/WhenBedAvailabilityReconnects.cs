using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenBedAvailabilityReconnects(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X95_0)]
    public async Task reconnecting_confirms_the_bed_only_after_its_new_snapshot_arrives()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        var dropped = await world.Drop(CareEndpoints.MoveInAvailabilityStream);
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability unconfirmed");
        await dropped.NextHeld();
        await Expect(world.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        var armed = await world.Arm(RealtimeGate.Answer, CareEndpoints.MoveInAvailability);
        await dropped.Reopen();
        var answer = await armed.Held();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability unconfirmed");
        await answer.Release();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability confirmed");
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X96_0)]
    public async Task refreshing_a_deliberately_closed_stream_asks_to_open_it()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh availability", Exact = true }).ClickAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInAvailability), Is.EqualTo(2));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability closed");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Open availability before refreshing.");
        Assert.That(world.SentTo(CareEndpoints.MoveInAvailability), Is.EqualTo(2));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X97_0)]
    public async Task a_reconnect_without_community_access_keeps_the_offer()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Opened(CareEndpoints.MoveInAvailabilityStream);
        var dropped = await world.Drop(CareEndpoints.MoveInAvailabilityStream);
        await dropped.NextHeld();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability unconfirmed");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(false));
        await dropped.Reopen();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability unavailable");
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeVisibleAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(true));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability confirmed");
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X98_0)]
    public async Task the_reconnect_offer_stays_until_both_held_connections_recover()
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
        await Expect(world.Page.Locator("#card-connection")).ToHaveTextAsync("Bed card connected");
        await world.Opened(CareEndpoints.MoveInCardStream);
        var page = await world.Refuse(CareEndpoints.MoveInAvailabilityStream);
        var nested = await world.Refuse(CareEndpoints.MoveInCardStream);
        await page.Refused(1);
        await nested.Refused(1);
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeVisibleAsync();
        await page.Reopen();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability confirmed");
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeVisibleAsync();
        await nested.Refused(2);
        await nested.Reopen();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reconnect", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#card-connection")).ToHaveTextAsync("Bed card connected");
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 12, available");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X99_0)]
    public async Task closing_the_card_keeps_page_updates_and_closing_the_final_owner_clears_the_offer()
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
        await Expect(world.Page.Locator("#card-connection")).ToHaveTextAsync("Bed card connected");
        await world.Opened(CareEndpoints.MoveInCardStream);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close bed card", Exact = true }).ClickAsync();
        await world.Closed(CareEndpoints.MoveInCardStream);
        await world.Seed(MoveInChanges.TheBedIsTaken());
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
        var failed = await world.Refuse(CareEndpoints.MoveInAvailabilityStream);
        await failed.Refused(1);
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeVisibleAsync();
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-status")).ToHaveTextAsync("Availability closed");
        await Expect(world.Page.Locator("#alis-reconnect")).ToBeHiddenAsync();
        await failed.Reopen();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open availability", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#availability-bed")).ToHaveTextAsync("BED4 revision 13, held by James Reed");
    }
}
