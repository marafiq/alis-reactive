using System.Collections.Concurrent;
using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.EmergencyContacts;

/// <summary>
/// As an admissions coordinator, I want each emergency contact's phone required only when the
/// resident has emergency contacts, so that I can complete an intake either way.
/// </summary>
/// <remarks>
/// One validator serves the page: Save Intake checks the whole form, Save Contacts checks only the
/// contacts section. The server also checks each number against the community's directory, which
/// the browser cannot.
/// </remarks>
[TestFixture]
public class WhenCoordinatorRecordsEmergencyContacts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/EmergencyContacts";
    private const string FrontDeskNumber = "555-0199 is the front desk. Enter the contact's own number.";

    private PagePlan<EmergencyContactsModel> _plan = null!;
    private ConcurrentQueue<string> _intakesSent = null!;
    private ConcurrentQueue<string> _contactSavesSent = null!;

    // IdGenerator is the framework's own id rule: it resolves the indexed contact rows, which PagePlan
    // cannot, and the checkbox, which PagePlan has no native accessor for.
    private ILocator Field(Expression<Func<EmergencyContactsModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator ErrorFor(Expression<Func<EmergencyContactsModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}_error");

    private ILocator IntakeHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Resident Intake: Emergency Contacts" });
    private ILocator SaveIntake => Page.Locator("#save-emergency-contacts-btn");
    private ILocator SaveContacts => Page.Locator("#save-contacts-btn");
    private ILocator IntakeStatus => Page.Locator("#emergency-contacts-status");

    private async Task NavigateAndBoot()
    {
        _intakesSent = new ConcurrentQueue<string>();
        _contactSavesSent = new ConcurrentQueue<string>();
        Page.Request += (_, request) =>
        {
            if (request.Url.EndsWith("/Sandbox/Validation/EmergencyContacts/Save")) _intakesSent.Enqueue(request.Url);
            if (request.Url.EndsWith("/Sandbox/Validation/EmergencyContacts/SaveContacts")) _contactSavesSent.Enqueue(request.Url);
        };
        await NavigateTo(Path);
        await Expect(IntakeHeading).ToBeVisibleAsync();
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<EmergencyContactsModel>.FromPage(Page);
    }

    private async Task RecordBothContacts(string secondPhone)
    {
        await Field(m => m.HasContacts).CheckAsync();
        await Field(m => m.Contacts[0].Name).FillAsync("Grace Park");
        await Field(m => m.Contacts[0].Phone).FillAsync("555-0142");
        await Field(m => m.Contacts[1].Name).FillAsync("Owen Park");
        await Field(m => m.Contacts[1].Phone).FillAsync(secondPhone);
        await Page.Keyboard.PressAsync("Tab");
    }

    [Test]
    public async Task a_contact_without_a_phone_is_asked_for_one_when_the_resident_has_contacts()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Field(m => m.HasContacts).CheckAsync();
        await Field(m => m.Contacts[0].Name).FillAsync("Grace Park");

        await ClickWhenStable(SaveIntake);

        await Expect(ErrorFor(m => m.Contacts[0].Phone)).ToHaveTextAsync("Phone is required.");
        // The server's matching rule would show the same message after a 400, which Chromium logs as
        // a console error; none here means the browser stopped the save itself.
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task a_resident_without_contacts_is_saved_without_asking_for_phones()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");

        await ClickWhenStable(SaveIntake);

        await Expect(IntakeStatus).ToHaveTextAsync("Intake saved for Helen Park with no emergency contacts.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_front_desk_number_is_refused_beside_the_contact_that_gave_it()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await RecordBothContacts(EmergencyContactValidator.FrontDeskPhone);

        await ClickWhenStable(SaveIntake);

        // The server names the second contact's phone, Contacts[1].Phone, and the page puts the reason there.
        await Expect(ErrorFor(m => m.Contacts[1].Phone)).ToHaveTextAsync(FrontDeskNumber);
        await Expect(ErrorFor(m => m.Contacts[0].Phone)).ToHaveTextAsync("");
        await Expect(IntakeStatus).ToHaveTextAsync("No intake saved yet.");
        Assert.That(_intakesSent, Has.Count.EqualTo(1), "Only the server knows the directory, so the intake is sent.");

        await Field(m => m.Contacts[1].Phone).FillAsync("555-0177");
        await ClickWhenStable(SaveIntake);
        await Expect(IntakeStatus).ToHaveTextAsync("Intake saved for Helen Park with 2 emergency contact(s).");
        Assert.That(_intakesSent, Has.Count.EqualTo(2));
        AssertNoConsoleErrorsExcept("the server responded with a status of 400");
    }

    // Save Contacts, under the contacts section, comes before Save Intake, and the section does not hold
    // the resident's name: leaving the name empty still asks for it, by the whole form's rule.
    [Test]
    public async Task leaving_the_residents_name_empty_asks_for_it_before_anything_is_saved()
    {
        await NavigateAndBoot();
        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await _plan.TextBox(m => m.ResidentName).Fill("");
        await Page.Keyboard.PressAsync("Tab");

        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("Resident name is required.");

        await _plan.TextBox(m => m.ResidentName).Fill("Helen Park");
        await Page.Keyboard.PressAsync("Tab");
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("");
        Assert.That(_intakesSent, Is.Empty, "Leaving a field sends nothing.");
        Assert.That(_contactSavesSent, Is.Empty);
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task saving_the_contacts_alone_does_not_wait_for_the_residents_name()
    {
        await NavigateAndBoot();
        await RecordBothContacts("555-0177");

        await ClickWhenStable(SaveContacts);

        // The resident's name is outside the contacts section, so its rule does not hold the contacts up.
        await Expect(IntakeStatus).ToHaveTextAsync("Emergency contacts saved: 2.");
        await Expect(_plan.ErrorFor(m => m.ResidentName)).ToHaveTextAsync("");
        Assert.That(_contactSavesSent, Has.Count.EqualTo(1));

        await Field(m => m.Contacts[1].Phone).FillAsync("");
        await ClickWhenStable(SaveContacts);
        await Expect(ErrorFor(m => m.Contacts[1].Phone)).ToHaveTextAsync("Phone is required.");
        Assert.That(_contactSavesSent, Has.Count.EqualTo(1), "A contact without a phone is not saved.");
        AssertNoConsoleErrors();
    }
}
