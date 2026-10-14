using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.MultiSelect;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.MultiSelect;

/// <summary>
/// As a dietitian, I want to record a resident's allergies, the snacks her texture allows, the dishes she
/// will not eat and her supplements in one diet order, and see the tray ticket the kitchen will print, so
/// that her tray never carries what she reacts to or refuses.
/// </summary>
/// <remarks>
/// Margaret Hale's chart lists a peanut allergy; her hospital discharge summary lists shellfish and
/// penicillin. Allergies are grouped Food, Medication, Environmental and None; the tray ticket prints the
/// food allergies only, and the page says whether the allergies on file are from her chart, changed by the
/// dietitian, or copied from the discharge summary. Choosing a texture fills the snack list with what that
/// texture allows and opens it (pureed: Applesauce, Pudding, Yogurt). The kitchen substitutes up to three
/// dishes; the third closes the list. The supplement field offers the kitchen's most-ordered supplements
/// (Magic Cup, Ensure Plus) until the catalog answers a search, 1.5 seconds later.
/// Not exercised: the render helper without a build step: a MultiSelect with no items has nothing to
/// choose. Change's e, element and oldValue fields are not declared; IsInteracted is EJ2's reading of e.
/// </remarks>
[TestFixture]
public class WhenUsingFusionMultiSelect : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/MultiSelect";
    private const string Searching = "Searching the kitchen's catalog...";

    private PagePlan<FusionMultiSelectModel> _plan = null!;

    private MultiSelectLocator Allergies => _plan.MultiSelect(m => m.Allergies);
    private MultiSelectLocator Snacks => _plan.MultiSelect(m => m.Snacks);
    private MultiSelectLocator Dislikes => _plan.MultiSelect(m => m.Dislikes);
    private MultiSelectLocator Supplements => _plan.MultiSelect(m => m.Supplements);
    private ILocator AllergyChips => Allergies.Wrapper.Locator(".e-chips");
    private ILocator AllergySource => Page.Locator("#allergy-source");
    private ILocator TrayTicket => Page.Locator("#tray-ticket");
    private ILocator DislikeLimit => Page.Locator("#dislike-limit");
    private ILocator SupplementStatus => Page.Locator("#supplement-status");
    private ILocator CopyFromDischarge => Page.Locator("#copy-discharge-btn");
    private ILocator SaveDietOrder => Page.Locator("#save-diet-order-btn");
    private ILocator SaveStatus => Page.Locator("#save-status");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionMultiSelectModel>.FromPage(Page);
        await Expect(AllergyChips).ToHaveTextAsync(new[] { "Peanuts" }, new() { Timeout = 5000 });
    }

    // A list with check boxes stays open while dishes are ticked.
    private async Task TickDish(string dish) =>
        await ClickWhenStable(Dislikes.PopupItems.GetByText(dish, new() { Exact = true }));

    private async Task SearchSupplements(string typed)
    {
        await Supplements.Open();
        await Page.Keyboard.TypeAsync(typed, new() { Delay = 50 });
    }

    [Test]
    public async Task her_peanut_allergy_on_file_shows_and_is_on_the_tray_ticket()
    {
        await NavigateAndBoot();

        await Expect(AllergyChips).ToHaveTextAsync(new[] { "Peanuts" });
        await Expect(TrayTicket).ToHaveTextAsync("Tray ticket: no peanuts.");
        await Expect(AllergySource).ToHaveTextAsync("From her chart.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_allergy_list_groups_allergies_by_kind()
    {
        await NavigateAndBoot();

        await Allergies.Open();

        await Expect(Allergies.Popup.Locator(".e-list-group-item")).ToHaveTextAsync(new[] { "Food", "Medication", "Environmental", "None" });
        await Expect(Allergies.PopupItem("Sulfa drugs")).ToBeVisibleAsync();
        await Expect(Allergies.PopupItem("No known allergies")).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_an_allergy_puts_it_on_the_tray_ticket_and_says_who_changed_it()
    {
        await NavigateAndBoot();

        await Allergies.SelectItem("Shellfish");

        await Expect(AllergyChips).ToHaveTextAsync(new[] { "Peanuts", "Shellfish" });
        await Expect(TrayTicket).ToHaveTextAsync("Tray ticket: no peanuts, no shellfish.", new() { Timeout = 5000 });
        await Expect(AllergySource).ToHaveTextAsync("Changed by you.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task removing_her_only_food_allergy_takes_it_off_the_tray_ticket()
    {
        await NavigateAndBoot();

        await Allergies.RemoveItem("Peanuts");

        await Expect(AllergyChips).ToHaveCountAsync(0);
        await Expect(TrayTicket).ToHaveTextAsync("Tray ticket: no food allergies.", new() { Timeout = 5000 });
        await Expect(AllergySource).ToHaveTextAsync("Changed by you.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task copying_the_discharge_summary_replaces_her_allergies_and_says_where_they_came_from()
    {
        await NavigateAndBoot();

        await ClickWhenStable(CopyFromDischarge);

        await Expect(AllergyChips).ToHaveTextAsync(new[] { "Shellfish", "Penicillin" }, new() { Timeout = 5000 });
        await Expect(TrayTicket).ToHaveTextAsync("Tray ticket: no shellfish.", new() { Timeout = 5000 });
        await Expect(AllergySource).ToHaveTextAsync("From her hospital discharge summary.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_pureed_texture_opens_the_snacks_a_pureed_diet_allows()
    {
        await NavigateAndBoot();
        await Expect(Snacks.Popup).ToBeHiddenAsync();

        await _plan.NativeDropDown(m => m.Texture).Select("Pureed");

        await Expect(Snacks.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Snacks.PopupItems).ToHaveTextAsync(new[] { "Applesauce", "Pudding", "Yogurt" });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task select_all_offers_her_every_snack_her_texture_allows()
    {
        await NavigateAndBoot();
        await _plan.NativeDropDown(m => m.Texture).Select("Pureed");
        await Expect(Snacks.PopupItems).ToHaveTextAsync(new[] { "Applesauce", "Pudding", "Yogurt" }, new() { Timeout = 5000 });

        await ClickWhenStable(Snacks.Popup.GetByText("Select All", new() { Exact = true }));
        await ClickWhenStable(SaveDietOrder);

        await Expect(SaveStatus).ToHaveTextAsync(
            "Saved. Allergies: Peanuts. Texture: Pureed. Snacks: Applesauce, Pudding, Yogurt. Never served: none. Supplements: none.",
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_third_dish_she_will_not_eat_closes_the_list_and_says_three_is_the_limit()
    {
        await NavigateAndBoot();
        await Dislikes.Open();
        await TickDish("Liver and onions");
        await TickDish("Fish sticks");

        await TickDish("Chili");

        await Expect(Dislikes.Popup).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(DislikeLimit).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task two_dishes_leave_the_list_open_for_a_third()
    {
        await NavigateAndBoot();
        await Dislikes.Open();
        await TickDish("Liver and onions");

        await TickDish("Fish sticks");

        await Expect(Dislikes.PopupItems.Filter(new() { HasText = "Fish sticks" })).ToHaveAttributeAsync("aria-selected", "true", new() { Timeout = 5000 });
        await Expect(Dislikes.Popup).ToBeVisibleAsync();
        await Expect(DislikeLimit).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_part_of_a_supplement_offers_what_the_kitchens_catalog_matches()
    {
        await NavigateAndBoot();

        await SearchSupplements("glu");

        await Expect(SupplementStatus).ToHaveTextAsync("2 supplements in the kitchen's catalog match \"glu\".", new() { Timeout = 10000 });
        await Expect(Supplements.PopupItems).ToHaveTextAsync(new[] { "Glucerna", "Glucerna Hunger Smart" });
        AssertNoConsoleErrors();
    }

    // Nothing among the usual supplements matches "glu"; the list keeps them rather than saying nothing
    // matches, and the status still says searching after the list is seen.
    [Test]
    public async Task while_the_catalog_is_searched_the_list_keeps_the_usual_supplements()
    {
        await NavigateAndBoot();

        await SearchSupplements("glu");

        await Expect(SupplementStatus).ToHaveTextAsync(Searching, new() { Timeout = 5000 });
        await Expect(Supplements.PopupItems).ToHaveTextAsync(new[] { "Magic Cup", "Ensure Plus" });
        await Expect(SupplementStatus).ToHaveTextAsync(Searching);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_sends_the_dishes_she_refuses_and_a_supplement_found_in_the_catalog()
    {
        await NavigateAndBoot();
        await Dislikes.Open();
        await TickDish("Liver and onions");
        await Dislikes.Blur();
        await SearchSupplements("glu");
        await Expect(Supplements.PopupItems).ToHaveTextAsync(new[] { "Glucerna", "Glucerna Hunger Smart" }, new() { Timeout = 10000 });
        await ClickWhenStable(Supplements.PopupItems.GetByText("Glucerna", new() { Exact = true }));

        await ClickWhenStable(SaveDietOrder);

        await Expect(SaveStatus).ToHaveTextAsync(
            "Saved. Allergies: Peanuts. Texture: Regular. Snacks: none. Never served: Liver and onions. Supplements: Glucerna.",
            new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_without_her_allergies_asks_for_them()
    {
        await NavigateAndBoot();
        await Allergies.RemoveItem("Peanuts");

        await ClickWhenStable(SaveDietOrder);

        await Expect(_plan.ErrorFor(m => m.Allergies)).ToHaveTextAsync("Record her allergies, or choose No known allergies.", new() { Timeout = 5000 });
        await Expect(SaveStatus).ToHaveTextAsync("Not saved yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task recording_no_known_allergies_clears_the_reminder()
    {
        await NavigateAndBoot();
        await Allergies.RemoveItem("Peanuts");
        await ClickWhenStable(SaveDietOrder);
        await Expect(_plan.ErrorFor(m => m.Allergies)).ToHaveTextAsync("Record her allergies, or choose No known allergies.", new() { Timeout = 5000 });

        await Allergies.SelectItem("No known allergies");

        await Expect(_plan.ErrorFor(m => m.Allergies)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(TrayTicket).ToHaveTextAsync("Tray ticket: no food allergies.");
        AssertNoConsoleErrors();
    }
}
