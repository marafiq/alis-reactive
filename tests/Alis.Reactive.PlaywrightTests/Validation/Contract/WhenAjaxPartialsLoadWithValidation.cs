namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenAjaxPartialsLoadWithValidation : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract/AjaxPartial";
    private const string ModelIdPrefix = "Alis_Reactive_SandboxApp_Areas_Sandbox_Models_ResidentModel__";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-reactive-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(string suffix) => Page.Locator($"#{ModelIdPrefix}{suffix}");

    private async Task SelectCustomAddress()
    {
        await Input("AddressType").SelectOptionAsync("Custom Address");
        await Expect(Input("Address_Street")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Input("Address_City")).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Input("Address_ZipCode")).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task FillParentFields()
    {
        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("jane@care.com");
        await Input("ConfirmEmail").FillAsync("jane@care.com");
    }

    [Test]
    public async Task full_ajax_partial_lifecycle()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Name")).ToContainTextAsync("required");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input("AddressType").SelectOptionAsync("Facility Address");
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Name")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await FillParentFields();
        await SelectCustomAddress();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToContainTextAsync("required");
        await Expect(ErrorFor("Address.ZipCode")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await FillParentFields();
        await Input("Address_Street").FillAsync("123 Sunrise Blvd");
        await Input("Address_City").FillAsync("Palm Springs");
        await Input("Address_ZipCode").FillAsync("92262");
        await ClickWhenStable(SubmitBtn);

        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(ErrorFor("Name")).Not.ToBeVisibleAsync();
        await Expect(ErrorFor("Address.Street")).Not.ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reloading_partial_replaces_html_and_revalidates()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        await Input("Address_Street").FillAsync("Old St");

        await Input("AddressType").SelectOptionAsync("Facility Address");
        await Expect(Input("Address_Street")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await SelectCustomAddress();

        var streetVal = await Input("Address_Street").InputValueAsync();
        Assert.That(streetVal, Is.EqualTo(""));

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task confirm_email_mismatch_shows_inline_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input("Name").FillAsync("Jane Smith");
        await Input("Email").FillAsync("a@b.com");
        await Input("ConfirmEmail").FillAsync("x@y.com");

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("ConfirmEmail")).ToContainTextAsync("must match");
        await Expect(ErrorFor("ConfirmEmail")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await Input("ConfirmEmail").FillAsync("a@b.com");
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync();

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task live_clear_works_on_reloaded_partial_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");

        await Input("Address_Street").FillAsync("123 Sunrise Blvd");
        await Expect(ErrorFor("Address.Street")).ToBeHiddenAsync();

        await Input("AddressType").SelectOptionAsync("Facility Address");
        await Expect(Input("Address_Street")).ToBeHiddenAsync(new() { Timeout = 5000 });
        await SelectCustomAddress();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToContainTextAsync("required");

        await Input("Address_Street").FillAsync("456 Palm Ave");
        await Expect(ErrorFor("Address.Street")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

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
