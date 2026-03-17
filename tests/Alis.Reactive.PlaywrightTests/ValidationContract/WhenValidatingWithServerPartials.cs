namespace Alis.Reactive.PlaywrightTests.ValidationContract;

[TestFixture]
public class WhenValidatingWithServerPartials : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract/ServerPartial";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-alis-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");

    private async Task FillAllRequired()
    {
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
        await Input("CareLevel").SelectOptionAsync("Independent");
        await Input("Address_Street").FillAsync("123 Main St");
        await Input("Address_City").FillAsync("Springfield");
        await Input("Address_ZipCode").FillAsync("62704");
        await Input("ReasonForNoContact").FillAsync("No relatives nearby");
    }

    // ── Basic composition ───────────────────────────────────

    [Test]
    public async Task parent_field_errors_show_inline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();

        // Parent fields should show inline errors
        await Expect(ErrorFor("Name")).ToContainTextAsync("required");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_field_errors_show_inline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();

        // Address fields from partial should show inline
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_unconditional_required_fields_across_parent_and_partials()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SubmitBtn.ClickAsync();

        // Parent fields
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(ErrorFor("CareLevel")).ToBeVisibleAsync();

        // Address partial fields
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.ZipCode")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── Conditionals across parent and partial ──────────────

    [Test]
    public async Task conditional_rule_where_condition_source_is_in_parent_and_field_is_in_parent()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("IsVeteran").CheckAsync();

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("VeteranId")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_where_field_is_in_partial()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Address_ZipCode").FillAsync("abc");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("5 digits");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── EqualTo across form ─────────────────────────────────

    [Test]
    public async Task equalto_compares_two_parent_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input("Email").FillAsync("a@b.com");
        await Input("ConfirmEmail").FillAsync("x@y.com");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }

    // ── Partial's own reactive behavior ─────────────────────

    [Test]
    public async Task address_partial_zipcode_change_fires_partial_owned_dispatch()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Type into field and tab out to fire native change event
        await Input("Address_ZipCode").ClickAsync();
        await Input("Address_ZipCode").PressSequentiallyAsync("90210");
        await Page.Keyboard.PressAsync("Tab");

        // Wait for the dispatch chain: change → dispatch("zipcode-validated") → custom-event listener
        var status = Page.Locator("#zipcode-status");
        await Expect(status).ToContainTextAsync("Zip validated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task emergency_partial_phone_change_updates_status()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Type into field and tab out to fire native change event
        await Input("EmergencyPhone").ClickAsync();
        await Input("EmergencyPhone").PressSequentiallyAsync("555-0123");
        await Page.Keyboard.PressAsync("Tab");

        var status = Page.Locator("#phone-format-status");
        await Expect(status).ToContainTextAsync("Phone updated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_reactive_entries_do_not_interfere_with_parent_validation()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Trigger partial reactive behavior
        await Input("Address_ZipCode").FillAsync("90210");
        await Page.WaitForTimeoutAsync(500);

        // Then submit form — validation should work normally
        await FillAllRequired();
        await SubmitBtn.ClickAsync();

        var result = Page.Locator("#result");
        await Expect(result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Full workflow ───────────────────────────────────────

    [Test]
    public async Task fixing_all_fields_across_parent_and_partials_results_in_success()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Submit empty → errors
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Fill everything
        await FillAllRequired();
        await SubmitBtn.ClickAsync();

        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
