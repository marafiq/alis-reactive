using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.HttpPipeline.HospitalTransfer;

/// <summary>
/// As a nurse, I want sending a resident to the hospital to tell the transfer desk everything on the form
/// and to tell the family where the resident is going, so that the hospital expects the resident and the
/// family hears it from us first.
/// </summary>
/// <remarks>
/// The whole form goes to the transfer desk as one JSON body: the resident, the reason, how they travel,
/// the hospital and the ward asked for, and two escort lines. The page asks for the resident and the
/// reason before anything is sent. The desk answers by resident: Helen Park (1042) is accepted, with the
/// ward she is received on and what to pack; Arthur Bell (1043) is already at the hospital; Walter Cho
/// (1045) was discharged this morning; Frank Moore (1046) needs the ambulance dispatcher, who is offline;
/// Rosa Diaz (1044) makes the desk fail with no answer at all. Mercy General has no bed tonight, which
/// only the desk knows, so that refusal shows beside the hospital field. The loader covers the page while
/// the desk is asked and lifts once it has answered, however it answered; only then is the family told,
/// from the hospital's own answer.
/// </remarks>
[TestFixture]
public class WhenNurseSendsResidentToHospital : PlaywrightTestBase
{
    private const string Path = "/Sandbox/HttpPipeline/HospitalTransfer";
    private const string TransferUrl = "**/Sandbox/HttpPipeline/HospitalTransfer/Transfer";
    private const string NotifyUrl = "**/Sandbox/HttpPipeline/HospitalTransfer/NotifyFamily";
    private const string NoTransfer = "No transfer sent.";
    private const string FamilyNotTold = "The family has not been told.";
    private const string HelensTransfer =
        "Helen Park to St. Luke's (Orthopedics requested): by ambulance, escorted by Nora Ellis and Malik Stone. Reason: Fall with hip pain.";
    private const string DeskUnreachable = "The transfer desk did not answer; call it on 555-0100.";

    private static readonly Regex Warning = new(@"\btext-danger\b");

    private PagePlan<HospitalTransferModel> _plan = null!;
    private ConcurrentQueue<IRequest> _transfers = null!;
    private ConcurrentQueue<IRequest> _notices = null!;

    private ILocator Field(Expression<Func<HospitalTransferModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");
    private NativeCheckListLocator Packing => new(Page, IdGenerator.For<HospitalTransferModel, object?>(m => m.Packing));
    private ILocator SendToHospital => Page.Locator("#send-transfer-btn");
    private ILocator Status => Page.Locator("#transfer-status");
    private ILocator NextStep => Page.Locator("#transfer-next");
    private ILocator Ward => Page.Locator("#transfer-ward");
    private ILocator PackingCount => Page.Locator("#packing-count");
    private ILocator FamilyNotice => Page.Locator("#family-notice");
    private ILocator Loader => Page.Locator("#alis-loader");

    private async Task OpenTransfer()
    {
        _transfers = new ConcurrentQueue<IRequest>();
        _notices = new ConcurrentQueue<IRequest>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/HospitalTransfer/Transfer")) _transfers.Enqueue(request);
            if (request.Url.EndsWith("/HospitalTransfer/NotifyFamily")) _notices.Enqueue(request);
        };
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<HospitalTransferModel>.FromPage(Page);
    }

