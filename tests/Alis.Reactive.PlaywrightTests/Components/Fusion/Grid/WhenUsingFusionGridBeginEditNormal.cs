namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Grid;

/// <summary>
/// As the Maple wing wellness nurse, I want to open each resident's review form from the care review list and
/// have a record on hold stay closed with the reason, so that I never change a record the hospital transfer
/// team holds, and the server knows which record I have open.
/// </summary>
/// <remarks>
/// The list is Amina Patel, Grace Bennett (on hold for a hospital transfer to Mercy General), Henry Liu and
/// Irene Morgan. The nurse selects a resident and presses Open review; the review form saves with its own Save
/// and Cancel buttons. Opening a review checks the record out on the server, which names the resident's census
/// line.
/// The review form's Risk level list opens on its first choice, Low, not on the resident's risk: the form's
/// select field carries no value of the row (FusionGridEditTemplates DialogForm), so the nurse chooses the risk
/// the review sets, and the tests do too.
/// Not exercised:
/// BeginEdit's Type: EJ2 sends "edit" on every beginEdit (normal-edit.js:110), so it carries nothing a page
/// could show.
/// BeginEdit's Cancel as a value read: the page writes it, and EJ2 reads it back (normal-edit.js:151).
/// </remarks>
[TestFixture]
public class WhenUsingFusionGridBeginEditNormal : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Grid/BeginEditNormal";
    private const string ReviewListId = "care-review-grid";

    // The list numbers its columns from 1.
    private const int ResidentColumn = 2;
    private const int RiskColumn = 4;
    private const int OpenTasksColumn = 5;

    private ILocator Rows => Page.Locator($"#{ReviewListId} .e-row");
    private ILocator Cell(string resident, int column) =>
        Rows.Filter(new() { HasText = resident }).Locator($"td[aria-colindex='{column}']");
    private ILocator ReviewForm => Page.Locator($"#{ReviewListId}_dialogEdit_wrapper");
    private ILocator ReviewField(string field) => ReviewForm.Locator($"[name='{field}']");
    private ILocator ReviewButton(string text) => ReviewForm.Locator(".e-footer-content button", new() { HasText = text });
    private ILocator OpenReview => Page.Locator("#open-review");
    private ILocator Checkout => Page.Locator("#review-checkout");
    private ILocator HoldNotice => Page.Locator("#hold-notice");
    private ILocator ReviewSaved => Page.Locator("#review-saved");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Rows).ToHaveCountAsync(4, new() { Timeout = 10000 });
    }

    private async Task OpenReviewOf(string resident)
    {
        await Cell(resident, ResidentColumn).ClickAsync();
        await ClickWhenStable(OpenReview);
    }

    [Test]
    public async Task opening_a_review_shows_the_residents_name_and_open_tasks_in_the_form()
    {
        await NavigateAndBoot();

        await OpenReviewOf("Henry Liu");

        await Expect(ReviewForm).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(ReviewField("residentName")).ToHaveValueAsync("Henry Liu");
        await Expect(ReviewField("openTasks")).ToHaveValueAsync("6");
        // The risk list opens on Henry's moderate risk, so a review saved untouched keeps it.
        await Expect(ReviewField("riskLevel")).ToHaveValueAsync("Moderate");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_review_checks_the_record_out_on_its_census_line()
    {
        await NavigateAndBoot();

        await OpenReviewOf("Henry Liu");

        await Expect(Checkout).ToHaveTextAsync(
            "Henry Liu's record is checked out to you for review (census line 3).", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_record_on_hold_stays_closed_and_says_why()
    {
        await NavigateAndBoot();

        await OpenReviewOf("Grace Bennett");

        await Expect(HoldNotice).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(HoldNotice).ToHaveTextAsync(
            "Grace Bennett's record is on hold (Hospital transfer to Mercy General). Lift the hold to open the review.");
        await Expect(ReviewForm).ToHaveCountAsync(0);
        await Expect(Checkout).ToHaveTextAsync("No review open.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_a_review_sets_the_residents_risk_and_open_tasks()
    {
        await NavigateAndBoot();
        await OpenReviewOf("Henry Liu");
        await Expect(ReviewField("openTasks")).ToHaveValueAsync("6", new() { Timeout = 10000 });
        await ReviewField("riskLevel").SelectOptionAsync("High");
        await ReviewField("openTasks").FillAsync("4");

        await ClickWhenStable(ReviewButton("Save"));

        await Expect(ReviewForm).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", RiskColumn)).ToHaveTextAsync("High");
        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveTextAsync("4");
        await Expect(ReviewSaved).ToHaveTextAsync("Review saved for Henry Liu: High risk, 4 open tasks.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task cancelling_a_review_leaves_the_resident_as_they_were()
    {
        await NavigateAndBoot();
        await OpenReviewOf("Henry Liu");
        await Expect(ReviewField("openTasks")).ToHaveValueAsync("6", new() { Timeout = 10000 });
        await ReviewField("openTasks").FillAsync("9");

        await ClickWhenStable(ReviewButton("Cancel"));

        await Expect(ReviewForm).ToBeHiddenAsync(new() { Timeout = 10000 });
        await Expect(Cell("Henry Liu", OpenTasksColumn)).ToHaveTextAsync("6");
        await Expect(ReviewSaved).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
