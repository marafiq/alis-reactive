using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenAnInputIsSentUnderAnotherPath(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_55_0)]
    public async Task the_input_is_checked_as_the_member_it_is_sent_as()
    {
        var letters = await NewWorld();
        await letters.Page.GotoAsync(CarePages.ShiftSheet);
        await letters.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("12a");
        await letters.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(letters.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: The value '12a' is not valid for Hours.");
        await Expect(letters.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).GetByText("The value '12a' is not valid for Hours.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        Assert.That(letters.SentTo(CareEndpoints.ShiftSheetShifts), Is.EqualTo(0));

        var tooMany = await NewWorld();
        await tooMany.Page.GotoAsync(CarePages.ShiftSheet);
        await tooMany.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("20");
        await tooMany.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(tooMany.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Notices", Exact = true })).ToHaveTextAsync("Notices: error: A shift is 1 to 12 hours.");
        await Expect(tooMany.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Group, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).GetByText("A shift is 1 to 12 hours.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var eight = await NewWorld();
        await eight.Page.GotoAsync(CarePages.ShiftSheet);
        await eight.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Hours", Exact = true }).FillAsync("8");
        await eight.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Shift 6", Exact = true }).GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(eight.Page.Locator("#sheet-status")).ToHaveTextAsync("Shift 6 saved");
    }
}
