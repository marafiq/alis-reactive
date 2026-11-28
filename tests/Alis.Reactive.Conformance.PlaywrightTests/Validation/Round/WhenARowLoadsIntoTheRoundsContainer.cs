using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowLoadsIntoTheRoundsContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X13_3_0)]
    public async Task the_item_rule_checks_the_row_the_slot_loaded_and_nothing_is_sent()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.RoundDosing);
        await empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a dose", Exact = true }).ClickAsync();
        await empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Third dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("0");
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save round", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Third dose", Exact = true })
            .GetByText("Give an amount.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(empty.Page.Locator("#saved")).ToBeEmptyAsync();

        var given = await NewWorld();
        await given.Page.GotoAsync(CarePages.RoundDosing);
        await given.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await given.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await given.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add a dose", Exact = true }).ClickAsync();
        await given.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Third dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await given.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save round", Exact = true }).ClickAsync();
        await Expect(given.Page.Locator("#saved")).ToHaveTextAsync("3");
    }
}
