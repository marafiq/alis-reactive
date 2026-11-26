using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenARowSavesItsOwnContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_51_0)]
    public async Task the_row_is_sent_under_the_names_past_its_prefix_and_checked_alone()
    {
        var saved = await NewWorld();
        await saved.Page.GotoAsync(CarePages.ShiftRoster);
        await saved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).FillAsync("Ben");
        await saved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(saved.Page.Locator("#roster-status")).ToHaveTextAsync("Shift 6 saved");
        await Expect(saved.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 7", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        await saved.Page.GotoAsync(CarePages.ShiftRoster);
        await Expect(saved.Page.Locator("#saved-shifts")).ToHaveTextAsync("Shift 6: Ben, 8 hours, from Carer, Hours, Id");

        var cleared = await NewWorld();
        await cleared.Page.GotoAsync(CarePages.ShiftRoster);
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).FillAsync("");
        await cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Carer", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Choose a carer." });
        await Expect(cleared.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 7", Exact = true }).GetByText("Choose a carer.", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        Assert.That(cleared.SentTo(CareEndpoints.ShiftRosterShifts), Is.EqualTo(0));
    }
}
