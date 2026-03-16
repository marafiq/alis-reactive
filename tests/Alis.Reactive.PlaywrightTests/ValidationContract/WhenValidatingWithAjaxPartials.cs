using System.Text.RegularExpressions;

namespace Alis.Reactive.PlaywrightTests.ValidationContract;

[TestFixture]
public class WhenValidatingWithAjaxPartials : PlaywrightTestBase
{
    private const string Path = "/Sandbox/ValidationContract/AjaxPartial";
    private const string R = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-alis-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{R}{suffix}");

    private async Task SelectCustomAddress()
    {
        await Input("AddressType").SelectOptionAsync("Custom Address");
        await Expect(Input("Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Page.WaitForTimeoutAsync(300);
    }

    private async Task FillParentFields()
    {
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
    }

    // ── Full AJAX partial lifecycle (Skill Rule #5) ─────

    [Test]
    public async Task full_ajax_partial_lifecycle()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Step 1: Submit with placeholder — parent errors only, NO address in summary
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToContainTextAsync("required");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Step 2: Select Facility Address → submit → still no address errors
        await Input("AddressType").SelectOptionAsync("Facility Address");
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Name")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Step 3: Fill parent fields + select Custom Address → partial loads
        await FillParentFields();
        await SelectCustomAddress();

        // Step 4: Submit with empty address → address errors inline, NO summary
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Step 5: Fill address → submit → client validation passes (all enriched fields valid)
        await Input("Address_Street").FillAsync("123 Sunrise Blvd");
        await Input("Address_City").FillAsync("Palm Springs");
        await Input("Address_ZipCode").FillAsync("92262");
        await SubmitBtn.ClickAsync();

        // Client-side passes — no inline errors, no summary
        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Address.Street")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Server may 400 (server validator covers full model)
        AssertNoConsoleErrorsExcept("400");
    }

    // ── Partial reload preserves validation ──────────────

    [Test]
    public async Task reloading_partial_replaces_html_and_revalidates()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        // Fill street
        await Input("Address_Street").FillAsync("Old St");

        // Switch to Facility → back to Custom (reload)
        await Input("AddressType").SelectOptionAsync("Facility Address");
        await Page.WaitForTimeoutAsync(500);
        await SelectCustomAddress();

        // Street should be fresh (DOM replaced)
        var streetVal = await Input("Address_Street").InputValueAsync();
        Assert.That(streetVal, Is.EqualTo(""));

        // Submit → address errors inline, NO summary
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    // ── EqualTo cross-field ─────────────────────────────

    [Test]
    public async Task confirm_email_mismatch_shows_inline_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("a@b.com");
        await Input("ConfirmEmail").FillAsync("x@y.com");

        await SubmitBtn.ClickAsync();

        await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");
        await Expect(ErrorFor("ConfirmEmail")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        // Fix → clear
        await Input("ConfirmEmail").FillAsync("a@b.com");
        await SubmitBtn.ClickAsync();
        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    // ── Partial reactive behavior ───────────────────────

    [Test]
    public async Task partial_zipcode_change_fires_own_reactive_entry()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SelectCustomAddress();

        await Input("Address_ZipCode").ClickAsync();
        await Input("Address_ZipCode").PressSequentiallyAsync("92262");
        await Page.Keyboard.PressAsync("Tab");

        var status = Page.Locator("#zipcode-status");
        await Expect(status).ToContainTextAsync("Zip validated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
