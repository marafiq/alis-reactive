using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Button;

// Journey: a care-team member runs a resident's Daily Wellness Check-In.
// The check-in opens personalised and locked; confirming the resident's identity unlocks it;
// the visit's priority is set (complete icon, urgent style, recommended, follow-up); the action
// can be triggered or focused for them; then the check-in is recorded and the server confirms it.
[TestFixture]
public class WhenUsingFusionButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Button";
    private const string ActionButtonId = "begin-checkin";

    private FusionButtonLocator ActionButton => new(Page, ActionButtonId);

    private ILocator ActionReady => Page.Locator("#action-ready");
    private ILocator Readiness => Page.Locator("#readiness");
    private ILocator PriorityState => Page.Locator("#priority-state");
    private ILocator Confirmation => Page.Locator("#checkin-confirmation");

    private ILocator ConfirmIdentity => Page.Locator("#confirm-identity");
    private ILocator LockVisit => Page.Locator("#lock-visit");
    private ILocator CheckReadiness => Page.Locator("#check-readiness");
    private ILocator MarkComplete => Page.Locator("#mark-complete");
    private ILocator FlagUrgent => Page.Locator("#flag-urgent");
    private ILocator RecommendAction => Page.Locator("#recommend-action");
    private ILocator EnableFollowUp => Page.Locator("#enable-followup");
    private ILocator RemindMe => Page.Locator("#remind-me");
    private ILocator JumpToAction => Page.Locator("#jump-to-action");
    private ILocator RecordCheckIn => Page.Locator("#record-checkin");

    private async Task OpenCheckIn()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(ActionButton.Button).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionButton builder renders the action, and the DomReady SetContent
    // personalises its label for the assigned resident while the Content() source reads that
    // label back into the visible "Action ready" line. The check-in opens locked.
    [Test]
    public async Task the_checkin_opens_personalised_and_ready_to_confirm()
    {
        await OpenCheckIn();

        await Expect(ActionButton.Button).ToHaveTextAsync("Begin check-in for Eleanor Whitfield");
        await Expect(ActionReady).ToHaveTextAsync("Begin check-in for Eleanor Whitfield");
        await Expect(Readiness).ToHaveTextAsync("Confirm the resident's identity to begin.");

        AssertNoConsoleErrors();
    }

    // SetDisabled — confirming the resident's identity unlocks the action; locking the visit
    // disables it again. The visible enabled/disabled state of the action follows each write.
    [Test]
    public async Task confirming_identity_unlocks_the_action_and_locking_disables_it()
    {
        await OpenCheckIn();

        await Expect(ActionButton.Button).ToBeDisabledAsync();

        await ConfirmIdentity.ClickAsync();
        await Expect(ActionButton.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(Readiness).ToHaveTextAsync("Identity confirmed — you can begin the check-in.");

        await LockVisit.ClickAsync();
        await Expect(ActionButton.Button).ToBeDisabledAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // Disabled() read source — the readiness check reads the action's disabled state into a
    // condition: locked shows the "confirm identity" message, ready shows the "you can begin"
    // message. The message follows the branch the Disabled() source routes.
    [Test]
    public async Task checking_readiness_reports_whether_the_visit_is_locked_or_ready()
    {
        await OpenCheckIn();

        await CheckReadiness.ClickAsync();
        await Expect(Readiness)
            .ToHaveTextAsync("This visit is locked. Confirm the resident's identity to begin.",
                new() { Timeout = 5000 });

        await ConfirmIdentity.ClickAsync();
        await CheckReadiness.ClickAsync();
        await Expect(Readiness)
            .ToHaveTextAsync("This visit is ready. You can begin the check-in now.",
                new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // SetIcon — marking the visit complete swaps the action's icon to a check on the right.
    // The icon span carries the new icon CSS and the right-position class.
    [Test]
    public async Task marking_the_visit_complete_shows_a_check_icon_after_the_label()
    {
        await OpenCheckIn();

        await MarkComplete.ClickAsync();

        await Expect(ActionButton.Icon).ToHaveCountAsync(1);
        var iconClasses = await ActionButton.IconClassAttribute();
        Assert.That(iconClasses, Does.Contain("e-check"),
            "SetIcon must apply the new icon CSS to the action");
        Assert.That(iconClasses, Does.Contain("e-icon-right"),
            "SetIcon must place the icon after the label (right position)");

        AssertNoConsoleErrors();
    }

    // SetCssClass — flagging the visit urgent applies the urgent style classes to the action.
    [Test]
    public async Task flagging_the_visit_urgent_restyles_the_action()
    {
        await OpenCheckIn();

        Assert.That(await ActionButton.HasClass("urgent-visit"), Is.False,
            "the action is not urgent before it is flagged");

        await FlagUrgent.ClickAsync();

        Assert.That(await ActionButton.HasClass("e-warning"), Is.True,
            "SetCssClass must apply the warning style to the action");
        Assert.That(await ActionButton.HasClass("urgent-visit"), Is.True,
            "SetCssClass must apply the urgent-visit class to the action");
        await Expect(PriorityState).ToHaveTextAsync("Flagged urgent.");

        AssertNoConsoleErrors();
    }

    // SetPrimary — making the check-in the recommended action promotes the button to Syncfusion's
    // primary styling.
    [Test]
    public async Task recommending_the_checkin_promotes_it_to_the_primary_action()
    {
        await OpenCheckIn();

        Assert.That(await ActionButton.HasClass("e-primary"), Is.False,
            "the action is not primary before it is recommended");

        await RecommendAction.ClickAsync();

        Assert.That(await ActionButton.HasClass("e-primary"), Is.True,
            "SetPrimary must promote the action to primary styling");
        await Expect(PriorityState).ToHaveTextAsync("Set as recommended.");

        AssertNoConsoleErrors();
    }

    // SetToggle + Click() — enabling follow-up tracking turns the action into a toggle; the
    // "Remind me" control then calls Click() on the action, which latches it into the active
    // state. Without SetToggle the click would not latch; without Click() nothing fires.
    [Test]
    public async Task enabling_follow_up_then_reminding_latches_the_action_active()
    {
        await OpenCheckIn();

        await ConfirmIdentity.ClickAsync();
        await Expect(ActionButton.Button).ToBeEnabledAsync(new() { Timeout = 5000 });

        Assert.That(await ActionButton.HasClass("e-active"), Is.False,
            "the action is not latched active before follow-up is enabled and Click() is called");

        await EnableFollowUp.ClickAsync();
        await RemindMe.ClickAsync();

        Assert.That(await ActionButton.HasClass("e-active"), Is.True,
            "SetToggle makes the action a toggle and Click() latches it into the active state");

        AssertNoConsoleErrors();
    }

    // FocusIn() — the "Jump to the action" control moves keyboard focus onto the action button
    // so a care-team member using the keyboard lands on it.
    [Test]
    public async Task jumping_to_the_action_moves_keyboard_focus_onto_it()
    {
        await OpenCheckIn();

        await ConfirmIdentity.ClickAsync();
        await Expect(ActionButton.Button).ToBeEnabledAsync(new() { Timeout = 5000 });

        await Expect(ActionButton.Button).Not.ToBeFocusedAsync();

        await JumpToAction.ClickAsync();

        await Expect(ActionButton.Button).ToBeFocusedAsync(new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    // CssClass(), IsPrimary(), IsToggle() read sources — recording the check-in gathers the
    // action's runtime state and the server confirmation the care-team member sees reflects each:
    // the recommended phrasing (IsPrimary), the follow-up phrasing (IsToggle), and the priority
    // style (CssClass).
    [Test]
    public async Task recording_the_checkin_confirms_its_recommendation_followup_and_priority_style()
    {
        await OpenCheckIn();

        await RecommendAction.ClickAsync();
        await EnableFollowUp.ClickAsync();
        await FlagUrgent.ClickAsync();

        await RecordCheckIn.ClickAsync();

        await Expect(Confirmation)
            .ToHaveTextAsync(
                "Recorded \"Begin check-in for Eleanor Whitfield\" as the recommended next step with a follow-up flagged. Priority style: e-warning urgent-visit.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // GATHERS — the framework gather pipeline carries the action's runtime sources into the POST
    // body under their declared keys. (Framework gather test: asserts request.PostData.)
    [Test]
    public async Task recording_the_checkin_posts_the_action_state_to_the_server()
    {
        await OpenCheckIn();

        await RecommendAction.ClickAsync();
        await EnableFollowUp.ClickAsync();
        await FlagUrgent.ClickAsync();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/Button/RecordCheckIn") && request.Method == "POST",
            new() { Timeout = 10000 });

        await RecordCheckIn.ClickAsync();

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"action\":\"Begin check-in for Eleanor Whitfield\""),
            "the gather pipeline must carry the Content() source under its declared key");
        Assert.That(body, Does.Contain("\"priority\":\"e-warning urgent-visit\""),
            "the gather pipeline must carry the CssClass() source under its declared key");
        Assert.That(body, Does.Contain("\"recommended\":true"),
            "the gather pipeline must carry the IsPrimary() source under its declared key");
        Assert.That(body, Does.Contain("\"followUp\":true"),
            "the gather pipeline must carry the IsToggle() source under its declared key");

        AssertNoConsoleErrors();
    }
}
