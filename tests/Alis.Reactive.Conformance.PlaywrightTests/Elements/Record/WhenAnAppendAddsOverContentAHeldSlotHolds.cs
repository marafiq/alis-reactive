using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnAppendAddsOverContentAHeldSlotHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X5_0)]
    public async Task the_append_adds_after_the_entry_and_keeps_it_where_a_set_over_the_same_slot_stops()
    {
        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.HourLog);
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the entry", Exact = true }).ClickAsync();
        await Expect(noted.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Hourly entry", Exact = true })).ToBeVisibleAsync();
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Note the hour", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#hour-status")).ToHaveTextAsync("Noted");
        await Expect(noted.Page.Locator("#hour-entry")).ToHaveTextAsync("Hourly entry Turned at midnight. Acknowledge Entry: Not acknowledged Noted at 02:00");
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Acknowledge", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#hour-acknowledged")).ToHaveTextAsync("Acknowledged");
        Assert.That(noted.ConsoleErrors, Is.Empty);

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.HourLog);
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the entry", Exact = true }).ClickAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Hourly entry", Exact = true })).ToBeVisibleAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Clear the hour", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#hour-status")).ToHaveTextAsync("Clearing");
        await Expect(cleared.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Hourly entry", Exact = true })).ToBeVisibleAsync();
        await cleared.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Acknowledge", Exact = true }).ClickAsync();
        await Expect(cleared.Page.Locator("#hour-acknowledged")).ToHaveTextAsync("Acknowledged");
        Assert.That(cleared.ConsoleErrors, Has.Count.EqualTo(1));
    }
}
