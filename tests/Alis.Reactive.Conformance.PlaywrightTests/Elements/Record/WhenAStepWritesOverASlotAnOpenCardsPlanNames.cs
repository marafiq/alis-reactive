using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepWritesOverASlotAnOpenCardsPlanNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.X12_12_3)]
    public async Task the_step_stops_while_the_card_that_names_the_slot_is_open_and_writes_once_it_closed()
    {
        var open = await NewWorld();
        await open.Page.GotoAsync(CarePages.VisitDesk);
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await Expect(open.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Visits today", Exact = true })).ToBeVisibleAsync();
        await open.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark no visit", Exact = true }).ClickAsync();
        await Expect(open.Page.Locator("#visit-status")).ToHaveTextAsync("Marking");
        await Expect(open.Page.Locator("#visit-note")).ToBeEmptyAsync();
        Assert.That(open.ConsoleErrors, Has.Count.EqualTo(1));

        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.VisitDesk);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the desk", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Visits today", Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the desk", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Visits today", Exact = true })).ToBeHiddenAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark no visit", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#visit-note")).ToHaveTextAsync("No visit today");
        await Expect(closed.Page.Locator("#visit-status")).ToHaveTextAsync("Marked");
        Assert.That(closed.ConsoleErrors, Is.Empty);
    }
}
