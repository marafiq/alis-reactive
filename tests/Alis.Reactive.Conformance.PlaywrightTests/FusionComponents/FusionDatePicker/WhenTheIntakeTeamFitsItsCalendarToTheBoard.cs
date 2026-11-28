using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheIntakeTeamFitsItsCalendarToTheBoard(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1762_0)]
    public async Task the_register_can_fit_its_calendar_to_only_one_screen_edge()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPaperMargin);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 0px; top: -24px; width: 320px; height: 300px; position: fixed;");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Margin advice", Exact = true })).ToHaveTextAsync("Leave the top margin for the transfer heading.");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalCrossMargin);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: -36px; top: 0px; width: 320px; height: 300px; position: fixed;");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Margin advice", Exact = true })).ToHaveTextAsync("Leave the side margin for the registration marks.");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1763_0)]
    public async Task the_last_bedside_booking_turns_back_into_view_while_the_printed_sheet_keeps_its_column()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalTurnedCalendar);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 592px; top: 192px; width: 320px; height: 300px; position: fixed;");
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Desk instruction", Exact = true })).ToHaveTextAsync("Use the gap beside the last bedside record.");
        await resident.Page.GetByRole(AriaRole.Grid).PressAsync("ArrowRight");
        await resident.Page.GetByRole(AriaRole.Grid).PressAsync("ArrowRight");
        await resident.Page.GetByRole(AriaRole.Grid).PressAsync("Enter");
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPrintedCalendar);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 800px; top: 236px; width: 320px; height: 300px; position: fixed;");
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Desk instruction", Exact = true })).ToHaveTextAsync("Align with the printed date box.");
        await referral.Page.GetByRole(AriaRole.Grid).PressAsync("ArrowRight");
        await referral.Page.GetByRole(AriaRole.Grid).PressAsync("ArrowRight");
        await referral.Page.GetByRole(AriaRole.Grid).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1764_0)]
    public async Task moving_the_booking_column_keeps_a_fitted_or_uncropped_review_at_its_field()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFittedReview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 48px; top: 90px; width: 320px; height: 300px;");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare booking column", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 80px; top: 122px; width: 320px; height: 300px;");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Retain calendar review", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 0px; top: 0px; width: 320px; height: 300px; position: fixed;");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalRetainedMargins);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 48px; top: 90px; width: 320px; height: 300px;");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare booking column", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 80px; top: 122px; width: 320px; height: 300px;");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Retain calendar review", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: -36px; top: -6px; width: 320px; height: 300px; position: fixed;");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1765_0)]
    public async Task a_supervisor_can_refit_the_open_calendar_or_leave_the_print_margins_intact()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFullReview);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: -36px; top: -24px; width: 320px; height: 300px; position: fixed;");
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Retain calendar review", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: 0px; top: 0px; width: 320px; height: 300px; position: fixed;");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalPrintReview);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: -36px; top: -24px; width: 320px; height: 300px; position: fixed;");
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Retain calendar review", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2950; left: -36px; top: -24px; width: 320px; height: 300px; position: fixed;");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1766_0)]
    public async Task the_review_register_pauses_and_resumes_following_the_moving_booking()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalPausedTracking);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 92px; position: fixed;");
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next arrival", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByText("Next arrival at the west reception desk", new PageGetByTextOptions { Exact = true })).ToBeFocusedAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 52px; position: fixed;");
        await Expect(resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to current admission", Exact = true })).ToBeVisibleAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Retain calendar review", Exact = true }).PressAsync("Enter");
        await resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to current admission", Exact = true }).PressAsync("Enter");
        await Expect(resident.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Current admission", Exact = true })).ToBeFocusedAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next arrival", Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 52px; position: fixed;");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalResumedTracking);
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open appointment board", Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 92px; position: fixed;");
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next arrival", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByText("Next arrival at the west reception desk", new PageGetByTextOptions { Exact = true })).ToBeFocusedAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 52px; position: fixed;");
        await Expect(referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to current admission", Exact = true })).ToBeVisibleAsync();
        await referral.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Retain calendar review", Exact = true }).PressAsync("Enter");
        await referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Return to current admission", Exact = true }).PressAsync("Enter");
        await Expect(referral.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions { Name = "Current admission", Exact = true })).ToBeFocusedAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Read next arrival", Exact = true })).ToBeVisibleAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveAttributeAsync("style", "z-index: 2600; left: 139px; top: 92px; position: fixed;");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
    }
}
