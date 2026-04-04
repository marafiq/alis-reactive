using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.AllRules;

[TestFixture]
public sealed class WhenDatabaseValidationRejectsConflicts : PlaywrightTestBase
{
    private ValidationShowcasePage Showcase => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/AllRules"));

    [Test]
    public async Task empty_database_form_shows_required_errors()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateDatabaseButton);

        await Expect(Showcase.ErrorFor("Db_Name")).ToContainTextAsync("required", new() { Timeout = 2000 });
        await Expect(Showcase.ErrorFor("Db_Email")).ToContainTextAsync("required", new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reserved_name_is_reported_as_a_server_validation_error()
    {
        await Showcase.Open();

        await Showcase.Input("Db_Name").FillAsync("admin");
        await Showcase.Input("Db_Email").FillAsync("valid@test.com");
        await ClickWhenStable(Showcase.ValidateDatabaseButton);

        await Expect(Showcase.DatabaseResult).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });
        await Expect(Showcase.ErrorFor("Db_Name")).ToContainTextAsync("reserved", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task taken_email_is_reported_as_a_server_validation_error()
    {
        await Showcase.Open();

        await Showcase.Input("Db_Name").FillAsync("validuser");
        await Showcase.Input("Db_Email").FillAsync("taken@test.com");
        await ClickWhenStable(Showcase.ValidateDatabaseButton);

        await Expect(Showcase.DatabaseResult).ToContainTextAsync("Database validation failed", new() { Timeout = 5000 });
        await Expect(Showcase.ErrorFor("Db_Email")).ToContainTextAsync("already registered", new() { Timeout = 2000 });

        AssertNoConsoleErrorsExcept("400");
    }

    [Test]
    public async Task fresh_database_values_show_success()
    {
        await Showcase.Open();

        await Showcase.Input("Db_Name").FillAsync("newuser");
        await Showcase.Input("Db_Email").FillAsync("newuser@example.com");
        await ClickWhenStable(Showcase.ValidateDatabaseButton);

        await Expect(Showcase.DatabaseResult).ToContainTextAsync("Saved to database", new() { Timeout = 5000 });
        await Expect(Showcase.DatabaseResult).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
