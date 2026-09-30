using System.Linq.Expressions;
using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.EmergencyContacts;

/// <summary>
/// As an admissions coordinator, I want each emergency contact's phone required only when the
/// resident has emergency contacts, so that I can complete an intake either way.
/// </summary>
[TestFixture]
public class WhenCoordinatorRecordsEmergencyContacts : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/EmergencyContacts";

    private PagePlan<EmergencyContactsModel> _plan = null!;

    // IdGenerator is the framework's own id rule: it resolves the indexed contact rows, which PagePlan
    // cannot, and the checkbox, which PagePlan has no native accessor for.
    private ILocator Field(Expression<Func<EmergencyContactsModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}");

    private ILocator ErrorFor(Expression<Func<EmergencyContactsModel, object?>> member) =>
        Page.Locator($"#{IdGenerator.For(member)}_error");

    private ILocator IntakeHeading => Page.GetByRole(AriaRole.Heading, new() { Name = "Resident Intake: Emergency Contacts" });
    private ILocator SaveIntake => Page.Locator("#save-emergency-contacts-btn");
    private ILocator IntakeStatus => Page.Locator("#emergency-contacts-status");

    private async Task NavigateAndBoot()
    {
        await NavigateTo(Path);
        await Expect(IntakeHeading).ToBeVisibleAsync();
        await WaitForTraceMessage("booted", 10000);
        _plan = await PagePlan<EmergencyContactsModel>.FromPage(Page);
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
}
