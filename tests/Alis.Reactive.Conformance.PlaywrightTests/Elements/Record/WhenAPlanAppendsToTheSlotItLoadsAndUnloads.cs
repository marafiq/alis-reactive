using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPlanAppendsToTheSlotItLoadsAndUnloads(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X5_2)]
    public async Task the_page_renders_and_the_unload_empties_the_slot_of_what_the_append_added()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.MealCard);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the menu", Exact = true }).ClickAsync();
        await Expect(closed.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Today's menu", Exact = true })).ToBeVisibleAsync();
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a note", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#meal-status")).ToHaveTextAsync("Note added");
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the menu", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#meal-status")).ToHaveTextAsync("Menu closed");
        await Expect(closed.Page.Locator("#menu-slot")).ToBeEmptyAsync();
        Assert.That(closed.ConsoleErrors, Is.Empty);

        var noted = await NewWorld();
        await noted.Page.GotoAsync(CarePages.MealCard);
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open the menu", Exact = true }).ClickAsync();
        await Expect(noted.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Today's menu", Exact = true })).ToBeVisibleAsync();
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close the menu", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#meal-status")).ToHaveTextAsync("Menu closed");
        await noted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a note", Exact = true }).ClickAsync();
        await Expect(noted.Page.Locator("#menu-slot")).ToHaveTextAsync("Soft diet today");
        await Expect(noted.Page.Locator("#meal-status")).ToHaveTextAsync("Note added");
    }
}
