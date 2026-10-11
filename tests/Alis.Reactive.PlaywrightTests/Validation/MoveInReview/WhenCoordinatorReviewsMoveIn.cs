using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.MoveInReview;

/// <summary>
/// As a move-in coordinator, I want the review form to tell me what is missing as I work, under the
/// same rules the server applies, so that a review I save is one the community accepts.
/// </summary>
/// <remarks>
/// The rules are written once in FluentValidation. The page checks every rule it can; a rule whose
/// condition only the server can decide (a returning resident's previous room) is answered by the
/// server, in the page's validation summary.
/// </remarks>
[TestFixture]
public class WhenCoordinatorReviewsMoveIn : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/MoveInReview";
    private const string Saved = "Move-in review saved for Margaret Hale.";

    private PagePlan<MoveInReviewModel> _plan = null!;
    private ConcurrentQueue<string> _reviewsSent = null!;

    private ILocator SaveReview => Page.Locator("#save-review-btn");
    private ILocator ReviewStatus => Page.Locator("#review-status");
    private ILocator ValidationSummary => Page.Locator("[data-reactive-validation-summary]");

    private async Task NavigateAndBoot()
    {
        _reviewsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Validation/MoveInReview/Save")) _reviewsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<MoveInReviewModel>.FromPage(Page);
    }

    // Leaving the field lets its message settle, so the next click lands where it aims.
    private async Task Write(Expression<Func<MoveInReviewModel, object?>> field, string text)
    {
        await _plan.TextBox(field).Fill(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    [Test]
    public async Task an_empty_review_asks_for_the_resident_and_the_score_before_anything_is_sent()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("'Resident Name' must not be empty.");
        await Expect(_plan.ErrorFor(m => m.FallRiskScore)).ToHaveTextAsync("Score the resident's fall risk.");
        await Expect(ReviewStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_reviewsSent, Is.Empty, "A review the page can see is incomplete is not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_name_too_long_for_the_wristband_is_refused_by_the_server_in_its_own_words()
    {
        const string LongName = "Margaret Elizabeth Hale-Worthington";
        await NavigateAndBoot();
        await Write(m => m.ResidentName, LongName);
        await Write(m => m.FallRiskScore, "2");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.ResidentName))
            .ToHaveTextAsync($"{LongName} is longer than the 30 characters a wristband prints.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "A message written from the model is the server's to say.");
        await Write(m => m.ResidentName, "Margaret Hale");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task a_fall_risk_score_outside_zero_to_ten_is_refused()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "11");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.FallRiskScore)).ToHaveTextAsync("Fall risk is scored 0 to 10.");
        await Write(m => m.FallRiskScore, "4");
        await Expect(_plan.ErrorFor(m => m.FallRiskScore)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "Only the corrected review is sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_high_fall_risk_needs_a_mobility_plan()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "9");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.MobilityNotes)).ToHaveTextAsync("Describe the mobility plan for a high fall risk.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.MobilityNotes, "Walker at all times, two-person assist on stairs");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_low_fall_risk_keeps_the_mobility_plan_short()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "3");
        await Write(m => m.MobilityNotes, "Walker at all times, two-person assist on stairs");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.MobilityNotes)).ToHaveTextAsync("Keep mobility notes under 40 characters.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.MobilityNotes, "Walker at all times");
        await Expect(_plan.ErrorFor(m => m.MobilityNotes)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_family_portal_link_must_be_a_web_address_and_may_be_left_blank()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.FamilyPortalLink, "ftp://portal");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.FamilyPortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        Assert.That(_reviewsSent, Is.Empty);
        await Write(m => m.FamilyPortalLink, "");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        Assert.That(_reviewsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_pharmacy_phone_is_asked_for_in_the_house_format()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await Write(m => m.Pharmacy.Phone, "5550100");

        await ClickWhenStable(SaveReview);

        await Expect(_plan.ErrorFor(m => m.Pharmacy.Phone)).ToHaveTextAsync("Enter the pharmacy phone as 555-0100.");
        Assert.That(_reviewsSent, Is.Empty, "The nested object's rule runs in the page.");
        await Write(m => m.Pharmacy.Phone, "555-0100");
        await Expect(_plan.ErrorFor(m => m.Pharmacy.Phone)).ToBeHiddenAsync();
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_server_asks_for_a_returning_residents_previous_room()
    {
        await NavigateAndBoot();
        await Write(m => m.ResidentName, "Margaret Hale");
        await Write(m => m.FallRiskScore, "2");
        await _plan.NativeCheckBox(m => m.IsReturning).Check();

        await ClickWhenStable(SaveReview);

        await Expect(ValidationSummary).ToBeVisibleAsync();
        await Expect(ValidationSummary).ToContainTextAsync("A returning resident's previous room is required.");
        await Expect(ReviewStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_reviewsSent, Has.Count.EqualTo(1), "The page cannot decide this rule, so the server is asked.");

        await Write(m => m.PreviousRoom, "Maple 214");
        await ClickWhenStable(SaveReview);
        await Expect(ReviewStatus).ToHaveTextAsync(Saved);
        await Expect(ValidationSummary).ToBeHiddenAsync();
        Assert.That(_reviewsSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("400");
    }
}
