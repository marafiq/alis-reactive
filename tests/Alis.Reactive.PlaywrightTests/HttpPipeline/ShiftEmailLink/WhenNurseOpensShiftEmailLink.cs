namespace Alis.Reactive.PlaywrightTests.HttpPipeline.ShiftEmailLink;

/// <summary>
/// As a nurse, I want the link in the shift email to open the resident's file where the email points,
/// at my community and for my shift, so that I act on the right resident's doses and notes without
/// searching for them.
/// </summary>
/// <remarks>
/// The link names the community, the resident, the shift, the section to open, the page of shift notes
/// and every wing the nurse covers; the page reads each from the link as it opens. The community's API
/// answers only for the community its X-Community header names: Helen Park (1042) and Arthur Bell
/// (1043) live at Maple Grove, and Cedar Point has neither. The chart is asked for by the resident in
/// its path, with the shift in its query, and names the dose due on that shift; the page then asks when
/// that dose was last given, by the resident the link names and the medication the chart named, both in
/// the path. Arthur Bell's dose is Carbidopa/Levodopa 25/100 mg: sent unencoded, its slashes would split
/// the path and the dose would not be found. The shift notes are asked for by shift and page in the
/// query. The pharmacy and the lab are checked side by side through the community's gateway, which
/// forwards each check to the partner its X-Partner header names. The "Checking with the pharmacy and
/// the lab..." line is not asserted: it is replaced once both checks settle, and that the line waits for
/// both is a behavior of parallel requests that
/// <c>WhenHoldingVisitsAtTheFrontDesk.the_arrival_card_waits_for_the_badge_with_the_desk_covered</c> proves.
/// </remarks>
[TestFixture]
public class WhenNurseOpensShiftEmailLink : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/ShiftEmailLink";
    private const string HelensNightLink = "community=maple-grove&resident=1042&shift=Night&page=1";
    private const string NotFound = "the server responded with a status of 404";

    private ILocator LinkShift => Page.Locator("#link-shift");
    private ILocator LinkWings => Page.Locator("#link-wings");
    private ILocator Resident => Page.Locator("#file-resident");
    private ILocator Room => Page.Locator("#file-room");
    private ILocator Community => Page.Locator("#file-community");
    private ILocator Refusal => Page.Locator("#file-refusal");
    private ILocator OverviewTab => Page.Locator("#file-overview-tab");
    private ILocator MedicationsTab => Page.Locator("#file-medications-tab");
    private ILocator Overview => Page.Locator("#file-overview");
    private ILocator Medications => Page.Locator("#file-medications");
    private ILocator NotesPage => Page.Locator("#notes-page");
    private ILocator NotesEntry => Page.Locator("#notes-entry");
    private ILocator OlderNotesHint => Page.Locator("#notes-older-hint");
    private ILocator DueDose => Page.Locator("#due-dose");
    private ILocator LastDose => Page.Locator("#last-dose");
    private ILocator CheckPharmacyAndLab => Page.Locator("#check-partners-btn");
    private ILocator PartnersStatus => Page.Locator("#partners-status");
    private ILocator PharmacyAnswer => Page.Locator("#pharmacy-answer");
    private ILocator LabAnswer => Page.Locator("#lab-answer");

    private Task OpenLink(string query) => NavigateToAndWaitForBoot($"{Path}?{query}");

    [Test]
    public async Task a_link_to_the_medications_opens_the_medications_section()
    {
        await OpenLink(HelensNightLink + "&tab=medications");

        await Expect(Medications).ToBeVisibleAsync();
        await Expect(Overview).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_that_names_no_section_opens_the_overview()
    {
        await OpenLink(HelensNightLink);

        await Expect(Overview).ToBeVisibleAsync();
        await Expect(Medications).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task i_can_move_between_the_overview_and_the_medications()
    {
        await OpenLink(HelensNightLink);
        await Expect(Overview).ToBeVisibleAsync();

        await ClickWhenStable(MedicationsTab);
        await Expect(Medications).ToBeVisibleAsync();
        await Expect(Overview).ToBeHiddenAsync();

        await ClickWhenStable(OverviewTab);
        await Expect(Overview).ToBeVisibleAsync();
        await Expect(Medications).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_page_names_the_shift_the_email_was_sent_for()
    {
        await OpenLink(HelensNightLink);

        await Expect(LinkShift).ToHaveTextAsync("Night");
        AssertNoConsoleErrors();
    }

    // The page reads the wings as MVC binds them from a query string: every value of the key, whatever
    // case the link spells the key in.
    [Test]
    public async Task a_link_that_names_several_wings_lists_every_wing_however_it_spells_wing()
    {
        await OpenLink(HelensNightLink + "&wing=Maple&WING=Cedar&Wing=Birch");

        await Expect(LinkWings).ToHaveTextAsync("Maple,Cedar,Birch");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_shift_notes_open_at_the_page_the_link_names()
    {
        await OpenLink("community=maple-grove&resident=1042&shift=Night&page=2");

        await Expect(NotesPage).ToHaveTextAsync("2");
        await Expect(NotesEntry).ToHaveTextAsync("2:15 AM: Woke asking for water and settled after a short walk.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task notes_past_the_first_page_say_the_newest_notes_are_on_page_one()
    {
        await OpenLink("community=maple-grove&resident=1042&shift=Night&page=2");

        await Expect(OlderNotesHint).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    // The page number is written before the page decides on the hint, so once it shows "1" the hint's
    // absence is the page's decision.
    [Test]
    public async Task the_first_page_of_notes_does_not_send_me_to_page_one()
    {
        await OpenLink(HelensNightLink);

        await Expect(NotesPage).ToHaveTextAsync("1");
        await Expect(OlderNotesHint).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_link_opens_the_residents_chart_at_its_community_with_the_dose_due_on_its_shift()
    {
        await OpenLink(HelensNightLink + "&tab=medications");

        await Expect(Resident).ToHaveTextAsync("Helen Park");
        await Expect(Room).ToHaveTextAsync("Maple 214");
        await Expect(Community).ToHaveTextAsync("Maple Grove");
        await Expect(DueDose).ToHaveTextAsync("Donepezil 10 mg at 9:00 PM");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_sent_for_the_day_shift_shows_the_dose_due_on_the_day_shift()
    {
        await OpenLink("community=maple-grove&resident=1042&shift=Day&page=1&tab=medications");

        await Expect(DueDose).ToHaveTextAsync("Metformin 500 mg at 8:00 AM");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_naming_a_community_the_resident_does_not_live_in_says_so_and_opens_no_chart()
    {
        await OpenLink("community=cedar-point&resident=1042&shift=Night&page=1");

        await Expect(Refusal).ToHaveTextAsync("Resident 1042 does not live at Cedar Point.");
        await Expect(Resident).ToHaveTextAsync("No chart open.");
        AssertNoConsoleErrorsExcept(NotFound);
    }

    [Test]
    public async Task the_medications_show_when_the_dose_due_on_my_shift_was_last_given()
    {
        await OpenLink(HelensNightLink + "&tab=medications");

        await Expect(LastDose).ToHaveTextAsync("Last given yesterday at 9:04 PM by Nora Ellis.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_dose_whose_name_holds_slashes_still_shows_when_it_was_last_given()
    {
        await OpenLink("community=maple-grove&resident=1043&shift=Night&page=1&tab=medications");
        await Expect(DueDose).ToHaveTextAsync("Carbidopa/Levodopa 25/100 mg at 10:00 PM");

        await Expect(LastDose).ToHaveTextAsync("Last given today at 6:00 PM by Malik Stone.");
        AssertNoConsoleErrors();
    }

    // Both checks go to the same gateway address; only each one's X-Partner header tells the gateway
    // where to forward it, so each line shows its own partner's answer only if each check carried its own.
    [Test]
    public async Task checking_with_the_pharmacy_and_the_lab_shows_each_ones_own_answer()
    {
        await OpenLink(HelensNightLink + "&tab=medications");

        await ClickWhenStable(CheckPharmacyAndLab);

        await Expect(PharmacyAnswer).ToHaveTextAsync("Riverside Pharmacy: the Donepezil refill is ready for pickup.");
        await Expect(LabAnswer).ToHaveTextAsync("Northgate Lab: the blood panel results are due Friday.");
        await Expect(PartnersStatus).ToHaveTextAsync("Pharmacy and lab checked.");
        AssertNoConsoleErrors();
    }
}
