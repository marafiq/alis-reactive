using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAResidentTransfersBetweenWards(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F20.M21_X1800_0)]
    public async Task the_transfer_retains_the_previous_ward_when_the_destination_is_cleared()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.Locator("#arrival-instruction")).ToHaveTextAsync("Report to the Cedar admission desk.");
        await Expect(desk.Page.Locator("#previous-ward")).ToHaveTextAsync("None");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(desk.Page.Locator("#arrival-instruction")).ToHaveTextAsync("Report to the Maple charge nurse.");
        await Expect(desk.Page.Locator("#previous-ward")).ToHaveTextAsync("Cedar");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove destination", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#arrival-instruction")).ToHaveTextAsync("Choose a destination.");
        await Expect(desk.Page.Locator("#previous-ward")).ToHaveTextAsync("Maple");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1801_0)]
    public async Task the_transfer_route_assigns_the_right_team_and_saves_the_route()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Travel route", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(desk.Page.Locator("#route-action")).ToHaveTextAsync("Send the lift team.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#handover-receipt")).ToHaveTextAsync("Lift team / Notice pending / Escort pending");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Travel route", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.Locator("#route-action")).ToHaveTextAsync("Arrange the outdoor escort.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#handover-receipt")).ToHaveTextAsync("Outdoor escort / Notice pending / Escort pending");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1802_0)]
    public async Task zero_minutes_and_thirty_minutes_have_distinct_transfer_instructions()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Advance notice", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(desk.Page.Locator("#notice-action")).ToHaveTextAsync("Meet the resident now.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#handover-receipt")).ToHaveTextAsync("Route pending / 0 minutes / Escort pending");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Advance notice", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.Locator("#notice-action")).ToHaveTextAsync("Prepare the room in thirty minutes.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#handover-receipt")).ToHaveTextAsync("Route pending / 30 minutes / Escort pending");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1803_0)]
    public async Task both_escort_answers_reach_the_handover_receipt()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bedside escort", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(desk.Page.Locator("#escort-action")).ToHaveTextAsync("Keep the escort team available.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#handover-receipt")).ToHaveTextAsync("Route pending / Notice pending / Independent travel");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bedside escort", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.Locator("#escort-action")).ToHaveTextAsync("Reserve the bedside escort.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save handover", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#handover-receipt")).ToHaveTextAsync("Route pending / Notice pending / Escort reserved");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1804_0)]
    public async Task the_chosen_receiving_team_supplies_its_own_handover_contact()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Receiving team", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await Expect(desk.Page.Locator("#receiving-team")).ToHaveTextAsync("Night lead at extension 214.");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Receiving team", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.Locator("#receiving-team")).ToHaveTextAsync("Day lead at extension 319.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1805_0)]
    public async Task incoming_referrals_reach_the_correct_ward_or_return_to_placement_review()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer destination", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("Home");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check referral M1", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#referral-contact")).ToHaveTextAsync("Report to the Maple charge nurse.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove destination", Exact = true }).ClickAsync();
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check referral C2", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#referral-contact")).ToHaveTextAsync("Report to the Cedar admission desk.");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check referral R9", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#referral-contact")).ToHaveTextAsync("Return this referral to placement review.");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1806_0)]
    public async Task released_wards_enter_the_round_at_the_requested_position()
    {
        var release = await NewWorld();
        await release.Page.GotoAsync(CarePages.WardTransferDesk);
        await release.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release both wards", Exact = true }).ClickAsync();
        await release.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Overflow wards", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(release.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar", "Oak", "Willow" });

        var priority = await NewWorld();
        await priority.Page.GotoAsync(CarePages.WardTransferDesk);
        await priority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize both wards", Exact = true }).ClickAsync();
        await priority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Overflow wards", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(priority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Oak", "Willow", "Cedar" });

        var single = await NewWorld();
        await single.Page.GotoAsync(CarePages.WardTransferDesk);
        await single.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Release Oak", Exact = true }).ClickAsync();
        await single.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Overflow wards", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(single.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar", "Oak" });

        var singlePriority = await NewWorld();
        await singlePriority.Page.GotoAsync(CarePages.WardTransferDesk);
        await singlePriority.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prioritize Oak", Exact = true }).ClickAsync();
        await singlePriority.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Overflow wards", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(singlePriority.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Oak", "Cedar" });
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1807_0)]
    public async Task ward_directory_searches_offer_only_the_requested_service()
    {
        var secure = await NewWorld();
        await secure.Page.GotoAsync(CarePages.WardTransferDesk);
        await secure.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(secure.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await secure.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("s");
        await Expect(secure.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Oak", "Willow" });

        var assisted = await NewWorld();
        await assisted.Page.GotoAsync(CarePages.WardTransferDesk);
        await assisted.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(assisted.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await assisted.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("a");
        await Expect(assisted.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar" });

        var unmatched = await NewWorld();
        await unmatched.Page.GotoAsync(CarePages.WardTransferDesk);
        await unmatched.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward directory", Exact = true }).GetByRole(AriaRole.Combobox).First.ClickAsync();
        await Expect(unmatched.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await unmatched.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox).PressSequentiallyAsync("x");
        await Expect(unmatched.Page.GetByText("No wards provide this service.", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1808_0)]
    public async Task an_isolation_hold_keeps_the_existing_bed_assignment()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed assignment", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("c");
        await Expect(desk.Page.Locator("#bed-review")).ToHaveTextAsync("A nurse must release the isolation hold.");
        await Expect(desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed assignment", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Maple");
        await desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed assignment", Exact = true }).GetByRole(AriaRole.Combobox).First.PressAsync("End");
        await Expect(desk.Page.Locator("#bed-review")).ToHaveTextAsync("Willow is ready for transfer.");
        await Expect(desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Bed assignment", Exact = true }).GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Willow");
    }

    [Test]
    [FailsFor(Promises.M21_F20.M21_X1809_0)]
    public async Task placement_review_observes_the_opening_hold_and_confirmation()
    {
        var held = await NewWorld();
        await held.Page.GotoAsync(CarePages.WardTransferDesk);
        await held.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Hold placement review", Exact = true }).CheckAsync();
        await held.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review placement choices", Exact = true }).ClickAsync();
        await Expect(held.Page.Locator("#confirmation-instruction")).ToHaveTextAsync("A nurse must release the placement hold.");
        await Expect(held.Page.GetByRole(AriaRole.Listbox)).ToBeHiddenAsync();

        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.WardTransferDesk);
        await desk.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Keep choices for confirmation", Exact = true }).CheckAsync();
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review placement choices", Exact = true }).ClickAsync();
        await Expect(desk.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await Expect(desk.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Return to transfer form", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#confirmation-instruction")).ToHaveTextAsync("Confirm a destination before closing.");
        var directory = desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward directory", Exact = true });
        await directory.GetByRole(AriaRole.Combobox).First.ClickAsync();
        var searchPanel = desk.Page.Locator("#Alis_Reactive_Conformance_App_Care_WardTransferDesk__Directory_popup");
        await Expect(searchPanel.GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await searchPanel.GetByRole(AriaRole.Combobox).PressSequentiallyAsync("a");
        await Expect(searchPanel.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar" });
        await searchPanel.GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Cedar", Exact = true }).ClickAsync();
        await Expect(directory.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");
        await Expect(searchPanel).ToBeHiddenAsync();
        var cedar = desk.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Cedar", Exact = true });
        var referral = desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check referral C2", Exact = true });
        await cedar.Or(referral).Last.ClickAsync();
        var confirmation = desk.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transfer confirmation", Exact = true });
        await Expect(confirmation.GetByRole(AriaRole.Combobox).Last).ToHaveValueAsync("Cedar");

        var finished = await NewWorld();
        await finished.Page.GotoAsync(CarePages.WardTransferDesk);
        await finished.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Review placement choices", Exact = true }).ClickAsync();
        await Expect(finished.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Maple", "Cedar" });
        await Expect(finished.Page.GetByRole(AriaRole.Dialog).GetByRole(AriaRole.Combobox)).ToBeFocusedAsync();
        await finished.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Return to transfer form", Exact = true }).ClickAsync();
        await Expect(finished.Page.Locator("#confirmation-instruction")).ToHaveTextAsync("Placement review closed.");
        await Expect(finished.Page.GetByRole(AriaRole.Listbox)).ToBeHiddenAsync();
    }
}
