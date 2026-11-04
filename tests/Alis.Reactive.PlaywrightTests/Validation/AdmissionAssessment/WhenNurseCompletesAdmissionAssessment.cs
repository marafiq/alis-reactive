using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.AdmissionAssessment;

[TestFixture]
public class WhenNurseCompletesAdmissionAssessment : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/AdmissionAssessment";
    private const string Saved = "Assessment saved for Helen Park.";
    private const string Printed = "Assessment printed for Helen Park.";
    private const string ChartRefused = "The chart refused the assessment.";
    private const string ServerRefusal = "the server responded with a status of 400";

    private PagePlan<AdmissionAssessmentModel> _plan = null!;
    private ConcurrentQueue<string> _savesSent = null!;
    private ConcurrentQueue<string> _printsSent = null!;

    private ILocator SaveAssessment => Page.Locator("#save-assessment-btn");
    private ILocator PrintForChart => Page.Locator("#print-assessment-btn");
    private ILocator AssessmentStatus => Page.Locator("#assessment-status");
    private ILocator PrintStatus => Page.Locator("#print-status");
    private NativeDropDownLocator CareLevel => _plan.NativeDropDown(m => m.CareLevel);
    private NativeDropDownLocator Residency => _plan.NativeDropDown(m => m.Residency);
    private NativeDropDownLocator MobilityAid => _plan.NativeDropDown(m => m.MobilityAid);
    private NativeRadioGroupLocator FellInLastYear => _plan.NativeRadioGroup(m => m.FellInLastYear);
    private NativeCheckBoxLocator ConsentOnFile => _plan.NativeCheckBox(m => m.ConsentOnFile);
    private NativeCheckBoxLocator PhotoConsent => _plan.NativeCheckBox(m => m.PhotoConsent);

    private async Task NavigateAndBoot()
    {
        _savesSent = new ConcurrentQueue<string>();
        _printsSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/AdmissionAssessment/Save")) _savesSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/AdmissionAssessment/Print")) _printsSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<AdmissionAssessmentModel>.FromPage(Page);
    }

    private async Task Write(Expression<Func<AdmissionAssessmentModel, object?>> field, string text)
    {
        await _plan.TextBox(field).Fill(text);
        await Page.Keyboard.PressAsync("Tab");
    }

    private async Task CompleteTheAssessmentLeavingTheFallQuestion()
    {
        await Write(m => m.ResidentName, "Helen Park");
        await Write(m => m.ConsentWord, "AGREE");
        await Write(m => m.SignaturesCollected, "2");
        await Write(m => m.MoveInDate, "2025-03-01");
        await Write(m => m.FallsLastYear, "1");
        await CareLevel.Select("Assisted living");
        await Residency.Select("Permanent");
        await MobilityAid.Select("Walker");
        await ConsentOnFile.Check();
        await PhotoConsent.Check();
    }

    private async Task CompleteTheAssessment()
    {
        await CompleteTheAssessmentLeavingTheFallQuestion();
        await FellInLastYear.Choose("No");
    }

    [Test]
    public async Task leaving_the_residents_name_blank_asks_for_it()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, "");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.ResidentName, "Helen Park");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_one_letter_name_is_refused_and_a_two_letter_name_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, "H");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("The resident's name is 2 to 40 characters.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("The resident's name is 2 to 40 characters.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.ResidentName, "Al");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync("Assessment saved for Al.");
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_forty_one_letter_name_is_refused_and_a_forty_letter_name_is_saved()
    {
        var fortyLetters = new string('H', 40);
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, fortyLetters + "H");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("The resident's name is 2 to 40 characters.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("The resident's name is 2 to 40 characters.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.ResidentName, fortyLetters);
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync($"Assessment saved for {fortyLetters}.");
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_two_letter_preferred_name_is_refused_and_a_three_letter_one_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PreferredName, "Le");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.PreferredName)).ToHaveTextAsync("A preferred name is at least 3 characters.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.PreferredName)).ToHaveTextAsync("A preferred name is at least 3 characters.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.PreferredName, "Len");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_thirteen_character_wristband_label_is_refused_and_twelve_characters_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.WristbandLabel, "PARK, HELEN M");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.WristbandLabel)).ToHaveTextAsync("A wristband label is at most 12 characters.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.WristbandLabel)).ToHaveTextAsync("A wristband label is at most 12 characters.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.WristbandLabel, "PARK, HELEN ");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_room_code_of_five_or_seven_characters_is_refused_and_six_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.RoomCode, "MA-21");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.RoomCode)).ToHaveTextAsync("Enter the room as wing and number, like MA-214.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.RoomCode)).ToHaveTextAsync("Enter the room as wing and number, like MA-214.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.RoomCode, "MA-2145");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.RoomCode)).ToHaveTextAsync("Enter the room as wing and number, like MA-214.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(2), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.RoomCode)).ToHaveTextAsync("Enter the room as wing and number, like MA-214.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.RoomCode, "MA-214");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_family_email_with_nothing_on_one_side_of_the_at_is_refused_and_name_at_domain_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FamilyEmail, "ana@");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.FamilyEmail)).ToHaveTextAsync("Enter the family email as name@domain.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.FamilyEmail)).ToHaveTextAsync("Enter the family email as name@domain.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.FamilyEmail, "@park.example");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.FamilyEmail)).ToHaveTextAsync("Enter the family email as name@domain.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(2), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.FamilyEmail)).ToHaveTextAsync("Enter the family email as name@domain.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.FamilyEmail, "ana@park.example");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_family_email_with_two_at_signs_is_refused()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FamilyEmail, "ana@park@example");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.FamilyEmail)).ToHaveTextAsync("Enter the family email as name@domain.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.FamilyEmail)).ToHaveTextAsync("Enter the family email as name@domain.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_portal_link_that_is_not_a_web_address_is_refused_and_an_https_link_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PortalLink, "ftp://family.example/hale");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.PortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.PortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.PortalLink, "https://family.example/hale");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_portal_link_with_a_space_in_it_is_refused_by_the_chart()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PortalLink, "http://family example/hale");

        await ClickWhenStable(PrintForChart);

        await Expect(_plan.ErrorFor(m => m.PortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_portal_link_with_a_space_in_it_is_refused_before_it_is_sent()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PortalLink, "http://family example/hale");

        await ClickWhenStable(SaveAssessment);

        await Expect(_plan.ErrorFor(m => m.PortalLink)).ToHaveTextAsync("The family portal link must be a web address.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_consent_word_in_lower_case_is_refused_and_agree_in_capitals_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ConsentWord, "agree");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.ConsentWord)).ToHaveTextAsync("Type AGREE to confirm the resident's consent.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ConsentWord)).ToHaveTextAsync("Type AGREE to confirm the resident's consent.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.ConsentWord, "AGREE");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task one_signature_or_three_are_refused_and_two_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.SignaturesCollected, "1");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.SignaturesCollected)).ToHaveTextAsync("Two signatures are collected: the resident's and a witness's.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.SignaturesCollected)).ToHaveTextAsync("Two signatures are collected: the resident's and a witness's.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.SignaturesCollected, "3");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.SignaturesCollected)).ToHaveTextAsync("Two signatures are collected: the resident's and a witness's.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(2), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.SignaturesCollected)).ToHaveTextAsync("Two signatures are collected: the resident's and a witness's.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.SignaturesCollected, "2");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_age_of_fifty_four_is_refused_and_fifty_five_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.Age, "54");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.Age)).ToHaveTextAsync("Residents are 55 or older.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.Age)).ToHaveTextAsync("Residents are 55 or older.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.Age, "55");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_age_of_one_hundred_twenty_is_refused_and_one_hundred_nineteen_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.Age, "120");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.Age)).ToHaveTextAsync("Check the age: 120 or more cannot be right.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.Age)).ToHaveTextAsync("Check the age: 120 or more cannot be right.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.Age, "119");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_weight_of_twenty_kilograms_is_refused_and_twenty_one_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.WeightKg, "20");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.WeightKg)).ToHaveTextAsync("Weight is recorded in kilograms, above 20.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.WeightKg)).ToHaveTextAsync("Weight is recorded in kilograms, above 20.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.WeightKg, "21");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_weight_of_two_hundred_fifty_one_kilograms_is_refused_and_two_hundred_fifty_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.WeightKg, "251");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.WeightKg)).ToHaveTextAsync("Weight is recorded in kilograms, up to 250.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.WeightKg)).ToHaveTextAsync("Weight is recorded in kilograms, up to 250.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.WeightKg, "250");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_pain_score_below_zero_or_above_ten_is_refused_and_zero_and_ten_are_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.PainScore, "-1");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.PainScore)).ToHaveTextAsync("Pain is scored 0 to 10.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.PainScore)).ToHaveTextAsync("Pain is scored 0 to 10.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.PainScore, "11");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.PainScore)).ToHaveTextAsync("Pain is scored 0 to 10.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(2), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.PainScore)).ToHaveTextAsync("Pain is scored 0 to 10.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.PainScore, "0");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        await Write(m => m.PainScore, "10");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(2), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task leaving_the_move_in_day_blank_asks_for_it()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.MoveInDate, "");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.MoveInDate)).ToHaveTextAsync("Enter the move-in day.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.MoveInDate)).ToHaveTextAsync("Enter the move-in day.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.MoveInDate, "2025-03-01");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_move_in_day_in_2023_is_refused_and_january_1_2024_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.MoveInDate, "2023-12-31");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.MoveInDate)).ToHaveTextAsync("Move-in days start at January 1, 2024.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.MoveInDate)).ToHaveTextAsync("Move-in days start at January 1, 2024.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.MoveInDate, "2024-01-01");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_assessment_dated_outside_2024_to_2026_is_refused_and_its_first_and_last_days_are_saved()
    {
        const string Outside = "Date the assessment between January 1, 2024 and December 31, 2026.";
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.AssessmentDate, "2023-12-31");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.AssessmentDate)).ToHaveTextAsync(Outside);
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.AssessmentDate)).ToHaveTextAsync(Outside);
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.AssessmentDate, "2027-01-01");
        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.AssessmentDate)).ToHaveTextAsync(Outside);
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(2), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.AssessmentDate)).ToHaveTextAsync(Outside);
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.AssessmentDate, "2024-01-01");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        await Write(m => m.AssessmentDate, "2026-12-31");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(2), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_next_review_on_the_assessment_day_is_refused_and_the_day_after_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.AssessmentDate, "2025-03-01");
        await Write(m => m.NextReviewDate, "2025-03-01");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.NextReviewDate)).ToHaveTextAsync("The next review comes after the assessment day.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.NextReviewDate)).ToHaveTextAsync("The next review comes after the assessment day.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.NextReviewDate, "2025-03-02");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_next_review_without_an_assessment_day_is_refused()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.NextReviewDate, "2025-03-02");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.NextReviewDate)).ToHaveTextAsync("The next review comes after the assessment day.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.NextReviewDate)).ToHaveTextAsync("The next review comes after the assessment day.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.AssessmentDate, "2025-03-01");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_next_review_in_2028_is_refused_and_december_31_2027_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.AssessmentDate, "2025-03-01");
        await Write(m => m.NextReviewDate, "2028-01-01");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.NextReviewDate)).ToHaveTextAsync("Reviews are planned up to December 31, 2027.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.NextReviewDate)).ToHaveTextAsync("Reviews are planned up to December 31, 2027.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await Write(m => m.NextReviewDate, "2027-12-31");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task leaving_the_care_level_on_its_placeholder_asks_for_it()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await CareLevel.Input.SelectOptionAsync(new SelectOptionValue { Value = "" });

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await CareLevel.Select("Memory care");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_mobility_aid_left_not_assessed_is_refused_and_a_cane_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await MobilityAid.Select("Not assessed");

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.MobilityAid)).ToHaveTextAsync("Assess the mobility aid.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.MobilityAid)).ToHaveTextAsync("Assess the mobility aid.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await MobilityAid.Select("Cane");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task leaving_the_fall_question_unanswered_asks_for_an_answer()
    {
        await NavigateAndBoot();
        await CompleteTheAssessmentLeavingTheFallQuestion();

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.FellInLastYear)).ToHaveTextAsync("Answer whether the resident fell in the last year.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.FellInLastYear)).ToHaveTextAsync("Answer whether the resident fell in the last year.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await FellInLastYear.Choose("Yes");
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task an_unticked_consent_box_is_refused_and_a_ticked_one_is_saved()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await ConsentOnFile.Input.UncheckAsync();

        await ClickWhenStable(PrintForChart);
        await Expect(_plan.ErrorFor(m => m.ConsentOnFile)).ToHaveTextAsync("Confirm the signed consent is on file.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1), "Print sends the assessment as it is; the server decides.");

        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ConsentOnFile)).ToHaveTextAsync("Confirm the signed consent is on file.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");

        await ConsentOnFile.Check();
        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_residency_left_not_set_is_refused_by_the_chart()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Residency.Select("Not set");

        await ClickWhenStable(PrintForChart);

        await Expect(_plan.ErrorFor(m => m.Residency)).ToHaveTextAsync("Say whether the stay is permanent or respite.");
        await Expect(PrintStatus).ToHaveTextAsync(ChartRefused);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrorsExcept(ServerRefusal);
    }

    [Test]
    public async Task a_residency_left_not_set_is_refused_before_it_is_sent()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Residency.Select("Not set");

        await ClickWhenStable(SaveAssessment);

        await Expect(_plan.ErrorFor(m => m.Residency)).ToHaveTextAsync("Say whether the stay is permanent or respite.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "The page refused the value, so nothing was sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_unticked_photo_consent_box_is_printed_as_a_no()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await PhotoConsent.Input.UncheckAsync();

        await ClickWhenStable(PrintForChart);

        await Expect(PrintStatus).ToHaveTextAsync(Printed);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_unticked_photo_consent_box_is_saved_as_a_no()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await PhotoConsent.Input.UncheckAsync();

        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        await Expect(_plan.ErrorFor(m => m.PhotoConsent)).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_falls_last_year_is_printed_as_zero()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FallsLastYear, "0");

        await ClickWhenStable(PrintForChart);

        await Expect(PrintStatus).ToHaveTextAsync(Printed);
        Assert.That(_printsSent, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task no_falls_last_year_is_saved_as_zero()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.FallsLastYear, "0");

        await ClickWhenStable(SaveAssessment);
        await Expect(AssessmentStatus).ToHaveTextAsync(Saved);
        Assert.That(_savesSent, Has.Count.EqualTo(1), "The page let the value through, and the server accepted it.");
        await Expect(_plan.ErrorFor(m => m.FallsLastYear)).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task typing_into_the_name_clears_its_message_before_the_field_is_left()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await Write(m => m.ResidentName, "");
        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Enter the resident's name.");

        await _plan.TextBox(m => m.ResidentName).Input.PressSequentiallyAsync("H");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("");
        await Expect(_plan.TextBox(m => m.ResidentName).Input).ToBeFocusedAsync();
        Assert.That(_savesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task leaving_the_name_with_one_letter_asks_for_two_without_saving()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();

        await _plan.TextBox(m => m.ResidentName).Fill("H");
        await Page.Keyboard.PressAsync("Tab");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("The resident's name is 2 to 40 characters.");
        await Expect(AssessmentStatus).ToHaveTextAsync("Not saved yet.");
        Assert.That(_savesSent, Is.Empty, "Leaving a field sends nothing.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task choosing_a_care_level_clears_its_message_at_once_and_unchoosing_it_asks_again()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await CareLevel.Input.SelectOptionAsync(new SelectOptionValue { Value = "" });
        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");

        await CareLevel.Select("Memory care");
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("");

        await CareLevel.Input.SelectOptionAsync(new SelectOptionValue { Value = "" });
        await Expect(_plan.ErrorFor(m => m.CareLevel)).ToHaveTextAsync("Choose a care level.");
        Assert.That(_savesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task ticking_the_consent_box_clears_its_message_at_once()
    {
        await NavigateAndBoot();
        await CompleteTheAssessment();
        await ConsentOnFile.Input.UncheckAsync();
        await ClickWhenStable(SaveAssessment);
        await Expect(_plan.ErrorFor(m => m.ConsentOnFile)).ToHaveTextAsync("Confirm the signed consent is on file.");

        await ConsentOnFile.Check();

        await Expect(_plan.ErrorFor(m => m.ConsentOnFile)).ToHaveTextAsync("");
        Assert.That(_savesSent, Is.Empty);
        AssertNoConsoleErrors();
    }
}
