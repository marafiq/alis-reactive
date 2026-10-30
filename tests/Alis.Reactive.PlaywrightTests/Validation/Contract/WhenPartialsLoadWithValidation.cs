using System.Linq.Expressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Validation.Contract;

[TestFixture]
public class WhenPartialsLoadWithValidation : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Validation/Contract/ServerPartial";

    private ILocator SubmitBtn => Page.Locator("#submit-btn");
    private ILocator SummaryDiv => Page.Locator("[data-reactive-validation-summary]");
    private ILocator Result => Page.Locator("#result");

    private ILocator ErrorFor(string fieldName) =>
        Page.Locator($"#resident-form span[data-valmsg-for='{fieldName}']");

    private ILocator Input(Expression<Func<ResidentModel, object?>> member) => Page.Locator($"#{IdGenerator.For(member)}");

    private async Task FillAllRequired()
    {
        await Input(m => m.Name).FillAsync("Jane Smith");
        await Input(m => m.Email).FillAsync("jane@care.com");
        await Input(m => m.ConfirmEmail).FillAsync("jane@care.com");
        await Input(m => m.CareLevel).SelectOptionAsync("Independent");
        await Input(m => m.Address.Street).FillAsync("123 Main St");
        await Input(m => m.Address.City).FillAsync("Springfield");
        await Input(m => m.Address.ZipCode).FillAsync("62704");
        await Input(m => m.ReasonForNoContact).FillAsync("No relatives nearby");
    }

    [Test]
    public async Task parent_field_errors_show_inline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToHaveTextAsync("'Name' is required.");
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToHaveTextAsync("'Email' is required.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_field_errors_show_inline()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Address.Street")).ToHaveTextAsync("'Street' is required.");
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToHaveTextAsync("'City' is required.");
        await Expect(ErrorFor("Address.ZipCode")).ToHaveTextAsync("'Zip Code' is required.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task all_unconditional_required_fields_across_parent_and_partials()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Email")).ToBeVisibleAsync();
        await Expect(ErrorFor("CareLevel")).ToBeVisibleAsync();

        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.City")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.ZipCode")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_where_condition_source_is_in_parent_and_field_is_in_parent()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.IsVeteran).CheckAsync();

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("VeteranId")).ToHaveTextAsync("'Veteran ID' is required when veteran.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task conditional_rule_where_field_is_in_partial()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Address.ZipCode).FillAsync("abc");

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("Address.ZipCode")).ToHaveTextAsync("'Zip Code' must be 5 digits.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task equalto_compares_two_parent_fields()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await FillAllRequired();
        await Input(m => m.Email).FillAsync("a@b.com");
        await Input(m => m.ConfirmEmail).FillAsync("x@y.com");

        await ClickWhenStable(SubmitBtn);

        await Expect(ErrorFor("ConfirmEmail")).ToHaveTextAsync("'Confirm Email' must match 'Email'.");
        await Expect(SummaryDiv).ToBeHiddenAsync();
        await Expect(Result).ToHaveTextAsync("");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task address_partial_zipcode_change_fires_partial_owned_dispatch()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Tab is required to fire the native change event.
        await Input(m => m.Address.ZipCode).ClickAsync();
        await Input(m => m.Address.ZipCode).PressSequentiallyAsync("90210");
        await Page.Keyboard.PressAsync("Tab");

        // Wait for the change-triggered dispatch to reach the matching CustomEvent behavior.
        var status = Page.Locator("#zipcode-status");
        await Expect(status).ToContainTextAsync("Zip validated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task emergency_partial_phone_change_updates_status()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Tab is required to fire the native change event.
        await Input(m => m.EmergencyPhone).ClickAsync();
        await Input(m => m.EmergencyPhone).PressSequentiallyAsync("555-0123");
        await Page.Keyboard.PressAsync("Tab");

        var status = Page.Locator("#phone-format-status");
        await Expect(status).ToContainTextAsync("Phone updated", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task partial_reactive_behaviors_do_not_interfere_with_parent_validation()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        // Partial dispatch should run before the parent validation submit path.
        await Input(m => m.Address.ZipCode).ClickAsync();
        await Input(m => m.Address.ZipCode).PressSequentiallyAsync("90210");
        await Page.Keyboard.PressAsync("Tab");
        await Expect(Page.Locator("#zipcode-status"))
            .ToContainTextAsync("Zip validated", new() { Timeout = 5000 });

        await FillAllRequired();
        await ClickWhenStable(SubmitBtn);

        var result = Page.Locator("#result");
        await Expect(result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task fixing_all_fields_across_parent_and_partials_results_in_success()
    {
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 5000);

        await ClickWhenStable(SubmitBtn);
        await Expect(ErrorFor("Name")).ToBeVisibleAsync();
        await Expect(ErrorFor("Address.Street")).ToBeVisibleAsync();
        await Expect(SummaryDiv).ToBeHiddenAsync();

        await FillAllRequired();
        await ClickWhenStable(SubmitBtn);

        await Expect(Result).ToContainTextAsync("Admission saved", new() { Timeout = 5000 });
        await Expect(SummaryDiv).ToBeHiddenAsync();
        AssertNoConsoleErrors();
    }
}
