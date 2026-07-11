using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.CheckBox;

// Journey: a new resident completes their Move-In Services Agreement with a move-in
// coordinator. Accepting the residency agreement unlocks the optional services; the
// coordinator can pre-select a recommended service, mark one for follow-up when the
// resident is undecided, or toggle one on the resident's behalf, then save the elections.
[TestFixture]
public class WhenUsingFusionCheckBox : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionCheckBox";
    private const string GeneratedTypeScope = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_FusionCheckBoxModel";
    private const string AgreementId = GeneratedTypeScope + "__AgreementAccepted";
    private const string HousekeepingId = GeneratedTypeScope + "__WeeklyHousekeeping";

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

    // RENDERS — the FusionCheckBox builder renders both checkboxes, and on load the
    // optional service is disabled (SetDisabled(true)) with a locked message read from
    // its Disabled() source. If the builder stops rendering, or the DomReady lock breaks,
    // the housekeeping box would be enabled and the locked message would be wrong.
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

    // INTERACTS — accepting the agreement fires the Changed event through the .Reactive
    // wiring; the FusionCheckBoxChangeArgs payload reaches the plan and the accepted
    // branch enables housekeeping. If Changed never fires, Reactive is unwired, or the
    // payload is not delivered, the box stays disabled and the unlocked message never shows.
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

    // FusionCheckBoxChangeArgs.Checked carries the state after each change: accepting
    // routes the Truthy branch, un-accepting routes the Else branch. If Checked stopped
    // carrying the new state, the message could not follow the box both ways.
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

    // SetChecked — the coordinator pre-selects the recommended service, writing the
    // checked state onto the box. If SetChecked stopped writing, the box would stay
    // unchecked after the click.
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

    // SetIndeterminate + Indeterminate() — when the resident is undecided the coordinator
    // marks housekeeping for follow-up (SetIndeterminate(true)), the box shows the EJ2
    // indeterminate dash, and the follow-up message is read from the Indeterminate()
    // source. If SetIndeterminate stopped writing, or Indeterminate() stopped reading, the
    // dash and the follow-up message would not appear.
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

    // Click() — the coordinator toggles housekeeping on the resident's behalf by invoking
    // the rendered checkbox click; the box becomes checked. If Click stopped invoking the
    // component, the box would not change.
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

    // FocusIn() — "Take me back to the agreement" moves focus into the agreement checkbox.
    // If FocusIn stopped moving focus, the agreement input would not be the focused element.
    [Test]
    public async Task jumping_back_to_the_agreement_focuses_the_agreement_checkbox()
    {
        await OpenMoveInForm();
        await AcceptAgreement();

        await JumpToAgreementButton.ClickAsync();

        await Expect(Agreement.Input).ToBeFocusedAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // SUBMITS — saving the agreement posts the elections; the resident sees the server's
    // confirmation reflecting the chosen services. If the response handler stopped revealing
    // the confirmation, the resident would see no acknowledgement after saving.
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

    // GATHERS — the framework gather pipeline carries the Checked() and Indeterminate()
    // sources into the POST body under their declared keys. (Framework gather test: asserts
    // request.PostData.) If Checked() or Indeterminate() stopped yielding into the gather,
    // the saved body would not carry the resident's elections.
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

        Assert.That(body, Does.Contain("\"agreementAccepted\":true"),
            "the gather must carry the agreement Checked() source under its declared key");
        Assert.That(body, Does.Contain("\"housekeepingNeedsFollowUp\":true"),
            "the gather must carry the housekeeping Indeterminate() source under its declared key");

        AssertNoConsoleErrors();
    }
}
