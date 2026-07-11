using Alis.Reactive.Playwright.Extensions;

namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion;

// Journey: a resident reads their personal "My Care Plan" page. The plan is organized into
// collapsible sections. Opening a section tells them which section they are reading; the
// monthly-charges section loads its detail on demand; and the care-agreement section stays
// locked until the resident confirms they have reviewed their agreement.
[TestFixture]
public class WhenUsingFusionAccordion : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Accordion";

    private FusionAccordionLocator CarePlan => new(Page, "care-plan");
    private ILocator OpenSectionLabel => Page.Locator("#open-section");
    private ILocator AgreementStatus => Page.Locator("#agreement-status");
    private ILocator OpenSummaryButton => Page.Locator("#open-summary");
    private ILocator ConfirmAgreementButton => Page.Locator("#confirm-agreement");

    // Indexes of the three care-plan sections.
    private const int CareTeam = 0;
    private const int Charges = 1;
    private const int Agreement = 2;

    private async Task OpenCarePlan()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(CarePlan.Header(CareTeam)).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    // RENDERS — the FusionAccordion builder renders the care-plan sections, with the
    // care-agreement section initially locked (e-overlay) per its AccordionItem.Disabled.
    [Test]
    public async Task the_care_plan_opens_showing_its_three_sections()
    {
        await OpenCarePlan();

        await Expect(CarePlan.Header(CareTeam)).ToHaveTextAsync("My Care Team");
        await Expect(CarePlan.Header(Charges)).ToHaveTextAsync("My Services & Monthly Charges");
        await Expect(CarePlan.Header(Agreement)).ToHaveTextAsync("My Care Agreement");
        await Expect(CarePlan.Item(Agreement)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-overlay"));

        AssertNoConsoleErrors();
    }

    // INTERACTS / Expanded event / Reactive wiring / payload delivered — opening a section
    // fires the Expanded event through the .Reactive wiring; the resident sees the section's
    // content and the page tells them which section is now open.
    [Test]
    public async Task opening_the_care_team_section_shows_its_content_and_names_it_as_open()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(CareTeam);

        await Expect(Page.GetByText("Primary nurse: Maria Alvarez, RN")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(OpenSectionLabel).ToHaveTextAsync("My Care Team");

        AssertNoConsoleErrors();
    }

    // FusionAccordionExpandedArgs.Index — the open-section label is named by branching on the
    // event's Index. Opening the third section (a different index) must name THAT section, which
    // is impossible unless Index carries the real panel index.
    [Test]
    public async Task opening_a_different_section_names_that_section_not_the_first()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(CareTeam);
        await Expect(OpenSectionLabel).ToHaveTextAsync("My Care Team");

        // Charges is enabled from the start; open it and confirm the label follows the new index.
        await CarePlan.OpenSection(Charges);

        await Expect(OpenSectionLabel).ToHaveTextAsync("My Services & Monthly Charges");

        AssertNoConsoleErrors();
    }

    // FusionAccordionExpandedArgs.IsExpanded — collapsing a section fires Expanded with
    // IsExpanded false, routing the Else branch so the page shows no section is open.
    [Test]
    public async Task closing_the_open_section_shows_no_section_open()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(CareTeam);
        await Expect(OpenSectionLabel).ToHaveTextAsync("My Care Team");

        // Click the same header again to collapse it.
        await CarePlan.OpenSection(CareTeam);

        await Expect(OpenSectionLabel).ToHaveTextAsync("No section open");

        AssertNoConsoleErrors();
    }

    // ExpandItem(isExpand, index) — the "Open my care plan summary" button calls ExpandItem(true, 0)
    // to expand the care-team section programmatically, so the resident lands on it without clicking
    // the header. Proven by the care-team content becoming visible after the button click alone.
    [Test]
    public async Task opening_my_care_plan_summary_expands_the_care_team_section()
    {
        await OpenCarePlan();
        await Expect(Page.GetByText("Primary nurse: Maria Alvarez, RN")).Not.ToBeVisibleAsync();

        await OpenSummaryButton.ClickAsync();

        await Expect(Page.GetByText("Primary nurse: Maria Alvarez, RN")).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // EnableItem(index, isEnable) — the care-agreement section starts locked: Syncfusion overlays it
    // (e-overlay), which blocks interaction. Confirming the agreement calls EnableItem(2, true), which
    // removes the overlay so the resident can open the section and read it.
    [Test]
    public async Task confirming_the_care_agreement_unlocks_the_agreement_section()
    {
        await OpenCarePlan();

        // Locked: the agreement section is overlaid and unreadable.
        await Expect(CarePlan.Item(Agreement)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-overlay"));
        await Expect(Page.GetByText("Your signed care agreement dated 1 March 2026 is on file."))
            .Not.ToBeVisibleAsync();

        await ConfirmAgreementButton.ClickAsync();
        await Expect(AgreementStatus)
            .ToHaveTextAsync("Care agreement confirmed. Your care agreement section is now available.");

        // Unlocked: the overlay is gone and the section now opens for the resident to read.
        await Expect(CarePlan.Item(Agreement)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-overlay"));
        await CarePlan.OpenSection(Agreement);
        await Expect(Page.GetByText("Your signed care agreement dated 1 March 2026 is on file."))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    // HTTP-on-expand driven by Index == 1 — opening the monthly-charges section fires Expanded with
    // Index 1, which triggers the GET that injects this month's charges into the section body.
    [Test]
    public async Task opening_the_charges_section_loads_this_months_charges()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(Charges);

        await Expect(Page.GetByText("Billing period:")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Page.GetByText("$6,090")).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
