using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.AllRules;

[TestFixture]
public sealed class WhenServerValidationResponds : PlaywrightTestBase
{
    private ValidationShowcasePage Showcase => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/AllRules"));

    [Test]
    public async Task empty_server_form_shows_required_errors()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateServerButton);

        await Expect(Showcase.ErrorFor("Server_Name")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(Showcase.ErrorFor("Server_Email")).ToContainTextAsync("required", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task incomplete_server_submission_routes_validation_errors_back_to_the_page()
    {
        await Showcase.Open();

        await Showcase.Input("Server_Name").FillAsync("Harold Wilson");
        await Showcase.Input("Server_Email").FillAsync("harold@care.com");
        await ClickWhenStable(Showcase.ValidateServerButton);

        await Expect(Showcase.ServerResult).ToContainTextAsync("Server returned validation errors", new() { Timeout = 5000 });
        await Expect(Showcase.ServerResult).ToHaveClassAsync(new Regex("text-red-600"));

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task invalid_server_email_is_blocked_before_the_request_runs()
    {
        await Showcase.Open();

        await Showcase.Input("Server_Name").FillAsync("Harold Wilson");
        await Showcase.Input("Server_Email").FillAsync("not-an-email");
        await ClickWhenStable(Showcase.ValidateServerButton);

        await Expect(Showcase.ErrorFor("Server_Email")).ToContainTextAsync("valid email", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }
}
