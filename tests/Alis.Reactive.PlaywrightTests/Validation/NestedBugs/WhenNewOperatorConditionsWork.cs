namespace Alis.Reactive.PlaywrightTests.Validation.NestedBugs;

[TestFixture]
public class WhenNewOperatorConditionsWork : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/NestedBugs/OperatorConditions";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_OperatorConditionModel__";

    private ILocator Input(string field) => Page.Locator($"#{R}{field}");
    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator ErrorFor(string field) =>
        Page.Locator($"#op-form span[data-valmsg-for='{field}']");

    [Test]
    public async Task gte_condition_fires_when_age_is_adult()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("25");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("JobTitle")).ToContainTextAsync("Adults must provide job title");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gte_condition_skips_when_age_is_minor()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("15");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("JobTitle")).Not.ToContainTextAsync("Adults");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task lt_condition_fires_when_age_is_minor()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("15");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToContainTextAsync("Guardian name required");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lt_condition_skips_when_age_is_adult()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("25");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).Not.ToContainTextAsync("Guardian");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task in_condition_fires_when_care_level_matches()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("CareLevel").SelectOptionAsync("memory-care");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Notes")).ToContainTextAsync("Notes required for high-acuity");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task in_condition_skips_when_care_level_not_in_set()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("CareLevel").SelectOptionAsync("independent");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Notes")).Not.ToContainTextAsync("high-acuity");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task contains_condition_fires_when_notes_contain_keyword()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Notes").FillAsync("This is an urgent matter");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Phone")).ToContainTextAsync("Phone required for urgent");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task contains_condition_skips_when_notes_lack_keyword()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Notes").FillAsync("routine checkup");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Phone")).Not.ToContainTextAsync("urgent");
        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task not_empty_condition_fires_when_email_filled()
    {
        // Age stays adult so the lt condition does not also require Name.
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("25");
        await Input("Email").FillAsync("jane@care.com");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToContainTextAsync("Name required when email provided");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task not_empty_condition_skips_when_email_empty()
    {
        await NavigateToAndWaitForBoot(Path);

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).Not.ToContainTextAsync("email provided");
        AssertNoConsoleErrorsExcept("400");
    }
}
