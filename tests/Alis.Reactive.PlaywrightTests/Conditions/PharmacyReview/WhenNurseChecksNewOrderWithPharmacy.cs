using System.Text.RegularExpressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.PharmacyReview;

namespace Alis.Reactive.PlaywrightTests.Conditions.PharmacyReview;

/// <summary>
/// As a charge nurse taking a physician's new medication order, I want the pharmacy's review to tell me
/// whether to give the medication, hold it or stop it, and to page the physician only for a major
/// interaction, so that no resident is given what the pharmacy flagged and no physician is paged for a
/// safe order.
/// </summary>
/// <remarks>
/// Riverside Pharmacy answers by resident and medication: Helen Park takes warfarin (aspirin is a major
/// interaction, 7 of 10; furosemide is clear), Arthur Bell takes lisinopril (potassium chloride is a
/// moderate interaction, 6 of 10; enalapril a duplicate, 8 of 10), Walter Cho takes digoxin
/// (clarithromycin is a major interaction, 9 of 10, and his physician's pager is down), Rosa Diaz is
/// allergic to penicillin (amoxicillin is refused, 409), and oxycodone is refused for anyone without a
/// signed prescription (422). Choosing a resident shows what the pharmacy has on file for them, and typing
/// a medication looks it up in the formulary, by generic or brand name. The MAR line is written after the
/// pharmacy's answer and any physician page have finished, so once it shows its answer every other line is
/// final; the tests read it first. Everything the page renders is exercised.
/// </remarks>
[TestFixture]
public class WhenNurseChecksNewOrderWithPharmacy : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Conditions/PharmacyReview";
    private const string Held = "Held: not given until the hold is lifted.";
    private const string Ready = "Ready: on the MAR for the next medication pass.";
    private const string NoPageNeeded = "No physician page needed.";
    private const string AskFirst = "Choose the resident and type the medication first.";

    private PagePlan<PharmacyReviewModel> _plan = null!;

    private NativeDropDownLocator Resident => _plan.NativeDropDown(m => m.ResidentId);
    private NativeTextBoxLocator Medication => _plan.TextBox(m => m.Medication);
    private NativeCheckBoxLocator HoldOrder => _plan.NativeCheckBox(m => m.HoldOrder);
    private ILocator CheckWithPharmacy => Page.Locator("#check-order-btn");
    private ILocator Finding => Page.Locator("#review-finding");
    private ILocator Detail => Page.Locator("#review-detail");
    private ILocator Mar => Page.Locator("#mar-status");
    private ILocator Physician => Page.Locator("#physician-page");
    private ILocator Profile => Page.Locator("#resident-profile");
    private ILocator FormularyNote => Page.Locator("#formulary-note");
    private ILocator FormularyDispensed => Page.Locator("#formulary-dispensed");
    private ILocator FormularyForm => Page.Locator("#formulary-form");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<PharmacyReviewModel>.FromPage(Page);
    }

    private async Task Order(string resident, string medication)
    {
        await Resident.Select(resident);
        await Medication.FillAndBlur(medication);
    }

    private async Task CheckOrder(string resident, string medication)
    {
        await Order(resident, medication);
        await ClickWhenStable(CheckWithPharmacy);
    }

    // Holds every request to the given address until the returned semaphore is released.
    private async Task<SemaphoreSlim> HoldRequests(string glob)
    {
        var released = new SemaphoreSlim(0);
        await Page.RouteAsync(glob, async route =>
        {
            await released.WaitAsync(TimeSpan.FromSeconds(30));
            await route.ContinueAsync();
        });
        return released;
    }

    [Test]
    public async Task a_major_interaction_holds_the_order_and_pages_the_physician_with_the_pharmacys_words()
    {
        await NavigateAndBoot();

        await CheckOrder("Helen Park, Maple 214", "aspirin");

        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Interaction found");
        await Expect(Detail).ToHaveTextAsync("Major interaction (7 of 10): aspirin with warfarin raises the risk of bleeding.");
        await Expect(HoldOrder.Input).ToBeCheckedAsync();
        await Expect(Physician).ToHaveTextAsync(
            "Dr. Okafor was paged about Helen Park: Major interaction (7 of 10): aspirin with warfarin raises the risk of bleeding.");
        AssertNoConsoleErrors();
    }

    // Six of ten is below a major interaction: the page decision needs both the interaction and its severity.
    [Test]
    public async Task a_moderate_interaction_holds_the_order_without_paging_the_physician()
    {
        await NavigateAndBoot();

        await CheckOrder("Arthur Bell, Maple 216", "potassium chloride");

        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Interaction found");
        await Expect(Detail).ToHaveTextAsync(
            "Moderate interaction (6 of 10): potassium with lisinopril can raise potassium; check levels in three days.");
        await Expect(Physician).ToHaveTextAsync(NoPageNeeded);
        AssertNoConsoleErrors();
    }

    // Eight of ten, but a duplicate is not an interaction: no page.
    [Test]
    public async Task a_duplicate_therapy_holds_the_order_without_paging_the_physician()
    {
        await NavigateAndBoot();

        await CheckOrder("Arthur Bell, Maple 216", "enalapril");

        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Duplicate therapy");
        await Expect(Detail).ToHaveTextAsync("Arthur Bell already takes lisinopril, another ACE inhibitor.");
        await Expect(HoldOrder.Input).ToBeCheckedAsync();
        await Expect(Physician).ToHaveTextAsync(NoPageNeeded);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_order_with_no_concerns_goes_on_the_mar_for_the_next_pass()
    {
        await NavigateAndBoot();

        await CheckOrder("Helen Park, Maple 214", "furosemide");

        await Expect(Mar).ToHaveTextAsync(Ready, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("No concerns");
        await Expect(Detail).ToHaveTextAsync("No interactions with Helen Park's current medications.");
        await Expect(HoldOrder.Input).Not.ToBeCheckedAsync();
        await Expect(Physician).ToHaveTextAsync(NoPageNeeded);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task rechecking_a_held_order_that_comes_back_clear_releases_the_hold()
    {
        await NavigateAndBoot();
        await CheckOrder("Helen Park, Maple 214", "aspirin");
        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Physician).ToHaveTextAsync(new Regex("^Dr. Okafor was paged"));

        await Medication.FillAndBlur("furosemide");
        await ClickWhenStable(CheckWithPharmacy);

        await Expect(Mar).ToHaveTextAsync(Ready, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("No concerns");
        await Expect(HoldOrder.Input).Not.ToBeCheckedAsync();
        await Expect(Physician).ToHaveTextAsync(NoPageNeeded);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_order_is_not_put_on_the_mar_until_the_pharmacy_answers()
    {
        await NavigateAndBoot();
        await Order("Helen Park, Maple 214", "furosemide");
        var pharmacyAnswers = await HoldRequests("**/Sandbox/Conditions/PharmacyReview/Residents/*/Review");

        await ClickWhenStable(CheckWithPharmacy);

        await Expect(Finding).ToHaveTextAsync("Asking Riverside Pharmacy...");
        await Expect(Mar).ToHaveTextAsync("Not on the MAR yet.");
        pharmacyAnswers.Release();
        await Expect(Mar).ToHaveTextAsync(Ready, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("No concerns");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_order_is_not_put_on_the_mar_until_the_physician_page_goes_out()
    {
        await NavigateAndBoot();
        await Order("Helen Park, Maple 214", "aspirin");
        var pageGoesOut = await HoldRequests("**/Sandbox/Conditions/PharmacyReview/PagePhysician");

        await ClickWhenStable(CheckWithPharmacy);

        await Expect(Physician).ToHaveTextAsync("Paging the physician...", new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Interaction found");
        await Expect(Mar).ToHaveTextAsync("Not on the MAR yet.");
        pageGoesOut.Release();
        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Physician).ToHaveTextAsync(
            "Dr. Okafor was paged about Helen Park: Major interaction (7 of 10): aspirin with warfarin raises the risk of bleeding.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_allergy_the_pharmacy_refuses_stops_the_order_with_its_reason()
    {
        await NavigateAndBoot();

        await CheckOrder("Rosa Diaz, Maple 218", "amoxicillin");

        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Allergy: do not give");
        await Expect(Detail).ToHaveTextAsync("Rosa Diaz is allergic to penicillin (hives, 2019); amoxicillin is a penicillin.");
        await Expect(HoldOrder.Input).ToBeCheckedAsync();
        await Expect(Physician).ToHaveTextAsync("Call the physician about this order.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 409");
    }

    [Test]
    public async Task an_order_the_pharmacy_cannot_review_waits_with_its_reason()
    {
        await NavigateAndBoot();

        await CheckOrder("Rosa Diaz, Maple 218", "oxycodone");

        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Not reviewed");
        await Expect(Detail).ToHaveTextAsync(
            "Oxycodone is a controlled substance: the pharmacy needs the physician's signed prescription.");
        await Expect(Physician).ToHaveTextAsync("Call the physician about this order.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 422");
    }

    [Test]
    public async Task when_the_physicians_pager_is_down_the_nurse_is_told_to_call()
    {
        await NavigateAndBoot();

        await CheckOrder("Walter Cho, Maple 220", "clarithromycin");

        await Expect(Mar).ToHaveTextAsync(Held, new() { Timeout = 5000 });
        await Expect(Finding).ToHaveTextAsync("Interaction found");
        await Expect(Physician).ToHaveTextAsync("The page did not go through: call the physician directly.");
        AssertNoConsoleErrorsExcept("the server responded with a status of 503");
    }

    [Test]
    public async Task checking_without_a_medication_asks_for_it_instead_of_the_pharmacy()
    {
        await NavigateAndBoot();
        await Resident.Select("Helen Park, Maple 214");

        await ClickWhenStable(CheckWithPharmacy);

        await Expect(Finding).ToHaveTextAsync(AskFirst);
        await Expect(Mar).ToHaveTextAsync("Not on the MAR yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task checking_without_a_resident_asks_for_one_instead_of_the_pharmacy()
    {
        await NavigateAndBoot();
        await Medication.FillAndBlur("aspirin");

        await ClickWhenStable(CheckWithPharmacy);

        await Expect(Finding).ToHaveTextAsync(AskFirst);
        await Expect(Mar).ToHaveTextAsync("Not on the MAR yet.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task holding_a_clear_order_by_hand_keeps_it_off_the_mar()
    {
        await NavigateAndBoot();
        await CheckOrder("Helen Park, Maple 214", "furosemide");
        await Expect(Mar).ToHaveTextAsync(Ready, new() { Timeout = 5000 });

        await HoldOrder.Check();

        await Expect(Mar).ToHaveTextAsync(Held);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_resident_shows_what_the_pharmacy_has_on_file_for_them()
    {
        await NavigateAndBoot();
        await Resident.Select("Rosa Diaz, Maple 218");
        await Expect(Profile).ToHaveTextAsync("Takes no regular medications. Allergic to penicillin.", new() { Timeout = 5000 });

        await Resident.Select("Helen Park, Maple 214");

        await Expect(Profile).ToHaveTextAsync("Takes warfarin 5 mg daily. No known allergies.", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_no_resident_clears_the_pharmacy_profile()
    {
        await NavigateAndBoot();
        await Resident.Select("Helen Park, Maple 214");
        await Expect(Profile).ToHaveTextAsync("Takes warfarin 5 mg daily. No known allergies.", new() { Timeout = 5000 });

        await Resident.Select("-- Choose a resident --");

        await Expect(Profile).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_generic_name_on_the_formulary_is_confirmed_with_its_form()
    {
        await NavigateAndBoot();

        await Medication.FillAndBlur("furosemide");

        await Expect(FormularyNote).ToHaveTextAsync("On the formulary:", new() { Timeout = 5000 });
        await Expect(FormularyDispensed).ToHaveTextAsync("furosemide");
        await Expect(FormularyForm).ToHaveTextAsync("20 mg tablet");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_brand_name_shows_the_generic_the_pharmacy_dispenses()
    {
        await NavigateAndBoot();

        await Medication.FillAndBlur("Lasix");

        await Expect(FormularyNote).ToHaveTextAsync("The pharmacy dispenses it as:", new() { Timeout = 5000 });
        await Expect(FormularyDispensed).ToHaveTextAsync("furosemide");
        await Expect(FormularyForm).ToHaveTextAsync("20 mg tablet");
        AssertNoConsoleErrors();
    }

    // Soma has four letters, the shortest name the page treats as a whole medication name.
    [Test]
    public async Task a_name_the_formulary_lacks_is_ordered_specially_and_shows_no_earlier_medication()
    {
        await NavigateAndBoot();
        await Medication.FillAndBlur("Lasix");
        await Expect(FormularyDispensed).ToHaveTextAsync("furosemide", new() { Timeout = 5000 });

        await Medication.FillAndBlur("Soma");

        await Expect(FormularyNote).ToHaveTextAsync("Not on the formulary: the pharmacy orders it specially.", new() { Timeout = 5000 });
        await Expect(FormularyDispensed).ToHaveTextAsync("");
        await Expect(FormularyForm).ToHaveTextAsync("");
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }

    [Test]
    public async Task a_name_too_short_to_look_up_asks_for_the_full_name()
    {
        await NavigateAndBoot();

        await Medication.FillAndBlur("Som");

        await Expect(FormularyNote).ToHaveTextAsync("Type the medication's full name to look it up.", new() { Timeout = 5000 });
        AssertNoConsoleErrorsExcept("the server responded with a status of 404");
    }
}
