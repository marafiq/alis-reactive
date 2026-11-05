namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Accordion;

[TestFixture]
public class WhenUsingFusionAccordion : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Accordion";

    private FusionAccordionLocator CarePlan => new(Page, "care-plan");
    private ILocator OpenSectionLabel => Page.Locator("#open-section");
    private ILocator AgreementStatus => Page.Locator("#agreement-status");
    private ILocator OpenSummaryButton => Page.Locator("#open-summary");
    private ILocator ConfirmAgreementButton => Page.Locator("#confirm-agreement");

    private const int CareTeam = 0;
    private const int Charges = 1;
    private const int Agreement = 2;

    private async Task OpenCarePlan()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(CarePlan.Header(CareTeam)).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

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

    [Test]
    public async Task opening_the_care_team_section_shows_its_content_and_names_it_as_open()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(CareTeam);

        await Expect(Page.GetByText("Primary nurse: Maria Alvarez, RN")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(OpenSectionLabel).ToHaveTextAsync("My Care Team");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_different_section_names_that_section_not_the_first()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(CareTeam);
        await Expect(OpenSectionLabel).ToHaveTextAsync("My Care Team");

        await CarePlan.OpenSection(Charges);

        await Expect(OpenSectionLabel).ToHaveTextAsync("My Services & Monthly Charges");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task closing_the_open_section_shows_no_section_open()
    {
        await OpenCarePlan();

        await CarePlan.OpenSection(CareTeam);
        await Expect(OpenSectionLabel).ToHaveTextAsync("My Care Team");

        await CarePlan.OpenSection(CareTeam);

        await Expect(OpenSectionLabel).ToHaveTextAsync("No section open");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_my_care_plan_summary_expands_the_care_team_section()
    {
        await OpenCarePlan();
        await Expect(Page.GetByText("Primary nurse: Maria Alvarez, RN")).Not.ToBeVisibleAsync();

        await OpenSummaryButton.ClickAsync();

        await Expect(Page.GetByText("Primary nurse: Maria Alvarez, RN")).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirming_the_care_agreement_unlocks_the_agreement_section()
    {
        await OpenCarePlan();

        await Expect(CarePlan.Item(Agreement)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-overlay"));
        await Expect(Page.GetByText("Your signed care agreement dated 1 March 2026 is on file."))
            .Not.ToBeVisibleAsync();

        await ConfirmAgreementButton.ClickAsync();
        await Expect(AgreementStatus)
            .ToHaveTextAsync("Care agreement confirmed. Your care agreement section is now available.");

        await Expect(CarePlan.Item(Agreement)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("e-overlay"));
        await CarePlan.OpenSection(Agreement);
        await Expect(Page.GetByText("Your signed care agreement dated 1 March 2026 is on file."))
            .ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

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
