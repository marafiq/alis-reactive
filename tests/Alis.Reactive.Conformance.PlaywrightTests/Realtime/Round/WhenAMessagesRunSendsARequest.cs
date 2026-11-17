using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Realtime;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMessagesRunSendsARequest(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M10_F1.X21_8_0)]
    public async Task the_reconnection_shows_no_busy_indicator_and_the_messages_request_shows_it_until_it_settles()
    {
        var dropped = await NewWorld();
        await dropped.Page.GotoAsync(CarePages.LiveDoses);
        await dropped.Opened(CareEndpoints.RoundNews);
        var drop = await dropped.Drop(CareEndpoints.RoundNews);
        await Expect(dropped.Page.Locator("#round-dropped")).ToHaveTextAsync("dropped");
        await drop.NextHeld();
        await Expect(dropped.Page.Locator("#round-back")).ToBeEmptyAsync();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await drop.Reopen();
        await Expect(dropped.Page.Locator("#round-back")).ToHaveTextAsync("back");
        await dropped.Opened(CareEndpoints.RoundNews);
        var armed = await dropped.Arm(RealtimeGate.Answer, CareEndpoints.LiveDoses);
        await dropped.Push(CareEndpoints.RoundNews, "dose", "{\"name\":\"Aspirin\"}");
        var held = await armed.Held();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await held.Release();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show the page; hide the page");
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LiveDoses);
        await open.Opened(CareEndpoints.RoundNews);
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: none");
    }

    [Test]
    [FailsFor(Promises.M10_F1.X21_8_1)]
    public async Task a_request_sent_while_the_stream_reconnects_shows_the_indicator_until_its_answer_and_no_longer()
    {
        var dropped = await NewWorld();
        await dropped.Page.GotoAsync(CarePages.LiveFeed);
        await dropped.Opened(CareEndpoints.Feed);
        var drop = await dropped.Drop(CareEndpoints.Feed);
        await Expect(dropped.Page.Locator("#feed-dropped")).ToHaveTextAsync("dropped");
        await drop.NextHeld();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        var armed = await dropped.Arm(RealtimeGate.Answer, CareEndpoints.LiveItems);
        await dropped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var held = await armed.Held();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await held.Release();
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show the page; hide the page");
        await Expect(dropped.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");
        await Expect(dropped.Page.Locator("#feed-back")).ToBeEmptyAsync();
        await drop.Reopen();
        await Expect(dropped.Page.Locator("#feed-back")).ToHaveTextAsync("back");

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.LiveFeed);
        await open.Opened(CareEndpoints.Feed);
        var armedOpen = await open.Arm(RealtimeGate.Answer, CareEndpoints.LiveItems);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        var heldOpen = await armedOpen.Held();
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await heldOpen.Release();
        await Expect(open.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show the page; hide the page");
    }

    [Test]
    [FailsFor(Promises.M10_F1.X21_8_2)]
    public async Task the_indicator_stays_shown_while_the_requests_finally_awaits()
    {
        var saved = await NewWorld();
        await saved.Page.GotoAsync(CarePages.LiveFeedItems);
        await saved.Opened(CareEndpoints.Feed);
        var answer = await saved.Arm(RealtimeGate.Answer, CareEndpoints.LiveItems);
        var settle = await saved.Arm(RealtimeGate.DefaultFinally);
        await saved.Push(CareEndpoints.Feed, "item", "{\"name\":\"Gauze\"}");
        var heldAnswer = await answer.Held();
        await Expect(saved.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await heldAnswer.Release();
        var heldSettle = await settle.Held();
        await Expect(saved.Page.Locator("#item-status")).ToHaveTextAsync("saved");
        await Expect(saved.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await heldSettle.Release();
        await Expect(saved.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");

        var quiet = await NewWorld();
        await quiet.Page.GotoAsync(CarePages.LiveFeedItems);
        await quiet.Opened(CareEndpoints.Feed);
        await Expect(quiet.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: none");
        await Expect(quiet.Page.Locator("#item-status")).ToBeEmptyAsync();
    }

    [Test]
    [FailsFor(Promises.M10_F1.X21_8_3)]
    public async Task the_messages_request_and_the_pages_are_counted_together()
    {
        var both = await NewWorld();
        await both.Page.GotoAsync(CarePages.LiveFeedItems);
        await both.Opened(CareEndpoints.Feed);
        var armed = await both.Arm(RealtimeGate.Answer, CareEndpoints.LiveItems);
        await both.Push(CareEndpoints.Feed, "item", "{\"name\":\"Gauze\"}");
        var held = await armed.Held();
        await Expect(both.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await both.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(both.Page.Locator("#list-status")).ToHaveTextAsync("loaded");
        await Expect(both.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: the page");
        await held.Release();
        await Expect(both.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show the page; hide the page");
        await Expect(both.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy", Exact = true })).ToHaveTextAsync("Busy: none");

        var load = await NewWorld();
        await load.Page.GotoAsync(CarePages.LiveFeedItems);
        await load.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(load.Page.Locator("#list-status")).ToHaveTextAsync("loaded");
        await Expect(load.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Busy calls", Exact = true })).ToHaveTextAsync("Busy calls: show the page; hide the page");
    }
}
