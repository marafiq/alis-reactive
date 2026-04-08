namespace Alis.Reactive.PlaywrightTests.Validation.NestedBugs;

/// <summary>
/// E2E tests proving new WhenField* operators (Gte, Lt, In, Contains, NotEmpty)
/// evaluate correctly in the browser — client-side matches server-side.
/// </summary>
[TestFixture]
public class WhenNewOperatorConditionsWork : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/NestedBugs/OperatorConditions";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_Validation_NestedBugs_OperatorConditionModel__";

    private ILocator Input(string field) => Page.Locator($"#{R}{field}");
    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator ErrorFor(string field) =>
        Page.Locator($"#op-form span[data-valmsg-for='{field}']");

    // ── Gte operator ───────────────────────────────────────────────────────

    [Test]
    public async Task gte_condition_fires_when_age_is_adult()
    {
        // Age=25 (>= 18) → JobTitle required
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("25");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("JobTitle")).ToContainTextAsync("Adults must provide job title");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task gte_condition_skips_when_age_is_minor()
    {
        // Age=15 (< 18) → JobTitle NOT required
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("15");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("JobTitle")).Not.ToContainTextAsync("Adults");
        AssertNoConsoleErrorsExcept("400");
    }

    // ── Lt operator ────────────────────────────────────────────────────────

    [Test]
    public async Task lt_condition_fires_when_age_is_minor()
    {
        // Age=15 (< 18) → Name required (guardian)
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("15");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToContainTextAsync("Guardian name required");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task lt_condition_skips_when_age_is_adult()
    {
        // Age=25 (>= 18) → Name NOT required by lt condition
        await NavigateToAndWaitForBoot(Path);

        await Input("Age").FillAsync("25");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).Not.ToContainTextAsync("Guardian");
        AssertNoConsoleErrorsExcept("400");
    }

    // ── In operator ────────────────────────────────────────────────────────

    [Test]
    public async Task in_condition_fires_when_care_level_matches()
    {
        // CareLevel="memory-care" (in set) → Notes required
        await NavigateToAndWaitForBoot(Path);

        await Input("CareLevel").SelectOptionAsync("memory-care");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Notes")).ToContainTextAsync("Notes required for high-acuity");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task in_condition_skips_when_care_level_not_in_set()
    {
        // CareLevel="independent" (not in set) → Notes NOT required
        await NavigateToAndWaitForBoot(Path);

        await Input("CareLevel").SelectOptionAsync("independent");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Notes")).Not.ToContainTextAsync("high-acuity");
        AssertNoConsoleErrorsExcept("400");
    }

    // ── Contains operator ──────────────────────────────────────────────────

    [Test]
    public async Task contains_condition_fires_when_notes_contain_keyword()
    {
        // Notes contains "urgent" → Phone required
        await NavigateToAndWaitForBoot(Path);

        await Input("Notes").FillAsync("This is an urgent matter");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Phone")).ToContainTextAsync("Phone required for urgent");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task contains_condition_skips_when_notes_lack_keyword()
    {
        // Notes="routine checkup" (no "urgent") → Phone NOT required
        await NavigateToAndWaitForBoot(Path);

        await Input("Notes").FillAsync("routine checkup");
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Phone")).Not.ToContainTextAsync("urgent");
        AssertNoConsoleErrorsExcept("400");
    }

    // ── NotEmpty operator ──────────────────────────────────────────────────

    [Test]
    public async Task not_empty_condition_fires_when_email_filled()
    {
        // Email filled → Name required (set age >= 18 to avoid lt condition on Name)
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
        // Email empty → Name NOT required by this condition
        await NavigateToAndWaitForBoot(Path);

        // Leave email empty
        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).Not.ToContainTextAsync("email provided");
        AssertNoConsoleErrorsExcept("400");
    }
}
