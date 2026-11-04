using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.CheckBox;

[TestFixture]
public class WhenUsingFusionCheckBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionCheckBox";
    private static readonly string AgreementId = IdGenerator.For<FusionCheckBoxModel>(m => m.AgreementAccepted);
    private static readonly string HousekeepingId = IdGenerator.For<FusionCheckBoxModel>(m => m.WeeklyHousekeeping);

    private FusionCheckBoxLocator Agreement => new(Page, AgreementId);
    private FusionCheckBoxLocator Housekeeping => new(Page, HousekeepingId);

    private ILocator AgreementStatus => Page.Locator("#agreement-status");
    private ILocator HousekeepingStatus => Page.Locator("#housekeeping-status");
    private ILocator SaveConfirmation => Page.Locator("#save-confirmation");
    private ILocator RecommendButton => Page.Locator("#recommend-housekeeping");
    private ILocator FlagFollowUpButton => Page.Locator("#flag-housekeeping-followup");
    private ILocator ToggleButton => Page.Locator("#toggle-housekeeping");
    private ILocator JumpToAgreementButton => Page.Locator("#jump-to-agreement");
    private ILocator SaveButton => Page.Locator("#save-agreement");

    private async Task OpenMoveInForm()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Agreement.Frame).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    private async Task AcceptAgreement()
    {
        await Agreement.Toggle();
        await Expect(AgreementStatus).ToHaveTextAsync(
            "Thank you. Your residency agreement is accepted.", new() { Timeout = 10000 });
    }

    [Test]
    public async Task move_in_form_opens_with_optional_services_locked_until_the_agreement_is_accepted()
    {
        await OpenMoveInForm();

        Assert.That(await Agreement.IsChecked(), Is.False);
        Assert.That(await Housekeeping.IsDisabled(), Is.True);
        await Expect(HousekeepingStatus)
            .ToHaveTextAsync("Locked until you accept the residency agreement.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task accepting_the_residency_agreement_unlocks_the_optional_services()
    {
        await OpenMoveInForm();
        Assert.That(await Housekeeping.IsDisabled(), Is.True);

        await AcceptAgreement();

        Assert.That(await Housekeeping.IsDisabled(), Is.False);
        await Expect(HousekeepingStatus)
            .ToHaveTextAsync("You can now add weekly housekeeping to your move-in services.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_agreement_message_follows_whether_the_resident_accepts_or_declines()
    {
        await OpenMoveInForm();

        await Agreement.Toggle();
        await Expect(AgreementStatus)
            .ToHaveTextAsync("Thank you. Your residency agreement is accepted.");

        await Agreement.Toggle();
        await Expect(AgreementStatus)
            .ToHaveTextAsync("Please accept the residency agreement to continue.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task adding_recommended_housekeeping_checks_the_box_for_the_resident()
    {
        await OpenMoveInForm();
        await AcceptAgreement();
        Assert.That(await Housekeeping.IsChecked(), Is.False);

        await RecommendButton.ClickAsync();

        Assert.That(await Housekeeping.IsChecked(), Is.True);
        await Expect(HousekeepingStatus)
            .ToHaveTextAsync("Weekly housekeeping will be included in your move-in services.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task flagging_housekeeping_for_follow_up_marks_it_undecided()
    {
        await OpenMoveInForm();
        await AcceptAgreement();

        await FlagFollowUpButton.ClickAsync();

        Assert.That(await Housekeeping.IsIndeterminate(), Is.True);
        await Expect(HousekeepingStatus)
            .ToHaveTextAsync("A coordinator will follow up with you about weekly housekeeping.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task toggling_housekeeping_for_the_resident_checks_the_box()
    {
        await OpenMoveInForm();
        await AcceptAgreement();
        Assert.That(await Housekeeping.IsChecked(), Is.False);

        await ToggleButton.ClickAsync();

        Assert.That(await Housekeeping.IsChecked(), Is.True);
        await Expect(HousekeepingStatus)
            .ToHaveTextAsync("Weekly housekeeping will be included in your move-in services.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task jumping_back_to_the_agreement_focuses_the_agreement_checkbox()
    {
        await OpenMoveInForm();
        await AcceptAgreement();

        await JumpToAgreementButton.ClickAsync();

        await Expect(Agreement.Input).ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_agreement_confirms_the_chosen_services()
    {
        await OpenMoveInForm();
        await AcceptAgreement();
        await RecommendButton.ClickAsync();

        await SaveButton.ClickAsync();

        await Expect(SaveConfirmation)
            .ToHaveTextAsync("Agreement saved. Weekly housekeeping is included in your move-in services.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_posts_the_agreement_and_service_elections_to_the_server()
    {
        await OpenMoveInForm();
        await AcceptAgreement();
        await FlagFollowUpButton.ClickAsync();
        Assert.That(await Housekeeping.IsIndeterminate(), Is.True);

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/FusionCheckBox/SaveAgreement") && request.Method == "POST",
            new() { Timeout = 10000 });

        await SaveButton.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"AgreementAccepted\":true"),
            "the gather must carry the agreement Checked() source under its declared key");
        Assert.That(body, Does.Contain("\"HousekeepingNeedsFollowUp\":true"),
            "the gather must carry the housekeeping Indeterminate() source under its declared key");

        AssertNoConsoleErrors();
    }
}
