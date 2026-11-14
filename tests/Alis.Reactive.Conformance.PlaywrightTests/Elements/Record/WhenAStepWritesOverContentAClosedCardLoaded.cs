using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesOverContentAClosedCardLoaded(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X12_12_4)]
    public async Task the_step_stops_and_the_entry_keeps_running_until_the_slot_that_holds_it_unloads()
    {
        var left = await NewWorld();
        await left.Page.GotoAsync(CarePages.NightLog);
        await left.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await left.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the entry", Exact = true }).ClickAsync();
        await Expect(left.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night entry", Exact = true })).ToBeVisibleAsync();
        await left.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await Expect(left.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeHiddenAsync();
        await left.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark no entry", Exact = true }).ClickAsync();
        await Expect(left.Page.Locator("#log-status")).ToHaveTextAsync("Marking");
        await Expect(left.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night entry", Exact = true })).ToBeVisibleAsync();
        await left.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Acknowledge", Exact = true }).ClickAsync();
        await Expect(left.Page.Locator("#entry-acknowledged")).ToHaveTextAsync("Acknowledged");
        Assert.That(left.ConsoleErrors, Has.Count.EqualTo(1));

        var unloaded = await NewWorld();
        await unloaded.Page.GotoAsync(CarePages.NightLog);
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load the entry", Exact = true }).ClickAsync();
        await Expect(unloaded.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night entry", Exact = true })).ToBeVisibleAsync();
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the entry", Exact = true }).ClickAsync();
        await Expect(unloaded.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night entry", Exact = true })).ToBeHiddenAsync();
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await Expect(unloaded.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Night shift", Exact = true })).ToBeHiddenAsync();
        await unloaded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark no entry", Exact = true }).ClickAsync();
        await Expect(unloaded.Page.Locator("#log-entry")).ToHaveTextAsync("No entry tonight");
        await Expect(unloaded.Page.Locator("#log-status")).ToHaveTextAsync("Marked");
        Assert.That(unloaded.ConsoleErrors, Is.Empty);
    }
}
