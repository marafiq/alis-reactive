using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffReviewWardPlacements(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1817_0)]
    public async Task the_handover_records_the_source_and_previous_destination()
    {
        var review = await NewWorld();
        await review.Page.GotoAsync(CarePages.WardPlacementReview);
        await review.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer history", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(review.Page.Locator("#assignment-method")).ToHaveTextAsync("Staff chose the destination with the keyboard.");
        await Expect(review.Page.Locator("#assignment-now")).ToHaveTextAsync("Cedar");
        await Expect(review.Page.Locator("#assignment-before")).ToHaveTextAsync("None");
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Apply Maple referral", Exact = true }).ClickAsync();
        await Expect(review.Page.Locator("#assignment-method")).ToHaveTextAsync("Applied from the referral.");
        await Expect(review.Page.Locator("#assignment-now")).ToHaveTextAsync("Maple");
        await Expect(review.Page.Locator("#assignment-before")).ToHaveTextAsync("Cedar");
        await review.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove assignment", Exact = true }).ClickAsync();
        await Expect(review.Page.Locator("#assignment-method")).ToHaveTextAsync("No destination assigned.");
        await Expect(review.Page.Locator("#assignment-now")).ToHaveTextAsync("Unassigned");
        await Expect(review.Page.Locator("#assignment-before")).ToHaveTextAsync("Maple");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1818_0)]
    public async Task the_round_finds_a_destination_by_position_name_or_referral()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.WardPlacementReview);
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Select first stop", Exact = true }).ClickAsync();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review selected destination", Exact = true }).ClickAsync();
        await Expect(round.Page.Locator("#stop-label")).ToHaveTextAsync("Maple");
        await Expect(round.Page.Locator("#stop-position")).ToHaveTextAsync("First stop");
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Select last stop", Exact = true }).ClickAsync();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review selected destination", Exact = true }).ClickAsync();
        await Expect(round.Page.Locator("#stop-label")).ToHaveTextAsync("Cedar");
        await Expect(round.Page.Locator("#stop-position")).ToHaveTextAsync("Second stop");
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find Maple by referral", Exact = true }).ClickAsync();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review selected destination", Exact = true }).ClickAsync();
        await Expect(round.Page.Locator("#stop-label")).ToHaveTextAsync("Maple");
        await Expect(round.Page.Locator("#stop-position")).ToHaveTextAsync("First stop");
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find Cedar by name", Exact = true }).ClickAsync();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review selected destination", Exact = true }).ClickAsync();
        await Expect(round.Page.Locator("#stop-label")).ToHaveTextAsync("Cedar");
        await Expect(round.Page.Locator("#stop-position")).ToHaveTextAsync("Second stop");
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Leave destination open", Exact = true }).ClickAsync();
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review selected destination", Exact = true }).ClickAsync();
        await Expect(round.Page.Locator("#stop-label")).ToHaveTextAsync("No destination.");
        await Expect(round.Page.Locator("#stop-position")).ToHaveTextAsync("No stop chosen.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1819_0)]
    public async Task the_confirmed_destination_can_be_locked_and_released()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.WardPlacementReview);
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Block assignment changes", Exact = true }).ClickAsync();
        await Expect(held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).First).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(held.Page.Locator("#edit-policy")).ToHaveTextAsync("The assignment is blocked.");
        await Expect(held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Lock confirmed assignment", Exact = true }).ClickAsync();
        await held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await Expect(held.Page.Locator("#edit-policy")).ToHaveTextAsync("The confirmed destination is read-only.");
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release assignment", Exact = true }).ClickAsync();
        await held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(held.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Confirmed destination", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await Expect(held.Page.Locator("#edit-policy")).ToHaveTextAsync("Changes are available.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1820_0)]
    public async Task keyboard_guidance_follows_focus_into_and_out_of_the_ward_list()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardPlacementReview);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Start keyboard review", Exact = true }).ClickAsync();
        await Expect(desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Keyboard placement", Exact = true }).GetByRole(AriaRole.Combobox).First).ToBeFocusedAsync();
        await Expect(desk.Page.Locator("#keyboard-instruction")).ToHaveTextAsync("Use the arrow keys to choose a receiving ward.");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Keyboard placement", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Keyboard placement", Exact = true }).GetByRole(AriaRole.Combobox).First).Not.ToBeFocusedAsync();
        await Expect(desk.Page.Locator("#keyboard-instruction")).ToHaveTextAsync("Return to the transfer handover.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1821_0)]
    public async Task placement_templates_show_the_current_detail_level_and_selected_ward()
    {
        var detailed = await NewWorld();
        await detailed.Page.GotoAsync(CarePages.WardPlacementReview);
        await detailed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Detailed placement list", Exact = true }).ClickAsync();
        await detailed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement list", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(detailed.Page.GetByText("Clinical placement directory", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(detailed.Page.GetByText("Clinical floor: North wing", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(detailed.Page.GetByText("Contact the transfer desk before moving.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(detailed.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple · North wing", "Cedar · South wing" });
        await detailed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement list", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await detailed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement list", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(detailed.Page.GetByText("Transfer to Cedar", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var concise = await NewWorld();
        await concise.Page.GotoAsync(CarePages.WardPlacementReview);
        await concise.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Concise placement list", Exact = true }).ClickAsync();
        await concise.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement list", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(concise.Page.GetByText("Destinations", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(concise.Page.GetByText("North wing", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(concise.Page.GetByText("Choose a destination.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(concise.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await concise.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement list", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await concise.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement list", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(concise.Page.GetByText("Cedar ward", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1822_0)]
    public async Task the_printed_transfer_instructions_follow_the_active_placement_layout()
    {
        var urgent = await NewWorld();
        await urgent.Page.GotoAsync(CarePages.WardPlacementReview);
        await urgent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Urgent placement layout", Exact = true }).ClickAsync();
        await Expect(urgent.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement instructions", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveAttributeAsync("placeholder", "Choose an urgent destination");
        await urgent.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print placement instructions", Exact = true }).ClickAsync();
        await Expect(urgent.Page.Locator("#printed-hint")).ToHaveTextAsync("Choose an urgent destination");
        await Expect(urgent.Page.Locator("#placement-contact")).ToHaveTextAsync("Call the urgent placement desk.");

        var routine = await NewWorld();
        await routine.Page.GotoAsync(CarePages.WardPlacementReview);
        await routine.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Routine placement layout", Exact = true }).ClickAsync();
        await Expect(routine.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement instructions", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveAttributeAsync("placeholder", "Choose a routine destination");
        await routine.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Print placement instructions", Exact = true }).ClickAsync();
        await Expect(routine.Page.Locator("#printed-hint")).ToHaveTextAsync("Choose a routine destination");
        await Expect(routine.Page.Locator("#placement-contact")).ToHaveTextAsync("Contact the routine placement desk.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1823_0)]
    public async Task a_closed_preview_returns_the_underlying_field_and_an_open_preview_keeps_its_choices()
    {
        var closed = await NewWorld();
        await closed.Page.GotoAsync(CarePages.WardPlacementReview);
        await closed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Close placement preview", Exact = true }).ClickAsync();
        await Expect(closed.Page.Locator("#preview-notice")).ToHaveTextAsync("The temporary placement preview has closed.");
        await Expect(closed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement preview", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("");

        var kept = await NewWorld();
        await kept.Page.GotoAsync(CarePages.WardPlacementReview);
        await kept.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Keep preview open", Exact = true }).ClickAsync();
        await kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement preview", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(kept.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement preview", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Escape");
        await kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement preview", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(kept.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Placement preview", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await Expect(kept.Page.Locator("#preview-notice")).ToHaveTextAsync("The placement preview remains available.");
    }
}
