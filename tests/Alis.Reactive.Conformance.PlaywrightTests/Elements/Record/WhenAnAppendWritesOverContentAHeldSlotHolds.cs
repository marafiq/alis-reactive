using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAppendWritesOverContentAHeldSlotHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X12_12_5)]
    public async Task the_append_stops_as_a_set_does_and_adds_its_text_once_the_slot_is_held_no_more()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.HourLog);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the entry", Exact = true }).ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Hourly entry", Exact = true })).ToBeVisibleAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Note the hour", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#hour-status")).ToHaveTextAsync("Noting");
        await Expect(held.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Hourly entry", Exact = true })).ToBeVisibleAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Acknowledge", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#hour-acknowledged")).ToHaveTextAsync("Acknowledged");
        Assert.That(held.ConsoleErrors, Has.Count.EqualTo(1));

        var free = await NewWorld();
        await free.Page.GotoAsync(CarePages.HourLog);
        await free.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await Expect(free.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeVisibleAsync();
        await free.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await Expect(free.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeHiddenAsync();
        await free.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Note the hour", Exact = true }).ClickAsync();
        await Expect(free.Page.Locator("#hour-entry")).ToHaveTextAsync("Noted at 02:00");
        await Expect(free.Page.Locator("#hour-status")).ToHaveTextAsync("Noted");
        Assert.That(free.ConsoleErrors, Is.Empty);
    }
}
