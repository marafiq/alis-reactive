using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Dialog;

[TestFixture]
public class WhenCoordinatorReviewsADischarge : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Dialog";

    private ILocator Dialog => Page.Locator("#discharge-dialog");
    private ILocator Summary => Page.Locator("#discharge-summary");
    private ILocator ChooseHint => Page.Locator("#discharge-choose");
    private ILocator ReviewStatus => Page.Locator("#discharge-status");
    private ILocator ReviewDischarge => Page.Locator("#review-discharge");
    private ILocator Blocked => Page.Locator("#discharge-blocked");
    private ILocator Overlay => Page.Locator("#discharge-dialog + .e-dlg-overlay");

    private ILocator ReviewTitle => Dialog.Locator(".e-dlg-header");

    private NativeCheckBoxLocator CareTasksOpen
        => new(Page, IdGenerator.For<DialogModel>(m => m.CareTasksOpen));

    private async Task OpenTheReview()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");
        await ReviewDischarge.ClickAsync();
        await Expect(Summary).ToContainTextAsync("goes home with her daughter", new() { Timeout = 10000 });
    }

    private async Task DragTheReviewAside()
    {
        var title = await ReviewTitle.BoundingBoxAsync();
        var x = title!.X + title.Width / 2;
        var y = title.Y + title.Height / 2;
        await Page.Mouse.MoveAsync(x, y);
        await Page.Mouse.DownAsync();
        await Page.Mouse.MoveAsync(x - 300, y - 200, new() { Steps = 12 });
        await Page.Mouse.UpAsync();
    }

    private async Task<float> ReviewMiddle()
    {
        var box = await Dialog.BoundingBoxAsync();
        return box!.X + box.Width / 2;
    }

    [Test]
    public async Task open_care_tasks_keep_the_discharge_review_from_opening()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");
        await CareTasksOpen.Check();

        await ReviewDischarge.ClickAsync();

        await Expect(Blocked).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Blocked).ToHaveTextAsync("Close Margaret's open care tasks before reviewing her discharge.");
        await Expect(Dialog).ToBeHiddenAsync();
        await Expect(ReviewStatus).ToHaveTextAsync("not started");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_care_tasks_are_closed_the_review_opens_and_the_reason_goes()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");
        await CareTasksOpen.Check();
        await ReviewDischarge.ClickAsync();
        await Expect(Blocked).ToBeVisibleAsync(new() { Timeout = 5000 });
        await CareTasksOpen.Input.UncheckAsync();

        await ReviewDischarge.ClickAsync();

        await Expect(Dialog).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Blocked).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reviewing_the_discharge_opens_the_dialog_with_the_summary()
    {
        await NavigateToAndWaitForVisibleSignal(Path, "#review-discharge");

        await ReviewDischarge.ClickAsync();

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

        await Expect(ChooseHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Dialog).ToBeVisibleAsync();
        await Expect(ReviewStatus).ToHaveTextAsync("in progress");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_outside_the_review_asks_for_a_choice()
    {
        await OpenTheReview();

        await Overlay.ClickAsync(new() { Position = new() { X = 5, Y = 5 } });

        await Expect(ChooseHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Dialog).ToBeVisibleAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clicking_outside_a_review_dragged_aside_brings_it_back_to_the_middle()
    {
        await OpenTheReview();
        var middle = await ReviewMiddle();
        await DragTheReviewAside();
        Assert.That(await ReviewMiddle(), Is.LessThan(middle - 200), "the review was dragged aside");

        await Overlay.ClickAsync(new() { Position = new() { X = 5, Y = 5 } });

        await Expect(ChooseHint).ToBeVisibleAsync(new() { Timeout = 5000 });
        Assert.That(await ReviewMiddle(), Is.EqualTo(middle).Within(2));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task stepping_away_with_not_yet_closes_the_review()
    {
        await OpenTheReview();

        await Page.Locator("#discharge-not-yet").ClickAsync();

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
