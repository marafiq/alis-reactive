using Alis.Reactive.Playwright.Extensions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

/// <summary>
/// As the evening charge nurse on Maple wing, I want to write the handover for the night nurse, starting
/// from the community's template, flag any incident for her to read first, and send it, so that the night
/// shift starts knowing what happened on mine.
/// </summary>
/// <remarks>
/// A text area raises change only when the nurse leaves it; Enter starts a new line (HTML, the textarea
/// element's change event), so an incident is flagged on leaving the field. The server answers a sent
/// handover with the number of lines it received. The text box locator reaches a text area by its plan
/// ID like any input. Not exercised: the text area as a validated input. This page does not validate,
/// and a text area's committing event, change, comes only with leaving the field, which rechecks a field
/// on its own, so no gesture could tell a broken commit event apart.
/// </remarks>
[TestFixture]
public class WhenMultilineTextEntered : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/NativeTextArea";

    private PagePlan<NativeTextAreaModel> _plan = null!;

    private NativeTextBoxLocator Handover => _plan.TextBox(m => m.HandoverNote);
    private NativeTextBoxLocator Incident => _plan.TextBox(m => m.Incident);
    private ILocator StartFromTemplate => Page.Locator("#handover-template-btn");
    private ILocator SendHandover => Page.Locator("#send-handover-btn");
    private ILocator Status => Page.Locator("#handover-status");
    private ILocator NoIncident => Page.Locator("#incident-none");
    private ILocator IncidentFlag => Page.Locator("#incident-flag");

    private async Task NavigateAndBoot()
    {
        await NavigateToAndWaitForBoot(Path);
        _plan = await PagePlan<NativeTextAreaModel>.FromPage(Page);
    }

    // Tabbing out of the text area commits what was written.
    private async Task WriteTheIncident(string incident)
    {
        await Incident.Fill(incident);
        await Incident.Input.PressAsync("Tab");
    }

    [Test]
    public async Task starting_from_the_template_puts_its_headings_in_the_handover()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartFromTemplate);

        await Expect(Handover.Input).ToHaveValueAsync("Vitals:\nMeals:\nMedications:\nFalls or incidents:\nFamily calls:");
        AssertNoConsoleErrors();
    }

    // The click leaves the cursor on the button; only the page moves it into the handover.
    [Test]
    public async Task starting_from_the_template_leaves_the_cursor_in_the_handover()
    {
        await NavigateAndBoot();

        await ClickWhenStable(StartFromTemplate);

        await Expect(Handover.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task sending_an_empty_handover_asks_for_it_and_puts_the_cursor_in_it()
    {
        await NavigateAndBoot();

        await ClickWhenStable(SendHandover);

        await Expect(Status).ToHaveTextAsync("Write the handover before sending it.");
        await Expect(Handover.Input).ToBeFocusedAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_sent_handover_reaches_the_night_nurse_line_by_line_with_its_incident()
    {
        await NavigateAndBoot();
        await Handover.Fill("Mrs. Hale: vitals stable.\nMr. Benson ate all his meals.\nMrs. Coleman refused the 9 PM Tylenol.");
        await WriteTheIncident("Mr. Benson slipped in the hallway at 7 PM; no injury.");

        await ClickWhenStable(SendHandover);

        await Expect(Status).ToHaveTextAsync("Handover sent to the night nurse: 3 lines, and an incident to read first.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_incident_written_down_is_flagged_for_the_night_nurse_to_read_first()
    {
        await NavigateAndBoot();
        await Expect(IncidentFlag).ToBeHiddenAsync();

        await WriteTheIncident("Mr. Benson slipped in the hallway at 7 PM; no injury.");

        await Expect(IncidentFlag).ToHaveTextAsync("The night nurse reads this first: Mr. Benson slipped in the hallway at 7 PM; no injury.");
        await Expect(NoIncident).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task clearing_the_incident_says_there_was_none()
    {
        await NavigateAndBoot();
        await WriteTheIncident("Mr. Benson slipped in the hallway at 7 PM; no injury.");
        await Expect(IncidentFlag).ToBeVisibleAsync();

        await WriteTheIncident("");

        await Expect(NoIncident).ToHaveTextAsync("No incidents this shift.");
        await Expect(NoIncident).ToBeVisibleAsync();
        await Expect(IncidentFlag).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
