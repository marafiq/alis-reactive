using System.Linq.Expressions;
using System.Text.Json;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenAjaxPartialsLoadWithValidation : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract/AjaxPartial";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-reactive-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(Expression<Func<ResidentModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");

    private async Task SelectCustomAddress()
    {
        await Input(m => m.AddressType).SelectOptionAsync("Custom Address");
        await Expect(Input(m => m.Address.Street)).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Input(m => m.Address.City)).ToBeVisibleAsync(new() { Timeout = 5000 });
        await Expect(Input(m => m.Address.ZipCode)).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task FillParentFields()
    {
        await Input(m => m.Name).FillAsync("Jane Smith");
        await Input(m => m.Email).FillAsync("jane@care.com");
        await Input(m => m.ConfirmEmail).FillAsync("jane@care.com");
    }

    private async Task AddEmergencyContact()
    {
        await ClickWhenStable(Page.Locator("#add-emergency-contact-btn"));
        await Expect(Input(m => m.EmergencyName)).ToBeVisibleAsync(new() { Timeout = 5000 });
    }

    private async Task FillCustomAddress()
    {
        await Input(m => m.Address.Street).FillAsync("123 Sunrise Blvd");
        await Input(m => m.Address.City).FillAsync("Palm Springs");
        await Input(m => m.Address.ZipCode).FillAsync("92262");
    }

    private Task<IRequest> SubmitAndCaptureRequest() =>
        Page.RunAndWaitForRequestAsync(
            () => ClickWhenStable(SubmitBtn),
            request => request.Method == "POST" && request.Url.Contains("/SubmitAjaxPartial"));

    [Test]
    public async Task full_ajax_partial_lifecycle()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Name")).ToHaveTextAsync("'Name' is required.");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToHaveTextAsync("'Email' is required.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");

        await Input(m => m.AddressType).SelectOptionAsync("Facility Address");
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Name")).ToHaveTextAsync("'Name' is required.");
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await FillParentFields();
        await SelectCustomAddress();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToHaveTextAsync("'Street' is required.");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToHaveTextAsync("'City' is required.");
        await Expect(ErrorFor("Address.ZipCode")).ToHaveTextAsync("'Zip Code' is required.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");

        await FillParentFields();
        await Input(m => m.Address.Street).FillAsync("123 Sunrise Blvd");
        await Input(m => m.Address.City).FillAsync("Palm Springs");
        await Input(m => m.Address.ZipCode).FillAsync("92262");
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

        await Input(m => m.Address.Street).FillAsync("Old St");

        await Input(m => m.AddressType).SelectOptionAsync("Facility Address");
        await Expect(Input(m => m.Address.Street)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await SelectCustomAddress();

        var streetVal = await Input(m => m.Address.Street).InputValueAsync();
        Assert.That(streetVal, Is.EqualTo(""));

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToHaveTextAsync("'Street' is required.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    // With no address chosen the address rules do not apply, so the corrected form is saved.
    [Test]
    public async Task confirm_email_mismatch_shows_inline_error()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await Input(m => m.Name).FillAsync("Jane Smith");
        await Input(m => m.Email).FillAsync("a@b.com");
        await Input(m => m.ConfirmEmail).FillAsync("x@y.com");

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("ConfirmEmail")).ToHaveTextAsync("'Confirm Email' must match 'Email'.");
        await Expect(ErrorFor("ConfirmEmail")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");

        await Input(m => m.ConfirmEmail).FillAsync("a@b.com");
        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("ConfirmEmail")).Not.ToBeVisibleAsync();
        await Expect(Result).ToHaveTextAsync("Admission saved", new() { Timeout = 5000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task live_clear_works_on_reloaded_partial_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillParentFields();
        await SelectCustomAddress();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToHaveTextAsync("'Street' is required.");

        await Input(m => m.Address.Street).FillAsync("123 Sunrise Blvd");
        await Expect(ErrorFor("Address.Street")).ToBeHiddenAsync();

        await Input(m => m.AddressType).SelectOptionAsync("Facility Address");
        await Expect(Input(m => m.Address.Street)).ToBeHiddenAsync(new() { Timeout = 5000 });
        await SelectCustomAddress();

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Address.Street")).ToHaveTextAsync("'Street' is required.");

        await Input(m => m.Address.Street).FillAsync("456 Palm Ave");
        await Expect(ErrorFor("Address.Street")).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_zipcode_change_fires_own_reactive_behavior()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await SelectCustomAddress();

        await Input(m => m.Address.ZipCode).ClickAsync();
        await Input(m => m.Address.ZipCode).PressSequentiallyAsync("92262");
        await Page.Keyboard.PressAsync("Tab");

        var status = Page.Locator("#zipcode-status");
        await Expect(status).ToContainTextAsync("Zip validated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Choosing the facility address empties the address container without unloading its slot, so its
    // inputs stay registered while they are off the page; Form() sends only the inputs on the page.
    [Test]
    public async Task address_fields_cleared_by_choosing_the_facility_address_are_not_saved()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await FillParentFields();
        await SelectCustomAddress();
        await Input(m => m.Address.Street).FillAsync("Old St");

        await Input(m => m.AddressType).SelectOptionAsync("Facility Address");
        await Expect(Input(m => m.Address.Street)).ToHaveCountAsync(0, new() { Timeout = 5000 });
        var submit = await SubmitAndCaptureRequest();

        using (var body = JsonDocument.Parse(submit.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.TryGetProperty("Address", out _), Is.False, submit.PostData);
            Assert.That(body.RootElement.GetProperty("AddressType").GetString(), Is.EqualTo("Facility Address"), submit.PostData);
        }
        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // The address and the emergency contact are partials of the page's own plan, loaded into two slots;
    // the inputs of both are the form's, and Submit sends them together.
    [Test]
    public async Task an_emergency_contact_loaded_beside_the_address_is_saved_with_it()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await FillParentFields();
        await SelectCustomAddress();
        await AddEmergencyContact();
        await FillCustomAddress();
        await Input(m => m.EmergencyName).FillAsync("Ruth Hale");

        var submit = await SubmitAndCaptureRequest();

        using (var body = JsonDocument.Parse(submit.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.GetProperty("Address").GetProperty("Street").GetString(), Is.EqualTo("123 Sunrise Blvd"), submit.PostData);
            Assert.That(body.RootElement.GetProperty("EmergencyName").GetString(), Is.EqualTo("Ruth Hale"), submit.PostData);
        }
        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    // Reloading the address replaces only the address slot: the emergency contact's inputs stay the form's.
    [Test]
    public async Task reloading_the_address_keeps_the_emergency_contact_loaded_beside_it()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);
        await FillParentFields();
        await SelectCustomAddress();
        await AddEmergencyContact();
        await Input(m => m.EmergencyName).FillAsync("Ruth Hale");

        await Input(m => m.AddressType).SelectOptionAsync("Facility Address");
        await Expect(Input(m => m.Address.Street)).ToHaveCountAsync(0, new() { Timeout = 5000 });
        await SelectCustomAddress();
        await FillCustomAddress();
        var submit = await SubmitAndCaptureRequest();

        using (var body = JsonDocument.Parse(submit.PostData ?? "{}"))
        {
            Assert.That(body.RootElement.GetProperty("EmergencyName").GetString(), Is.EqualTo("Ruth Hale"), submit.PostData);
            Assert.That(body.RootElement.GetProperty("Address").GetProperty("Street").GetString(), Is.EqualTo("123 Sunrise Blvd"), submit.PostData);
        }
        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }
}
