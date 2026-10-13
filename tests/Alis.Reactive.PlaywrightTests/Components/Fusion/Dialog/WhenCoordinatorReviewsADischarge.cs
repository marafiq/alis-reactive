using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Dialog;

/// <summary>
/// As a care coordinator, I review a resident's discharge in a dialog that opens only when no care
/// task is still open, loads the discharge summary when it opens, and closes only on a choice.
/// </summary>
[TestFixture]
public class WhenCoordinatorReviewsADischarge : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Dialog";

    private ILocator Dialog => Page.Locator("#discharge-dialog");
    private ILocator Summary => Page.Locator("#discharge-summary");
    private ILocator ChooseHint => Page.Locator("#discharge-choose");
    private ILocator ReviewStatus => Page.Locator("#discharge-status");
    private ILocator ReviewDischarge => Page.Locator("#review-discharge");

    private NativeCheckBoxLocator CareTasksOpen =>
        new(Page, IdGenerator.For<DialogModel>(m => m.CareTasksOpen));

    private async Task OpenTheReview()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");
        await ReviewDischarge.ClickAsync();
        await Expect(Summary).ToContainTextAsync("goes home with her daughter", new() { Timeout = 10000 });
    }

    [Test]
    public async Task open_care_tasks_keep_the_discharge_review_from_opening()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");
        await CareTasksOpen.Check();

        await ReviewDischarge.ClickAsync();

        // beforeOpen cancels the open and says why; the review never starts.
        await Expect(Page.Locator("#discharge-blocked"))
            .ToHaveTextAsync("Close Margaret's open care tasks before reviewing her discharge.", new() { Timeout = 5000 });
        await Expect(Dialog).ToBeHiddenAsync();
        await Expect(ReviewStatus).ToHaveTextAsync("not started");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_the_discharge_opens_the_dialog_with_the_summary()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");

        await ReviewDischarge.ClickAsync();

        // Show opens the dialog; its open event marks the review in progress and loads the summary.
        await Expect(Dialog).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ReviewStatus).ToHaveTextAsync("in progress", new() { Timeout = 5000 });
        await Expect(Summary).ToContainTextAsync("goes home with her daughter on Friday", new() { Timeout = 10000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task escape_does_not_dismiss_the_review_without_a_choice()
    {
        await OpenTheReview();

        await Page.Keyboard.PressAsync("Escape");

        // beforeClose sees a close the coordinator started and cancels it.
        await Expect(ChooseHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Dialog).ToBeVisibleAsync();
        await Expect(ReviewStatus).ToHaveTextAsync("in progress");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_outside_the_review_asks_for_a_choice()
    {
        await OpenTheReview();

        await Page.Locator("#discharge-dialog + .e-dlg-overlay")
            .ClickAsync(new() { Position = new() { X = 5, Y = 5 } });

        await Expect(ChooseHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Dialog).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task stepping_away_with_not_yet_closes_the_review()
    {
        await OpenTheReview();

        await Page.Locator("#discharge-not-yet").ClickAsync();

        // Hide closes the dialog (a close the page asked for is not cancelled); its close event
        // marks the review closed.
        await Expect(Dialog).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(ReviewStatus).ToHaveTextAsync("closed", new() { Timeout = 5000 });
        await Expect(Page.Locator("#discharge-decision")).ToBeEmptyAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_the_discharge_closes_the_review_and_records_the_decision()
    {
        await OpenTheReview();

        await Page.Locator("#discharge-confirm").ClickAsync();

        await Expect(Dialog).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(Page.Locator("#discharge-decision"))
            .ToHaveTextAsync("Margaret Hale's discharge is confirmed.", new() { Timeout = 5000 });
        await Expect(ReviewStatus).ToHaveTextAsync("closed");
        AssertNoConsoleErrors();
    }
}
