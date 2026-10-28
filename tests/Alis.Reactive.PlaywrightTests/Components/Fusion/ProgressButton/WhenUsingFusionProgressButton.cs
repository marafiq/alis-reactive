using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ProgressButton;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.ProgressButton;

/// <summary>
/// As a med-pass nurse handing off the evening shift, I want to send the medication pass to the pharmacy from a
/// button that shows how much of the report has gone, so that I leave the floor knowing the pharmacy has it.
/// </summary>
/// <remarks>
/// The send button stays disabled until the nurse checks off the pass; checking it off readies the button and
/// puts her on it. Lakeside Pharmacy confirms once the six-second upload finishes; Riverside Pharmacy confirms
/// the moment it has the report, which ends the upload there and then; County Pharmacy takes the pass by fax,
/// which has no upload bar. A saved draft that stopped at 60% can be resumed from there or sent again whole.
/// Content and CssClass are proven by what the button shows once the report is sent; nothing on a send page
/// reads its own button's words or colour back, so their reads are not exercised (the page reads the button
/// through Disabled and EnableProgress). Begin always reports 0% after no time, so Percent and CurrentDuration
/// are told apart on a resumed send's first moment (60% after no time), and Step on Begin. A confirmed send is
/// told apart from the upload's own end by time: it ends inside three seconds, the upload takes six.
/// </remarks>
[TestFixture]
public class WhenUsingFusionProgressButton : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/FusionProgressButton";

    // The upload runs six seconds; a pharmacy that confirms on receipt ends it well inside three.
    private const int ConfirmedOnReceiptWithinMs = 3000;
    private const int UploadFinishesWithinMs = 15000;

    private PagePlan<FusionProgressButtonModel> _plan = null!;

    private FusionProgressButtonLocator SendReport => new(Page, "send-report");
    private NativeCheckBoxLocator PassChecked => _plan.NativeCheckBox(m => m.PassChecked);
    private ILocator ResumeDraft => Page.Locator("#resume-draft-btn");
    private ILocator SendItAllAgain => Page.Locator("#send-again-btn");
    private ILocator SendingWhole => Page.Locator("#send-whole");
    private ILocator SendingRest => Page.Locator("#send-rest");
    private ILocator AmountSent => Page.Locator("#send-progress");
    private ILocator Faxing => Page.Locator("#send-faxing");
    private ILocator ReportSent => Page.Locator("#report-sent");
    private ILocator PharmacyReply => Page.Locator("#pharmacy-reply");
    private ILocator DraftWarning => Page.Locator("#draft-warning");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<FusionProgressButtonModel>.FromPage(Page);
        await Expect(SendReport.Content).ToHaveTextAsync("Send to pharmacy", new() { Timeout = 5000 });
    }

    private async Task CheckOffThePass() => await PassChecked.Check();

    private async Task ChoosePharmacy(string pharmacy) => await _plan.NativeDropDown(m => m.Pharmacy).Select(pharmacy);

    [Test]
    public async Task checking_off_the_pass_readies_the_send_button_and_puts_the_nurse_on_it()
    {
        await NavigateAndBoot();
        await Expect(SendReport.Button).ToBeDisabledAsync(new() { Timeout = 5000 });

        await CheckOffThePass();

        await Expect(SendReport.Button).ToBeEnabledAsync(new() { Timeout = 5000 });
        await Expect(SendReport.Button).ToBeFocusedAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task unchecking_the_pass_takes_the_send_away_again()
    {
        await NavigateAndBoot();
        await CheckOffThePass();
        await Expect(SendReport.Button).ToBeEnabledAsync(new() { Timeout = 5000 });

        await PassChecked.Input.UncheckAsync();

        await Expect(SendReport.Button).ToBeDisabledAsync(new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_pass_says_the_whole_report_is_going_and_shows_how_much_has_gone()
    {
        await NavigateAndBoot();
        await CheckOffThePass();

        await ClickWhenStable(SendReport.Button);

        await Expect(SendingWhole).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SendReport.ProgressBar).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(AmountSent).ToHaveTextAsync(new Regex(@"^[1-9]\d?(\.\d+)?% sent$"), new() { Timeout = 5000 });
        await Expect(SendingRest).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_page_says_the_report_is_sent_when_the_upload_finishes()
    {
        await NavigateAndBoot();
        await CheckOffThePass();

        await ClickWhenStable(SendReport.Button);

        await Expect(PharmacyReply).ToHaveTextAsync("Lakeside Pharmacy confirms once the upload finishes.", new() { Timeout = 5000 });
        await Expect(ReportSent).ToBeVisibleAsync(new() { Timeout = UploadFinishesWithinMs });
        await Expect(SendReport.Content).ToHaveTextAsync("Sent");
        await Expect(SendReport.Button).ToHaveClassAsync(new Regex(@"\be-success\b"));
        await Expect(SendReport.Button).Not.ToHaveClassAsync(new Regex(@"\be-primary\b"));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_pharmacy_that_confirms_on_receipt_ends_the_upload_there_and_then()
    {
        await NavigateAndBoot();
        await ChoosePharmacy("Riverside Pharmacy");
        await CheckOffThePass();

        await ClickWhenStable(SendReport.Button);

        await Expect(ReportSent).ToBeVisibleAsync(new() { Timeout = ConfirmedOnReceiptWithinMs });
        await Expect(PharmacyReply).ToHaveTextAsync("Riverside Pharmacy confirmed it has the report.");
        await Expect(SendReport.Content).ToHaveTextAsync("Sent");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task once_the_report_is_sent_the_stopped_draft_is_put_away()
    {
        await NavigateAndBoot();
        await ChoosePharmacy("Riverside Pharmacy");
        await CheckOffThePass();
        await Expect(ResumeDraft).ToBeVisibleAsync();

        await ClickWhenStable(SendReport.Button);

        await Expect(ReportSent).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(ResumeDraft).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_fax_has_no_upload_bar_and_the_page_says_the_report_is_being_faxed()
    {
        await NavigateAndBoot();
        await ChoosePharmacy("County Pharmacy (fax)");
        await CheckOffThePass();

        await ClickWhenStable(SendReport.Button);

        await Expect(Faxing).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SendReport.ProgressBar).ToHaveCountAsync(0);
        await Expect(AmountSent).ToBeHiddenAsync();
        await Expect(PharmacyReply).ToHaveTextAsync("County Pharmacy confirms faxes by phone.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task resuming_the_saved_draft_sends_only_the_rest_of_the_report()
    {
        await NavigateAndBoot();
        await CheckOffThePass();

        await ClickWhenStable(ResumeDraft);

        await Expect(SendingRest).ToHaveTextAsync("Sending the rest of the report: 60% had already reached the pharmacy.", new() { Timeout = 5000 });
        await Expect(SendingWhole).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_the_draft_again_sends_the_whole_report_from_the_start()
    {
        await NavigateAndBoot();
        await CheckOffThePass();

        await ClickWhenStable(SendItAllAgain);

        await Expect(SendingWhole).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SendingRest).ToBeHiddenAsync();
        await Expect(DraftWarning).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_draft_is_not_sent_before_the_pass_is_checked_off()
    {
        await NavigateAndBoot();

        await ClickWhenStable(ResumeDraft);

        await Expect(DraftWarning).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(SendingRest).ToBeHiddenAsync();
        await Expect(SendingWhole).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
