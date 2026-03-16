using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.ValidationContract;

[TestFixture]
public class WhenValidatingWithAjaxPartials : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract/AjaxPartial";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");

    private async Task SelectCustomAddress()
    {
        await Input("AddressType").SelectOptionAsync("Custom Address");
        // Wait for partial to load and merge
        await Expect(Input("Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        // Small delay for plan merge to complete
        await Page.WaitForTimeoutAsync(300);
    }

    private async Task FillParentFields()
    {
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
    }

    // ── Before partial loads ────────────────────────────────

    [Test]
    public async Task parent_fields_validate_inline_before_partial_loads()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Address container is empty — no partial loaded
        await SubmitBtn.ClickAsync();

        // Parent fields should show inline errors
        await Expect(ErrorFor("Name")).ToContainTextAsync("required");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToContainTextAsync("required");

        AssertNoConsoleErrors();
    }

    // ── After partial loads ─────────────────────────────────

    [Test]
    public async Task loading_partial_enables_address_inline_validation()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        // Submit with empty address fields
        await SubmitBtn.ClickAsync();

        // Address fields should now validate inline
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_fields_validate_required_after_load()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        // Leave address fields empty
        await SubmitBtn.ClickAsync();

        // Address fields should show required errors
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");

        // Fill Street and City, leave ZipCode empty
        await Input("Address_Street").FillAsync("123 Main St");
        await Input("Address_City").FillAsync("Springfield");
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("Address.Street")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");

        AssertNoConsoleErrors();
    }

    // ── EqualTo works with parent fields ────────────────────

    [Test]
    public async Task equalto_works_when_both_fields_are_parent_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await Input("Email").FillAsync("a@b.com");
        await Input("ConfirmEmail").FillAsync("x@y.com");

        await SelectCustomAddress();
        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");
        AssertNoConsoleErrors();
    }

    // ── Partial reload ──────────────────────────────────────

    [Test]
    public async Task reloading_partial_replaces_old_html_and_re_enriches()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        // Fill street
        await Input("Address_Street").FillAsync("Old St");

        // Reload partial by re-selecting
        await Input("AddressType").SelectOptionAsync("Facility Address");
        await Page.WaitForTimeoutAsync(500);
        await SelectCustomAddress();

        // Street should be fresh (DOM replaced)
        var streetVal = await Input("Address_Street").InputValueAsync();
        Assert.That(streetVal, Is.EqualTo(""));

        // Submit → address errors inline (re-enriched)
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");

        AssertNoConsoleErrors();
    }

    // ── Partial reactive behavior after load ────────────────

    [Test]
    public async Task ajax_loaded_partial_reactive_entries_work_after_merge()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SelectCustomAddress();

        // Type into zip and tab out → partial's own dispatch fires
        await Input("Address_ZipCode").ClickAsync();
        await Input("Address_ZipCode").PressSequentiallyAsync("90210");
        await Page.Keyboard.PressAsync("Tab");

        var status = Page.Locator("#zipcode-status");
        await Expect(status).ToContainTextAsync("Zip validated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // ── Full lifecycle ──────────────────────────────────────

    [Test]
    public async Task full_lifecycle_errors_then_load_then_fix_then_post_sent()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // 1. Submit empty → parent errors inline
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();

        // 2. Fill parent
        await FillParentFields();
        await SubmitBtn.ClickAsync();
        // Parent fields pass client validation → POST sent → server may reject
        // (server validates ALL fields including those not on page)

        // 3. Load address partial
        await SelectCustomAddress();
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();

        // 4. Fill address fields → client validation passes for enriched fields
        await Input("Address_Street").FillAsync("123 Main St");
        await Input("Address_City").FillAsync("Springfield");
        await Input("Address_ZipCode").FillAsync("62704");

        await SubmitBtn.ClickAsync();

        // All enriched fields pass client validation → POST sent
        // Server validates full model so it may 400 for missing fields
        // But client-side validation should not block (unenriched fields skipped)
        await Expect(ErrorFor("Address.Street")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Address.ZipCode")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }
}