    // The request is held at the browser until the test releases it, then sent on unchanged, so what the
    // page shows before the answer arrives can be seen.
    private async Task<SemaphoreSlim> HoldUntilReleased(string url)
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(url, async route =>
        {
            await released.WaitAsync();
            await route.ContinueAsync();
        });
        return released;
    }

    private async Task Describe(
        string resident,
        string hospital = "St. Luke's",
        bool ambulance = true,
        string? secondEscort = "Malik Stone")
    {
        await _plan.NativeDropDown(m => m.ResidentId).Select(resident);
        await Field(m => m.Reason).FillAsync("Fall with hip pain.");
        if (ambulance) await _plan.NativeCheckBox(m => m.Ambulance).Check();
        await _plan.TextBox(m => m.Destination.Hospital).Fill(hospital);
        await _plan.TextBox(m => m.Destination.Ward).Fill("Orthopedics");
        await Field(m => m.Escorts[0].Name).FillAsync("Nora Ellis");
        if (secondEscort != null) await Field(m => m.Escorts[1].Name).FillAsync(secondEscort);
    }

    [Test]
    public async Task sending_a_resident_to_the_hospital_tells_the_desk_everything_on_the_form()
    {
        await OpenTransfer();
        await Describe("Helen Park");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync(HelensTransfer);
        await Expect(Status).Not.ToHaveClassAsync(Warning);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_hospitals_answer_names_the_ward_and_ticks_what_to_pack()
    {
        await OpenTransfer();
        await Describe("Helen Park");

        await ClickWhenStable(SendToHospital);

        await Expect(Ward).ToHaveTextAsync("4B");
        await Expect(Packing.Box("Medication list")).ToBeCheckedAsync();
        await Expect(Packing.Box("Face sheet")).ToBeCheckedAsync();
        await Expect(Packing.Box("Glasses")).ToBeCheckedAsync();
        await Expect(Packing.Box("Hearing aids")).Not.ToBeCheckedAsync();
        await Expect(PackingCount).ToHaveTextAsync("3");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_going_by_wheelchair_van_with_one_escort_packs_no_glasses()
    {
        await OpenTransfer();
        await Describe("Helen Park", ambulance: false, secondEscort: null);

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync(
            "Helen Park to St. Luke's (Orthopedics requested): by wheelchair van, escorted by Nora Ellis. Reason: Fall with hip pain.");
        await Expect(Packing.Box("Glasses")).Not.ToBeCheckedAsync();
        await Expect(Packing.Box("Medication list")).ToBeCheckedAsync();
        await Expect(PackingCount).ToHaveTextAsync("2");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_page_asks_for_the_resident_and_the_reason_before_anything_is_sent()
    {
        await OpenTransfer();
        await _plan.TextBox(m => m.Destination.Hospital).Fill("St. Luke's");

        await ClickWhenStable(SendToHospital);

        await Expect(_plan.ErrorFor(m => m.ResidentId)).ToHaveTextAsync("Choose the resident who is going.");
        await Expect(_plan.ErrorFor(m => m.Reason)).ToHaveTextAsync("Say why the resident is going to the hospital.");
        await Expect(Status).ToHaveTextAsync(NoTransfer);
        await Expect(Loader).ToBeHiddenAsync();
        Assert.That(_transfers, Is.Empty, "A form the page can see is incomplete is not sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task naming_the_resident_and_the_reason_after_the_block_sends_the_transfer()
    {
        await OpenTransfer();
        await ClickWhenStable(SendToHospital);
        await Expect(_plan.ErrorFor(m => m.Reason)).ToBeVisibleAsync();

        await Describe("Helen Park");
        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync(HelensTransfer);
        await Expect(_plan.ErrorFor(m => m.Reason)).ToBeHiddenAsync();
        Assert.That(_transfers, Has.Count.EqualTo(1), "Only the corrected form reaches the desk.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_hospital_with_no_bed_is_refused_beside_the_hospital_field()
    {
        await OpenTransfer();
        await Describe("Helen Park", hospital: "Mercy General");

        await ClickWhenStable(SendToHospital);

        await Expect(_plan.ErrorFor(m => m.Destination.Hospital)).ToHaveTextAsync("Mercy General has no bed tonight; name another hospital.");
        await Expect(Status).ToHaveTextAsync(NoTransfer);
        await Expect(FamilyNotice).ToHaveTextAsync(FamilyNotTold);
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    [Test]
    public async Task a_resident_already_at_the_hospital_is_refused_with_a_call_to_make()
    {
        await OpenTransfer();
        await Describe("Arthur Bell");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync("Arthur Bell is already at St. Luke's since this morning.");
        await Expect(Status).ToHaveClassAsync(Warning);
        await Expect(NextStep).ToHaveTextAsync("Call St. Luke's to confirm the earlier transfer.");
        await Expect(FamilyNotice).ToHaveTextAsync(FamilyNotTold);
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task a_resident_discharged_this_morning_is_refused_with_a_register_update()
    {
        await OpenTransfer();
        await Describe("Walter Cho");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync("Walter Cho was discharged this morning and is no longer on the floor.");
        await Expect(NextStep).ToHaveTextAsync("Update the register before the next transfer.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }

    [Test]
    public async Task an_offline_ambulance_dispatcher_is_reported_with_the_number_to_call()
    {
        await OpenTransfer();
        await Describe("Frank Moore");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync("The ambulance dispatcher is offline until 6 AM.");
        await Expect(NextStep).ToHaveTextAsync("Call the dispatcher on 555-0199.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 503");
    }

    [Test]
    public async Task a_transfer_desk_that_fails_tells_me_to_call_it()
    {
        await OpenTransfer();
        await Describe("Rosa Diaz");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync(DeskUnreachable);
        await Expect(Status).ToHaveClassAsync(Warning);
        await Expect(NextStep).ToHaveTextAsync("");
        await Expect(FamilyNotice).ToHaveTextAsync(FamilyNotTold);
        AssertNoConsoleErrorsExcept("the server responded with a status of 500");
    }

    [Test]
    public async Task a_transfer_desk_that_cannot_be_reached_tells_me_to_call_it_and_the_loader_lifts()
    {
        await Page.RouteAsync(TransferUrl, route => route.AbortAsync("connectionrefused"));
        await OpenTransfer();
        await Describe("Helen Park");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync(DeskUnreachable);
        await Expect(Loader).ToBeAttachedAsync();
        await Expect(Loader).ToBeHiddenAsync();
        await Expect(FamilyNotice).ToHaveTextAsync(FamilyNotTold);
        AssertNoConsoleErrorsExcept("net::ERR_CONNECTION_REFUSED", "[alis:http] fetch.network-error");
    }

    [Test]
    public async Task the_page_is_covered_while_the_desk_is_asked_and_uncovered_once_it_answers()
    {
        var deskAnswers = await HoldUntilReleased(TransferUrl);
        await OpenTransfer();
        await Describe("Helen Park");

        await ClickWhenStable(SendToHospital);

        await Expect(Loader).ToBeVisibleAsync();
        await Expect(Status).ToHaveTextAsync(NoTransfer);
        deskAnswers.Release();
        await Expect(Status).ToHaveTextAsync(HelensTransfer);
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_loader_lifts_when_the_desk_refuses_the_transfer()
    {
        var deskAnswers = await HoldUntilReleased(TransferUrl);
        await OpenTransfer();
        await Describe("Arthur Bell");

        await ClickWhenStable(SendToHospital);

        await Expect(Loader).ToBeVisibleAsync();
        deskAnswers.Release();
        await Expect(Status).ToHaveTextAsync("Arthur Bell is already at St. Luke's since this morning.");
        await Expect(Loader).ToBeHiddenAsync();
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    // The notice carries the transfer's number and the hospital's whole answer, both read from that
    // answer; the family desk composes its message from what it received.
    [Test]
    public async Task the_family_is_told_where_the_resident_is_going_from_the_hospitals_answer()
    {
        await OpenTransfer();
        await Describe("Helen Park");

        await ClickWhenStable(SendToHospital);

        await Expect(FamilyNotice).ToHaveTextAsync("Grace Park was told: Helen Park is going to St. Luke's, ward 4B (transfer T-1042-7).");
        await Expect(Status).ToHaveTextAsync(HelensTransfer);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_loader_lifts_before_the_family_is_told()
    {
        var familyDeskAnswers = await HoldUntilReleased(NotifyUrl);
        await OpenTransfer();
        await Describe("Helen Park");

        await ClickWhenStable(SendToHospital);

        await Expect(Status).ToHaveTextAsync(HelensTransfer);
        await Expect(Loader).ToBeHiddenAsync(new() { Timeout = 5000 });
        await Expect(FamilyNotice).ToHaveTextAsync(FamilyNotTold);
        familyDeskAnswers.Release();
        await Expect(FamilyNotice).ToHaveTextAsync("Grace Park was told: Helen Park is going to St. Luke's, ward 4B (transfer T-1042-7).");
        AssertNoConsoleErrors();
    }

    // Helen Park's notice reaches the family desk after any notice Arthur Bell's refusal could have sent,
    // so once her family line shows its answer, the only notice sent is hers.
    [Test]
    public async Task a_refused_transfer_tells_the_family_nothing()
    {
        await OpenTransfer();
        await Describe("Arthur Bell");
        await ClickWhenStable(SendToHospital);
        await Expect(Status).ToHaveTextAsync("Arthur Bell is already at St. Luke's since this morning.");

        await _plan.NativeDropDown(m => m.ResidentId).Select("Helen Park");
        await ClickWhenStable(SendToHospital);

        await Expect(FamilyNotice).ToHaveTextAsync("Grace Park was told: Helen Park is going to St. Luke's, ward 4B (transfer T-1042-7).");
        Assert.That(_notices, Has.Count.EqualTo(1), "A refused transfer sends the family no notice.");
        Assert.That(_notices.Single().PostData, Does.Contain("T-1042-7"));
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }
}
