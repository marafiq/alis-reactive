using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;

namespace Alis.Reactive.PlaywrightTests.Components.Brought.IntakeSheet;

/// <summary>
/// As the admissions coordinator, I want the wing's night desk told of a move-in with every fact as the
/// page knows it, wherever the fact lives and wherever the notice carries it, so that the desk works
/// from the same sheet I do.
/// </summary>
/// <remarks>
/// The facts live in six places: the intake sheet, a widget the sandbox wrote itself (its properties,
/// its contact object, what its functions return at once and what its waiting-list check returns a
/// moment later), the link that opened the page, the notice the page raises, the desk's plugins, and the
/// page itself (what it wrote in). Each fact goes wherever the notice takes it: the preview on the page,
/// the raised notice, the letter's plugin, the desk's address, its headers and its body; the desk's
/// receipt reads back what reached it by each. A fact carried in the wrong kind reads "?" in the letter
/// and binds wrong at the desk, so the receipt and the letter name the kind each fact arrived in.
/// The link names the resident, the wing, the room and the party: a link that names no wing stops the
/// notice before it is raised. Not exercised: the desk's address when the waiting list has not answered
/// (a route value with nothing to fill it), which the HTTP pipeline's own journeys prove.
/// </remarks>
[TestFixture]
public class WhenNotifyingTheWingOfAMoveIn : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/IntakeSheet";
    private const string MargaretsLink = Path + "?resident=margaret&wing=garden&room=Garden%20214&visitors=3";
    private const string Summary = "Margaret Hale, 62, assisted, moving in 2026-07-01";

    private ILocator Sheet => Page.Locator($"#{MoveInNoticeModel.SheetId}");
    private ILocator Preview => Page.Locator("#preview");
    private ILocator Notify => Page.Locator("#notify");
    private ILocator SendTheStandardNotice => Page.Locator("#notify-standard");
    private ILocator Reload => Page.Locator("#reload");

    private ILocator Field(string label) => Sheet.GetByLabel(label, new() { Exact = true });

    private ILocator Line(string id) => Page.Locator($"#{id}");

    private async Task OpenTheNotice(string link = MargaretsLink)
    {
        await NavigateToAndWaitForBoot(link);
        await Expect(Field("Applicant")).ToHaveValueAsync("Margaret Hale");
    }

    private async Task Type(string label, string text)
    {
        var field = Field(label);
        await field.FillAsync(text);
        await field.BlurAsync();
    }

    // ── a reaction value, from each place a fact lives ──────────────────────

    [Test]
    public async Task the_preview_reads_each_fact_from_where_it_lives()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();

        await Expect(Line("preview-name")).ToHaveTextAsync("Margaret Hale");
        await Expect(Line("preview-phone")).ToHaveTextAsync("555-0101");
        await Expect(Line("preview-summary")).ToHaveTextAsync(Summary);
        await Expect(Line("preview-position")).ToHaveTextAsync("2", new() { Timeout = 5000 });
        await Expect(Line("preview-wing")).ToHaveTextAsync("garden");
        await Expect(Line("preview-visitors")).ToHaveTextAsync("3");
        await Expect(Line("preview-source")).ToHaveTextAsync("Intake sheet");
        await Expect(Line("preview-years")).ToHaveTextAsync("0");
        await Expect(Line("preview-allergies")).ToHaveTextAsync("Penicillin");
        await Expect(Line("preview-code")).ToHaveTextAsync("margaret-hale");
        AssertNoConsoleErrors();
    }

    // The link's room is written into the sheet: a value from the link, into a property.
    [Test]
    public async Task the_preview_writes_the_rooms_from_the_link_onto_the_sheet()
    {
        await OpenTheNotice();
        await Expect(Field("Room preference")).ToHaveValueAsync("");

        await Preview.ClickAsync();

        await Expect(Field("Room preference")).ToHaveValueAsync("Garden 214");
        AssertNoConsoleErrors();
    }

    // ── a condition, from each place a fact lives ───────────────────────────

    [Test]
    public async Task the_familys_number_is_judged_from_the_sheets_contact_and_a_cleared_one_cannot_be_called()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();
        await Expect(Line("preview-local")).ToHaveTextAsync("The desk can call the family locally.");
        await Expect(Line("preview-phone-missing")).ToHaveTextAsync("The desk has the family's number.");

        await Type("Contact phone", "212-555-0101");
        await Preview.ClickAsync();
        await Expect(Line("preview-local")).ToHaveTextAsync("A long-distance call to the family.");

        await Type("Contact phone", "");
        await Preview.ClickAsync();
        await Expect(Line("preview-phone-missing")).ToHaveTextAsync("No family phone: the desk cannot call.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_age_is_judged_from_what_the_sheet_works_out_and_sixty_one_is_under_it()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();
        await Expect(Line("preview-eligible")).ToHaveTextAsync("Of age for the wing.");

        await Type("Age", "61");
        await Preview.ClickAsync();
        await Expect(Line("preview-eligible")).ToHaveTextAsync("Under the wing's age.");
        await Expect(Line("preview-years")).ToHaveTextAsync("-1");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_place_on_the_list_is_judged_once_the_list_answers_and_an_unanswered_list_is_unknown()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();
        await Expect(Line("preview-near-top")).ToHaveTextAsync("Near the top of the list.", new() { Timeout = 5000 });

        await Type("Waiting list number", "");
        await Preview.ClickAsync();
        await Expect(Line("preview-near-top")).ToHaveTextAsync("Further down the list, or unknown.", new() { Timeout = 5000 });
        await Expect(Line("preview-position")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_desk_and_the_party_are_judged_from_the_link_and_a_link_naming_neither_says_so()
    {
        await OpenTheNotice();
        await Preview.ClickAsync();
        await Expect(Line("preview-garden")).ToHaveTextAsync("The Garden wing's desk.");
        await Expect(Line("preview-party")).ToHaveTextAsync("A large party on move-in day: book the family room.");

        await OpenTheNotice(Path + "?resident=margaret");
        await Preview.ClickAsync();

        await Expect(Line("preview-garden")).ToHaveTextAsync("Another wing's desk, or none named.");
        await Expect(Line("preview-party")).ToHaveTextAsync("A small party, or none named.");
        await Expect(Line("preview-wing")).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_desk_code_is_judged_from_the_plugins_answer_and_a_new_name_makes_a_new_code()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();
        await Expect(Line("preview-known-code")).ToHaveTextAsync("Her desk code is on file.");

        await Type("Applicant", "Harold Benson");
        await Preview.ClickAsync();
        await Expect(Line("preview-known-code")).ToHaveTextAsync("A new desk code.");
        await Expect(Line("preview-code")).ToHaveTextAsync("harold-benson");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_gloves_are_judged_from_the_sheets_allergy_list()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();
        await Expect(Line("preview-latex")).ToHaveTextAsync("Standard gloves.");

        await Type("Allergies", "Penicillin, Latex");
        await Preview.ClickAsync();
        await Expect(Line("preview-latex")).ToHaveTextAsync("Latex-free gloves at the desk.");
        await Expect(Line("preview-allergies")).ToHaveTextAsync("Penicillin,Latex");
        AssertNoConsoleErrors();
    }

    // ── a plugin's arguments, from each place a fact lives, each in its own kind ──

    [Test]
    public async Task the_letter_is_written_from_every_fact_in_its_own_kind()
    {
        await OpenTheNotice();

        await Preview.ClickAsync();

        await Expect(Line("letter-opening")).ToHaveTextAsync("Margaret Hale moves into the garden wing on 2026-07-01.");
        await Expect(Line("letter-standing")).ToHaveTextAsync("Aged 62, a veteran, number 2 on the waiting list.", new() { Timeout = 5000 });
        await Expect(Line("letter-contact")).ToHaveTextAsync($"Call 555-0101; on file: Son 555-0199; {Summary}.");
        await Expect(Line("letter-allergies")).ToHaveTextAsync("Allergies: Penicillin (from the intake sheet).");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_civilian_with_no_answer_from_the_list_is_written_as_such()
    {
        await OpenTheNotice();
        await Field("Veteran").UncheckAsync();
        await Type("Waiting list number", "");

        await Preview.ClickAsync();

        await Expect(Line("letter-standing")).ToHaveTextAsync("Aged 62, a civilian, no number yet on the waiting list.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── a raised event's payload, from each place a fact lives ──────────────

    [Test]
    public async Task notifying_the_wing_raises_the_notice_with_every_fact_from_where_it_lives()
    {
        await OpenTheNotice();

        await Notify.ClickAsync();

        await Expect(Line("notice-status")).ToHaveTextAsync("Notice raised.");
        await Expect(Line("notice-applicant")).ToHaveTextAsync("Margaret Hale", new() { Timeout = 5000 });
        await Expect(Line("notice-phone")).ToHaveTextAsync("555-0101");
        await Expect(Line("notice-summary")).ToHaveTextAsync(Summary);
        await Expect(Line("notice-position")).ToHaveTextAsync("2");
        await Expect(Line("notice-wing")).ToHaveTextAsync("garden");
        await Expect(Line("notice-source")).ToHaveTextAsync("intake-sheet");
        await Expect(Line("notice-allergies")).ToHaveTextAsync("Penicillin");
        await Expect(Line("notice-services")).ToHaveTextAsync("Transport,Meals");
        await Expect(Line("notice-relation")).ToHaveTextAsync("Daughter");
        await Expect(Line("notice-record")).ToHaveTextAsync("Margaret Hale's record");
        await Expect(Line("notice-age")).ToHaveTextAsync("62");
        await Expect(Line("notice-veteran")).ToHaveTextAsync("true");
        await Expect(Line("notice-care")).ToHaveTextAsync("Assisted living.");
        await Expect(Line("notice-opening-day")).ToHaveTextAsync("On opening day.");
        await Expect(Line("notice-record-held")).ToHaveTextAsync("Record attached.");
        await Expect(Line("letter-notice")).ToHaveTextAsync("Notice for Margaret Hale (Daughter).");
        AssertNoConsoleErrors();
    }

    // The standard notice is written in the page as one object and raised as written.
    [Test]
    public async Task the_standard_notice_is_raised_as_written_in_the_page()
    {
        await OpenTheNotice();

        await SendTheStandardNotice.ClickAsync();

        await Expect(Line("notice-applicant")).ToHaveTextAsync("Standard notice", new() { Timeout = 5000 });
        await Expect(Line("notice-phone")).ToHaveTextAsync("555-0000");
        await Expect(Line("notice-position")).ToHaveTextAsync("9");
        await Expect(Line("notice-services")).ToHaveTextAsync("Meals");
        await Expect(Line("notice-relation")).ToHaveTextAsync("Family");
        await Expect(Line("notice-record")).ToHaveTextAsync("");
        await Expect(Line("notice-age")).ToHaveTextAsync("80");
        await Expect(Line("notice-veteran")).ToHaveTextAsync("false");
        await Expect(Line("notice-care")).ToHaveTextAsync("Not assisted living.");
        await Expect(Line("notice-opening-day")).ToHaveTextAsync("On opening day.");
        await Expect(Line("notice-record-held")).ToHaveTextAsync("No record attached.");
        await Expect(Line("letter-notice")).ToHaveTextAsync("Notice for Standard notice (Family).");
        AssertNoConsoleErrors();
    }

    // An edit is raised with the applicant as one object, read back member by member.
    [Test]
    public async Task editing_the_sheet_raises_the_edit_with_the_applicant_as_an_object()
    {
        await OpenTheNotice();

        await Type("Age", "63");

        await Expect(Line("edit-field")).ToHaveTextAsync("age");
        await Expect(Line("edit-name")).ToHaveTextAsync("Margaret Hale");
        await Expect(Line("edit-age")).ToHaveTextAsync("63");
        AssertNoConsoleErrors();
    }

    // ── the desk's address, headers and body, from each place a fact lives ──

    [Test]
    public async Task the_desk_receives_the_address_as_the_page_knows_it()
    {
        await OpenTheNotice();

        await Notify.ClickAsync();

        await Expect(Line("receipt-route")).ToHaveTextAsync("garden/night/Margaret Hale/62/555-0101/30/2/Daughter", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_desk_receives_the_headers_as_the_page_knows_them()
    {
        await OpenTheNotice();

        await Notify.ClickAsync();

        await Expect(Line("receipt-headers")).ToHaveTextAsync(
            $"garden · Margaret Hale · 555-0101 · {Summary} · 2 · intake-sheet · intake-sheet · Daughter · margaret-hale · 0",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_desk_receives_the_body_as_the_page_knows_it_each_fact_in_its_own_kind()
    {
        await OpenTheNotice();

        await Notify.ClickAsync();

        await Expect(Line("receipt-body")).ToHaveTextAsync(
            $"Margaret Hale · 555-0101 · {Summary} · 2 · garden · intake-sheet · intake-sheet · Daughter · Penicillin · Transport+Meals · margaret-hale · Assisted · 2026-07-01 · veteran · Son 555-0199 · 0",
            new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    // The standard notice goes to the desk with the sheet's facts beside the notice's own.
    [Test]
    public async Task the_standard_notice_reaches_the_desk_under_its_own_name_and_contact()
    {
        await OpenTheNotice();

        await SendTheStandardNotice.ClickAsync();

        await Expect(Line("receipt-route")).ToHaveTextAsync("garden/night/Standard notice/62/555-0101/30/2/Family", new() { Timeout = 10000 });
        await Expect(Line("receipt-headers")).ToHaveTextAsync(
            $"garden · Margaret Hale · 555-0101 · {Summary} · 2 · intake-sheet · standard · Family · margaret-hale · 0");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_link_that_names_no_wing_stops_the_notice_before_it_is_raised()
    {
        await OpenTheNotice(Path + "?resident=margaret");

        await Notify.ClickAsync();

        await Expect(Line("notice-status")).ToHaveTextAsync("Name the wing in the link before notifying.");
        await Expect(Line("notice-applicant")).ToHaveTextAsync("");
        await Expect(Line("receipt-route")).ToHaveTextAsync("Not sent yet.");
        AssertNoConsoleErrors();
    }

    // ── an object from a response, into a method; a response member, into a property ──

    [Test]
    public async Task reloading_from_the_waiting_list_fills_the_sheet_from_the_record()
    {
        await OpenTheNotice(Path + "?resident=harold&wing=garden");

        await Reload.ClickAsync();

        await Expect(Field("Applicant")).ToHaveValueAsync("Harold Benson", new() { Timeout = 10000 });
        await Expect(Field("Age")).ToHaveValueAsync("71");
        await Expect(Field("Move-in date")).ToHaveValueAsync("2026-07-15");
        await Expect(Field("Veteran")).Not.ToBeCheckedAsync();
        await Expect(Field("Care level")).ToHaveValueAsync("memoryCare");
        await Expect(Field("Monthly rate")).ToHaveValueAsync("6200.00");
        await Expect(Field("Room preference")).ToHaveValueAsync("Maple 118");
        await Expect(Field("Pets")).ToHaveValueAsync("0");
        await Expect(Field("Previous stay ended")).ToHaveValueAsync("2025-12-31");
        await Expect(Field("Flu shot")).ToHaveValueAsync("yes");
        await Expect(Field("Diet")).ToHaveValueAsync("pureed");
        await Expect(Field("Contact phone")).ToHaveValueAsync("555-0142");
        await Expect(Field("Allergies")).ToHaveValueAsync("Latex, Codeine");
        await Expect(Field("Waiting list number")).ToHaveValueAsync("5");
        await Expect(Line("record-wing")).ToHaveTextAsync("garden wing");
        await Expect(Line("record-first-allergy")).ToHaveTextAsync("Latex");
        AssertNoConsoleErrors();
    }
}
