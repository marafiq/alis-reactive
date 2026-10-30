using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AutoComplete;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.AutoComplete;

/// <summary>
/// As a charge nurse on the evening shift, I want to find the physician to page by typing part of a name,
/// with the list filtered by the hospital's physician directory, and see how that physician is reached, so
/// that Margaret Hale's change of condition reaches the right physician and nobody else.
/// </summary>
/// <remarks>
/// The directory holds Dr. Anita Patel (Geriatrics, 555-0142, her attending), Dr. Lena Patterson (Internal
/// Medicine), Dr. James Okafor (Cardiology, 555-0148), Dr. Maria Reyes (Hospital Medicine) and Dr. Wen Lim
/// (Psychiatry), who are on call tonight, and Dr. Elena Novak. Her care team, Dr. Patel and Dr. Okafor, is
/// what the field holds before the directory answers; the directory answers a search after 1.5 seconds,
/// and the page says it is searching meanwhile. A physician chosen by name is to be confirmed with the
/// answering service, and the keyboard goes away so the callback shows; one the page fills in is not. Once
/// the page is sent, the physician it went to cannot be changed until the nurse pages someone else.
/// Not exercised: writing Text, which a page never needs: it changes the physician by value; Text is read.
/// Enabled is written here (the page locks and opens the field); it is not read. The Fields overload with
/// a group member is not used: no physician list here is grouped (the MultiSelect diet order groups
/// allergies by kind). Change's e, item and itemData fields are not declared; IsInteracted is EJ2's reading
/// of e.
/// </remarks>
[TestFixture]
public class WhenUsingFusionAutoComplete : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/AutoComplete";
    private const string Searching = "Searching the physician directory...";

    private PagePlan<FusionAutoCompleteModel> _plan = null!;

    private AutoCompleteLocator Physician => _plan.AutoComplete(m => m.Physician);
    private ILocator PageHerAttending => Page.Locator("#page-attending-btn");
    private ILocator OnCallTonight => Page.Locator("#on-call-btn");
    private ILocator AttendingFilled => Page.Locator("#attending-filled");
    private ILocator DirectoryStatus => Page.Locator("#directory-status");
    private ILocator CheckPhysician => Page.Locator("#check-physician");
    private ILocator Contact => Page.Locator("#physician-contact");
    private ILocator Specialty => Page.Locator("#physician-specialty");
    private ILocator Callback => Page.Locator("#physician-callback");
    private ILocator SendPage => Page.Locator("#send-page-btn");
    private ILocator PageSomeoneElse => Page.Locator("#page-again-btn");
    private ILocator PageStatus => Page.Locator("#page-status");

    // The cross the field shows while it holds a physician and has the cursor.
    private ILocator ClearButton => Physician.Input.Locator("xpath=..").Locator(".e-clear-icon");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionAutoCompleteModel>.FromPage(Page);
        await Expect(Physician.Input).ToBeEditableAsync(new() { Timeout = 5000 });
    }

    // Key by key, as the field searches on each key.
    private async Task TypeInPhysician(string text)
    {
        await ClickWhenStable(Physician.Input);
        await Physician.Input.PressSequentiallyAsync(text, new() { Delay = 50 });
    }

    private async Task ChooseByName(string typed, string physician)
    {
        await TypeInPhysician(typed);
        await Physician.SelectItem(physician);
    }

    private async Task SendAboutAFall()
    {
        await ClickWhenStable(PageHerAttending);
        await Expect(Physician.Input).ToHaveValueAsync("Dr. Anita Patel", new() { Timeout = 5000 });
        await _plan.TextBox(m => m.Concern).Fill("Fell in the bathroom.");
        await ClickWhenStable(SendPage);
        await Expect(PageStatus).ToHaveTextAsync("Paged Dr. Anita Patel on 555-0142 about Margaret Hale: Fell in the bathroom.", new() { Timeout = 5000 });
    }

    [Test]
    public async Task typing_part_of_a_name_offers_the_physicians_the_directory_matches()
    {
        await NavigateAndBoot();

        await TypeInPhysician("pat");

        await Expect(DirectoryStatus).ToHaveTextAsync("2 physicians in the directory match \"pat\".", new() { Timeout = 10000 });
        await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Anita Patel", "Dr. Lena Patterson" });
        AssertNoConsoleErrors();
    }

    // Her care team has Dr. Okafor, but the list waits for the directory's answer rather than offering him
    // from the care team; the status still says searching after the list is seen empty.
    [Test]
    public async Task while_the_directory_is_searched_her_care_team_is_not_offered_for_what_was_typed()
    {
        await NavigateAndBoot();

        await TypeInPhysician("oka");

        await Expect(DirectoryStatus).ToHaveTextAsync(Searching, new() { Timeout = 5000 });
        await Expect(Physician.PopupItems).ToHaveCountAsync(0);
        await Expect(DirectoryStatus).ToHaveTextAsync(Searching);
        await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. James Okafor" }, new() { Timeout = 10000 });
        await Expect(DirectoryStatus).ToHaveTextAsync("1 physician in the directory matches \"oka\".");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_name_nobody_in_the_directory_has_closes_the_list_and_says_so()
    {
        await NavigateAndBoot();
        await TypeInPhysician("pat");
        await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Anita Patel", "Dr. Lena Patterson" }, new() { Timeout = 10000 });

        await Physician.Input.PressSequentiallyAsync("z", new() { Delay = 50 });

        await Expect(DirectoryStatus).ToHaveTextAsync("No physician in the directory matches \"patz\".", new() { Timeout = 10000 });
        await Expect(Physician.Popup).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_physician_shows_their_specialty_and_callback_number()
    {
        await NavigateAndBoot();

        await ChooseByName("oka", "Dr. James Okafor");

        await Expect(Physician.Input).ToHaveValueAsync("Dr. James Okafor");
        await Expect(Contact).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Specialty).ToHaveTextAsync("Cardiology");
        await Expect(Callback).ToHaveTextAsync("555-0148");
        AssertNoConsoleErrors();
    }

    // Choosing from the list leaves the cursor in the field; only the page moves it out.
    [Test]
    public async Task a_physician_chosen_by_name_is_to_be_confirmed_and_the_keyboard_goes_away()
    {
        await NavigateAndBoot();

        await ChooseByName("oka", "Dr. James Okafor");

        await Expect(CheckPhysician).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Physician.Input).Not.ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_her_attending_fills_in_dr_patel_and_says_whom()
    {
        await NavigateAndBoot();

        await ClickWhenStable(PageHerAttending);

        await Expect(Physician.Input).ToHaveValueAsync("Dr. Anita Patel", new() { Timeout = 5000 });
        await Expect(AttendingFilled).ToBeVisibleAsync();
        await Expect(AttendingFilled).ToHaveTextAsync("Her attending, Dr. Anita Patel, is filled in.");
        await Expect(Specialty).ToHaveTextAsync("Geriatrics", new() { Timeout = 5000 });
        await Expect(CheckPhysician).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The last search offered only Dr. Okafor; her attending still replaces him, and needs no confirming.
    [Test]
    public async Task paging_her_attending_replaces_a_physician_chosen_by_name()
    {
        await NavigateAndBoot();
        await ChooseByName("oka", "Dr. James Okafor");
        await Expect(CheckPhysician).ToBeVisibleAsync(new() { Timeout = 5000 });

        await ClickWhenStable(PageHerAttending);

        await Expect(Physician.Input).ToHaveValueAsync("Dr. Anita Patel", new() { Timeout = 5000 });
        await Expect(AttendingFilled).ToBeVisibleAsync();
        await Expect(AttendingFilled).ToHaveTextAsync("Her attending, Dr. Anita Patel, is filled in.");
        await Expect(Specialty).ToHaveTextAsync("Geriatrics", new() { Timeout = 5000 });
        await Expect(CheckPhysician).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // The on-call list does not hold her attending; paging her brings her care team back first.
    [Test]
    public async Task paging_her_attending_after_opening_the_on_call_list_fills_in_dr_patel()
    {
        await NavigateAndBoot();
        await ClickWhenStable(OnCallTonight);
        await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Maria Reyes", "Dr. Wen Lim" }, new() { Timeout = 5000 });

        await ClickWhenStable(PageHerAttending);

        await Expect(Physician.Input).ToHaveValueAsync("Dr. Anita Patel", new() { Timeout = 5000 });
        await Expect(AttendingFilled).ToBeVisibleAsync();
        await Expect(AttendingFilled).ToHaveTextAsync("Her attending, Dr. Anita Patel, is filled in.");
        await Expect(Specialty).ToHaveTextAsync("Geriatrics", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task on_call_tonight_opens_the_list_of_tonights_on_call_physicians()
    {
        await NavigateAndBoot();

        await ClickWhenStable(OnCallTonight);

        await Expect(Physician.Popup).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Physician.PopupItems).ToHaveTextAsync(new[] { "Dr. Maria Reyes", "Dr. Wen Lim" });
        await ClickWhenStable(Physician.PopupItem("Dr. Maria Reyes"));
        await Expect(Specialty).ToHaveTextAsync("Hospital Medicine", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_an_empty_page_asks_for_the_physician_and_what_changed()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SendPage);

        await Expect(_plan.ErrorFor(m => m.Physician)).ToHaveTextAsync("Choose the physician to page.", new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.Concern)).ToHaveTextAsync("Say what changed for her.");
        await Expect(PageStatus).ToHaveTextAsync("Not paged yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_physician_clears_the_reminder_to_choose_one()
    {
        await NavigateAndBoot();
        await ClickWhenStable(SendPage);
        await Expect(_plan.ErrorFor(m => m.Physician)).ToHaveTextAsync("Choose the physician to page.", new() { Timeout = 5000 });

        await ChooseByName("oka", "Dr. James Okafor");

        await Expect(_plan.ErrorFor(m => m.Physician)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(_plan.ErrorFor(m => m.Concern)).ToHaveTextAsync("Say what changed for her.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_page_names_the_physician_their_number_and_what_changed()
    {
        await NavigateAndBoot();
        await ChooseByName("pat", "Dr. Lena Patterson");
        await _plan.TextBox(m => m.Concern).Fill("Short of breath after lunch.");

        await ClickWhenStable(SendPage);

        await Expect(PageStatus).ToHaveTextAsync(
            "Paged Dr. Lena Patterson on 555-0161 about Margaret Hale: Short of breath after lunch.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_page_is_sent_the_physician_it_went_to_cannot_be_changed()
    {
        await NavigateAndBoot();

        await SendAboutAFall();

        await Expect(Physician.Input).ToBeDisabledAsync(new() { Timeout = 5000 });
        await Expect(PageSomeoneElse).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task paging_someone_else_empties_the_field_and_puts_the_cursor_there()
    {
        await NavigateAndBoot();
        await SendAboutAFall();
        await Expect(Physician.Input).ToBeDisabledAsync(new() { Timeout = 5000 });

        await ClickWhenStable(PageSomeoneElse);

        await Expect(Physician.Input).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Physician.Input).ToHaveValueAsync("");
        await Expect(Physician.Input).ToBeFocusedAsync();
        await Expect(Contact).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_physician_takes_down_their_callback_number()
    {
        await NavigateAndBoot();
        await ChooseByName("oka", "Dr. James Okafor");
        await Expect(Contact).ToBeVisibleAsync(new() { Timeout = 5000 });
        await ClickWhenStable(Physician.Input);

        await ClickWhenStable(ClearButton);

        await Expect(Physician.Input).ToHaveValueAsync("", new() { Timeout = 5000 });
        await Expect(Contact).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(CheckPhysician).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
