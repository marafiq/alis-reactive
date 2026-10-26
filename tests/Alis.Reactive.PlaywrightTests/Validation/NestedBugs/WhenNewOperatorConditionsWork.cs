namespace Alis.Reactive.PlaywrightTests.Validation.NestedBugs;

[TestFixture]
public class WhenNewOperatorConditionsWork : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/NestedBugs/OperatorConditions";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_OperatorConditionModel__";
    private const string AdultAgeOutsideLessThanRule = "25";

    private ILocator Input(string field) => Page.Locator($"#{ModelIdPrefix}{field}");
    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator Result => Page.Locator("#result");
    private ILocator ErrorFor(string field) =>
        Page.Locator($"#op-form span[data-valmsg-for='{field}']");

    // An adult with a job title: every other condition is off, so a "skips" test can end in a save.
    private async Task EnterAnEmployedAdult()
    {
        await Input("Age").FillAsync(AdultAgeOutsideLessThanRule);
        await Input("JobTitle").FillAsync("Care Coordinator");
    }

    private async Task AssertSaved()
    {
        await Expect(Result).ToHaveTextAsync("Saved");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gte_condition_fires_when_age_is_adult()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync(AdultAgeOutsideLessThanRule);
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("JobTitle")).ToHaveTextAsync("Adults must provide job title.");
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gte_condition_skips_when_age_is_minor()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("15");
        await Input("Name").FillAsync("Jane Smith");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("JobTitle")).ToHaveTextAsync("");
        await AssertSaved();
    }

    [Test]
    public async Task lt_condition_fires_when_age_is_minor()
    {
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("15");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToHaveTextAsync("Guardian name required for minors.");
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    // An int left empty binds as 0 on the server, where "Age < 18" then holds; the browser reads the
    // age the same way, so it asks for the guardian before the form is sent.
    [Test]
    public async Task an_age_left_empty_counts_as_a_minor_as_the_server_binds_it()
    {
        await NavigateToAndWaitForBoot(Path);

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToHaveTextAsync("Guardian name required for minors.");
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lt_condition_skips_when_age_is_adult()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToHaveTextAsync("");
        await AssertSaved();
    }

    [Test]
    public async Task in_condition_fires_when_care_level_matches()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await Input("CareLevel").SelectOptionAsync("memory-care");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Notes")).ToHaveTextAsync("Notes required for high-acuity care.");
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task in_condition_skips_when_care_level_not_in_set()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await Input("CareLevel").SelectOptionAsync("independent");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Notes")).ToHaveTextAsync("");
        await AssertSaved();
    }

    [Test]
    public async Task contains_condition_fires_when_notes_contain_keyword()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await Input("Notes").FillAsync("This is an urgent matter");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Phone")).ToHaveTextAsync("Phone required for urgent cases.");
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task contains_condition_skips_when_notes_lack_keyword()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await Input("Notes").FillAsync("routine checkup");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Phone")).ToHaveTextAsync("");
        await AssertSaved();
    }

    [Test]
    public async Task not_empty_condition_fires_when_email_filled()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await Input("Email").FillAsync("jane@care.com");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToHaveTextAsync("Name required when email provided.");
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task not_empty_condition_skips_when_email_empty()
    {
        await NavigateToAndWaitForBoot(Path);

        await EnterAnEmployedAdult();
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToHaveTextAsync("");
        await AssertSaved();
    }
}
