using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenCoordinatorsUseTheMoveInLine(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X245_0)]
    public async Task a_post_a_ward_update_and_reconnection_preserve_only_this_worlds_arrival()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
        var other = await NewWorld();
        await other.Page.GotoAsync(CarePages.MoveIn);
        await other.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(other.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Line message", Exact = true }).FillAsync("Helen arrives at 10:00");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send message", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-post-receipt")).ToHaveTextAsync("MSG1 · Helen arrives at 10:00");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 1");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("MSG1 · Helen arrives at 10:00");
        var preparation = await world.Page.RunAndWaitForRequestAsync(
            () => world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync(), "**/D100/ward-preparation");
        Assert.That(preparation.Method, Is.EqualTo("POST"));
        Assert.That((await preparation.HeaderValueAsync("Content-Type"))!.Split(';').First(), Is.EqualTo("multipart/form-data"));
        Assert.That(await preparation.HeaderValueAsync("X-Coordinator"), Is.EqualTo("CO1"));
        Assert.That(await preparation.HeaderValueAsync("X-Operation"), Is.EqualTo("11111111-1111-4111-8111-111111111111"));
        await Expect(world.Page.Locator("#ward-instruction")).ToHaveTextAsync("Water requested for apartment 7, Window bed");
        await Expect(other.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(other.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
        await world.Opened(CareEndpoints.MoveInCoordinatorLine);
        var dropped = await world.Drop(CareEndpoints.MoveInCoordinatorLine);
        await dropped.NextHeld();
        await Expect(world.Page.Locator("#line-connection")).ToHaveTextAsync("Reconnecting");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("Waiting for the current messages.");
        await dropped.Reopen();
        await Expect(world.Page.Locator("#line-connection")).ToHaveTextAsync("Reconnected on a new connection");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("MSG1 · Helen arrives at 10:00");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 1");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("MSG1 · Helen arrives at 10:00");
        await Expect(world.Page.Locator("#saved-ward-preparation")).ToHaveTextAsync("Window bed: water requested");
        await Expect(world.Page.Locator("#saved-version")).ToHaveTextAsync("7");
        await other.Page.GotoAsync(CarePages.MoveIn);
        await other.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(other.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(other.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(other.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
        await Expect(other.Page.Locator("#saved-ward-preparation")).ToHaveTextAsync("Window bed: no water requested");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X246_0)]
    public async Task short_and_forbidden_messages_leave_the_next_snapshot_empty()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Line message", Exact = true }).FillAsync("Hi");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send message", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-post-receipt")).ToHaveTextAsync("Enter at least three characters.");
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Line message", Exact = true }).FillAsync("spam");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send message", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-post-receipt")).ToHaveTextAsync("Choose a message other than test or spam.");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X247_0)]
    public async Task revoking_membership_refuses_the_post_on_the_already_joined_connection()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(false));
        var posting = await world.Arm(RunGate.Settle, CareEndpoints.MoveInCoordinatorLine);
        await world.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Line message", Exact = true }).FillAsync("Helen arrives at 10:00");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Send message", Exact = true }).ClickAsync();
        var decided = await posting.Held();
        await Expect(world.Page.Locator("#line-post-receipt")).ToHaveTextAsync("Awaiting confirmation");
        await world.Seed(MoveInChanges.TheCoordinatorServesHarbor(true));
        await decided.Release();
        await Expect(world.Page.Locator("#line-post-receipt")).ToHaveTextAsync("Membership revoked.");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Refresh line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-refresh")).ToHaveTextAsync("Snapshot refreshed");
        await Expect(world.Page.Locator("#line-post-receipt")).ToHaveTextAsync("Membership revoked.");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
        Assert.That(world.ConsoleErrors, Is.EqualTo(new[] { "An awaited step rejected, so its pipeline stops: The hub '/care/move-ins/coordinator-line' did not run Post: An unexpected error occurred invoking 'Post' on the server. HubException: Membership revoked." }));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X248_0)]
    public async Task a_failed_ward_request_leaves_the_window_bed_unprepared()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await world.Seed(MoveInChanges.TheEndpointDropsItsConnection(CareEndpoints.MoveInWardPreparation));
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#ward-instruction")).ToHaveTextAsync("Ward preparation unavailable.");
        Assert.That(world.SentTo(CareEndpoints.MoveInWardPreparation), Is.EqualTo(1));
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#saved-ward-preparation")).ToHaveTextAsync("Window bed: no water requested");
    }

    [Test]
    [FailsFor(Promises.M9_F2.M9_X249_0)]
    public async Task closing_the_line_releases_its_hub_while_the_quote_finishes()
    {
        var world = await NewWorld();
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Building", Exact = true }).SelectOptionAsync("B1");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Apartment", Exact = true }).SelectOptionAsync("A7");
        await world.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Bed", Exact = true }).SelectOptionAsync("BED4");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#ward-preparation-store").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Subscribers", Exact = true })).ToHaveTextAsync("subscribers: 1");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#ward-instruction")).ToHaveTextAsync("Water requested for apartment 7, Window bed");
        await world.Opened(CareEndpoints.MoveInCoordinatorLine);
        var quote = await world.Arm(CompositionGate.Answer, CareEndpoints.MoveInQuote);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Calculate quote", Exact = true }).ClickAsync();
        var pricing = await quote.Held();
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Calculating");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close coordinators' line", Exact = true }).ClickAsync();
        await world.Closed(CareEndpoints.MoveInCoordinatorLine);
        await Expect(world.Page.Locator("#ward-preparation-store").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Subscribers", Exact = true })).ToHaveTextAsync("subscribers: 0");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Post an update", Exact = true }).ClickAsync();
        Assert.That(world.SentTo(CareEndpoints.MoveInWardPreparation), Is.EqualTo(1));
        await pricing.Release();
        await Expect(world.Page.Locator("#quote-status")).ToHaveTextAsync("Quote ready: 3040.00 USD");
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#ward-preparation-store").GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Subscribers", Exact = true })).ToHaveTextAsync("subscribers: 1");
        await world.Page.GotoAsync(CarePages.MoveIn);
        await world.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open coordinators' line", Exact = true }).ClickAsync();
        await Expect(world.Page.Locator("#line-coordinators")).ToHaveTextAsync("Maya Chen, Luis Ortiz");
        await Expect(world.Page.Locator("#quote-on-file")).ToHaveTextAsync("Q100 revision 3: 3040.00 USD, assistance 2432.00 USD. 1 calculation on file.");
        await Expect(world.Page.Locator("#line-sequence")).ToHaveTextAsync("Sequence 0");
        await Expect(world.Page.Locator("#line-messages")).ToHaveTextAsync("No messages.");
    }
}
