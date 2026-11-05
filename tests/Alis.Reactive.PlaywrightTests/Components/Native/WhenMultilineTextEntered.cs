using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.Native;

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

        await Expect(IncidentFlag).ToBeVisibleAsync();
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
