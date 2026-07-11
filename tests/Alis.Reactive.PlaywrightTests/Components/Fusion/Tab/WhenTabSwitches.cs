using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Tab;

// Journey: a care coordinator works a resident's Care Workspace — a tabbed surface with
// Care Schedule, Medications, Incident Reports, and Billing sections. The coordinator moves
// between sections, jumps straight to logging an incident, resumes in Medications, and hides
// Billing from the view when it should not be shown.
[TestFixture]
public class WhenTabSwitches : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Tab";
    private const string Workspace = "care-workspace";

    private FusionTabLocator CareWorkspace => new(Page, Workspace);
    private ILocator ActiveSection => Page.Locator("#active-section");
    private ILocator SectionContext => Page.Locator("#section-context");
    private ILocator NavigationMode => Page.Locator("#navigation-mode");
    private ILocator BillingAccess => Page.Locator("#billing-access");
    private ILocator ResumeButton => Page.Locator("#resume-medications");
    private ILocator LogIncidentButton => Page.Locator("#log-incident");
    private ILocator HideBillingButton => Page.Locator("#hide-billing");
    private ILocator ShowBillingButton => Page.Locator("#show-billing");

    private async Task OpenWorkspace()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(CareWorkspace.Header(0)).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionTab builder renders the workspace on its first section with that
    // section's content visible.
    [Test]
    public async Task workspace_opens_showing_the_care_schedule_section()
    {
        await OpenWorkspace();

        await Expect(CareWorkspace.ActiveHeader).ToHaveTextAsync("Care Schedule");
        await Expect(CareWorkspace.ActiveContent).ToContainTextAsync("Today's care schedule");
        await Expect(ActiveSection).ToHaveTextAsync("Care Schedule");

        AssertNoConsoleErrors();
    }

    // INTERACTS — clicking the Medications header fires the Selected event through the .Reactive
    // wiring; the FusionTabSelectedArgs.SelectedIndex routes the workspace to Medications and its
    // content shows.
    [Test]
    public async Task opening_the_medications_section_shows_the_current_medications()
    {
        await OpenWorkspace();

        await CareWorkspace.OpenSection(1);

        await Expect(CareWorkspace.ActiveHeader).ToHaveTextAsync("Medications");
        await Expect(CareWorkspace.ActiveContent).ToContainTextAsync("Lisinopril 10 mg");
        await Expect(ActiveSection).ToHaveTextAsync("Medications");

        AssertNoConsoleErrors();
    }

    // FusionTabSelectedArgs.PreviousIndex — the workspace names the section the coordinator just
    // left. Moving from Medications to Incident Reports must read that the move came from Medications.
    [Test]
    public async Task moving_between_sections_records_where_the_coordinator_came_from()
    {
        await OpenWorkspace();

        await CareWorkspace.OpenSection(1);
        await Expect(SectionContext).ToHaveTextAsync("You moved here from Care Schedule.");

        await CareWorkspace.OpenSection(2);

        await Expect(ActiveSection).ToHaveTextAsync("Incident Reports");
        await Expect(SectionContext).ToHaveTextAsync("You moved here from Medications.");

        AssertNoConsoleErrors();
    }

    // FusionTabSelectedArgs.IsSwiped — a section opened by clicking is recorded as a deliberate
    // selection, not a swipe. Clicking must show the selection wording, never the swipe wording.
    [Test]
    public async Task opening_a_section_by_clicking_is_recorded_as_a_deliberate_selection()
    {
        await OpenWorkspace();

        await CareWorkspace.OpenSection(2);

        await Expect(NavigationMode).ToHaveTextAsync("Last navigation: opened by selection.");

        AssertNoConsoleErrors();
    }

    // SetSelectedItem — the "Resume in Medications" action writes the selected section index, so
    // the workspace jumps to Medications and shows its content without the coordinator touching the
    // tab strip.
    [Test]
    public async Task resuming_returns_the_coordinator_to_the_medications_section()
    {
        await OpenWorkspace();
        await Expect(CareWorkspace.ActiveHeader).ToHaveTextAsync("Care Schedule");

        await ResumeButton.ClickAsync();

        await Expect(CareWorkspace.ActiveHeader).ToHaveTextAsync("Medications");
        await Expect(CareWorkspace.ActiveContent).ToContainTextAsync("Lisinopril 10 mg");
        await Expect(ActiveSection).ToHaveTextAsync("Medications");

        AssertNoConsoleErrors();
    }

    // Select — the "Log an incident now" shortcut calls the select method to jump straight to the
    // Incident Reports section.
    [Test]
    public async Task the_log_incident_shortcut_jumps_straight_to_incident_reports()
    {
        await OpenWorkspace();
        await Expect(CareWorkspace.ActiveHeader).ToHaveTextAsync("Care Schedule");

        await LogIncidentButton.ClickAsync();

        await Expect(CareWorkspace.ActiveHeader).ToHaveTextAsync("Incident Reports");
        await Expect(CareWorkspace.ActiveContent).ToContainTextAsync("No incidents reported");
        await Expect(ActiveSection).ToHaveTextAsync("Incident Reports");

        AssertNoConsoleErrors();
    }

    // HideTab — hiding Billing takes the Billing section out of the tab strip the coordinator sees
    // and tells them it is hidden. The Billing header is no longer visible and the visible sections
    // drop from four to three.
    [Test]
    public async Task hiding_billing_removes_it_from_the_workspace()
    {
        await OpenWorkspace();
        await Expect(CareWorkspace.VisibleHeaders).ToHaveCountAsync(4);

        await HideBillingButton.ClickAsync();

        await Expect(CareWorkspace.HeaderByText("Billing")).ToBeHiddenAsync();
        await Expect(CareWorkspace.VisibleHeaders).ToHaveCountAsync(3);
        await Expect(BillingAccess).ToHaveTextAsync("Billing is hidden from this workspace.");

        AssertNoConsoleErrors();
    }

    // HideTab (show) — restoring Billing brings the section back into the tab strip. After hiding it,
    // the restore action makes the Billing header visible again and returns the strip to four sections.
    [Test]
    public async Task restoring_billing_brings_the_tab_back()
    {
        await OpenWorkspace();

        await HideBillingButton.ClickAsync();
        await Expect(CareWorkspace.HeaderByText("Billing")).ToBeHiddenAsync();

        await ShowBillingButton.ClickAsync();

        await Expect(CareWorkspace.HeaderByText("Billing")).ToBeVisibleAsync();
        await Expect(CareWorkspace.VisibleHeaders).ToHaveCountAsync(4);
        await Expect(BillingAccess).ToHaveTextAsync("Billing is visible to coordinators.");

        AssertNoConsoleErrors();
    }
}
