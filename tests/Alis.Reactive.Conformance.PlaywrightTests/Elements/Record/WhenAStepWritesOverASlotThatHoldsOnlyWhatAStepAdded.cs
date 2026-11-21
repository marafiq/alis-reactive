using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesOverASlotThatHoldsOnlyWhatAStepAdded(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X5_1)]
    public async Task the_step_writes_once_the_desk_that_names_the_slot_has_closed_and_stops_while_it_is_open()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.TurnChart);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeHiddenAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log a turn", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#turn-slot")).ToHaveTextAsync("Turned at 02:00");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Start again", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#turn-slot")).ToHaveTextAsync("No turns yet");
        await Expect(closed.Page.Locator("#turn-status")).ToHaveTextAsync("Started");
        Assert.That(closed.ConsoleErrors, Is.Empty);

        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.TurnChart);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeVisibleAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Log a turn", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#turn-slot")).ToHaveTextAsync("Turned at 02:00");
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Start again", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#turn-status")).ToHaveTextAsync("Starting");
        await Expect(open.Page.Locator("#turn-slot")).ToHaveTextAsync("Turned at 02:00");
        Assert.That(open.ConsoleErrors, Has.Count.EqualTo(1));
    }
}
