using System.Text.RegularExpressions;
using Alis.Reactive.PlaywrightTests.Support.Pages;

namespace Alis.Reactive.PlaywrightTests.Validation.Rules.AllRules;

[TestFixture]
public sealed class WhenBasicFieldRulesRun : PlaywrightTestBase
{
    private ValidationShowcasePage Showcase => new(Page, () => NavigateToAndWaitForReady("/Sandbox/Validation/AllRules"));

    [Test]
    public async Task empty_form_shows_required_errors()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);

        await Expect(Showcase.ErrorFor("AllRules_Name")).ToContainTextAsync("required");
        await Expect(Showcase.ErrorFor("AllRules_Email")).ToContainTextAsync("required");
        await Expect(Showcase.Input("AllRules_Name")).ToHaveClassAsync(new Regex("alis-has-error"));

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task password_rule_clears_after_valid_input()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Showcase.Input("AllRules_Password").FillAsync("abc");
        await Showcase.Input("AllRules_Password").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Password")).ToContainTextAsync("at least 8", new() { Timeout = 2000 });

        await Showcase.Input("AllRules_Password").FillAsync("securepassword");
        await Showcase.Input("AllRules_Password").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Password")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task name_error_clears_on_blur_after_fix()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Expect(Showcase.ErrorFor("AllRules_Name")).ToContainTextAsync("required");

        await Showcase.Input("AllRules_Name").FillAsync("Margaret Thompson");
        await Showcase.Input("AllRules_Name").BlurAsync();

        await Expect(Showcase.ErrorFor("AllRules_Name")).ToBeHiddenAsync(new() { Timeout = 2000 });
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task email_rule_rejects_bad_format_and_clears_after_fix()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Showcase.Input("AllRules_Email").FillAsync("notanemail");
        await Showcase.Input("AllRules_Email").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Email")).ToContainTextAsync("valid email", new() { Timeout = 2000 });

        await Showcase.Input("AllRules_Email").FillAsync("margaret@care.com");
        await Showcase.Input("AllRules_Email").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Email")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task age_rule_rejects_out_of_range_values()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Showcase.Input("AllRules_Age").FillAsync("150");
        await Showcase.Input("AllRules_Age").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Age")).ToContainTextAsync("between", new() { Timeout = 2000 });

        await Showcase.Input("AllRules_Age").FillAsync("75");
        await Showcase.Input("AllRules_Age").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Age")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task phone_rule_requires_expected_format()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Showcase.Input("AllRules_Phone").FillAsync("badphone");
        await Showcase.Input("AllRules_Phone").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Phone")).ToContainTextAsync("123-456-7890", new() { Timeout = 2000 });

        await Showcase.Input("AllRules_Phone").FillAsync("123-456-7890");
        await Showcase.Input("AllRules_Phone").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Phone")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task salary_rule_rejects_values_over_the_limit()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Showcase.Input("AllRules_Salary").FillAsync("600000");
        await Showcase.Input("AllRules_Salary").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Salary")).ToContainTextAsync("500,000", new() { Timeout = 2000 });

        await Showcase.Input("AllRules_Salary").FillAsync("75000");
        await Showcase.Input("AllRules_Salary").BlurAsync();
        await Expect(Showcase.ErrorFor("AllRules_Salary")).ToBeHiddenAsync(new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task invalid_fields_gain_and_lose_error_class_as_they_change()
    {
        await Showcase.Open();

        await ClickWhenStable(Showcase.ValidateAllRulesButton);
        await Expect(Showcase.Input("AllRules_Name")).ToHaveClassAsync(new Regex("alis-has-error"));
        await Expect(Showcase.Input("AllRules_Email")).ToHaveClassAsync(new Regex("alis-has-error"));

        await Showcase.Input("AllRules_Name").FillAsync("Valid Name");
        await Showcase.Input("AllRules_Name").BlurAsync();
        await Expect(Showcase.Input("AllRules_Name"))
            .Not.ToHaveClassAsync(new Regex("alis-has-error"), new() { Timeout = 2000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task valid_all_rules_form_shows_success_message()
    {
        await Showcase.Open();

        await Showcase.Input("AllRules_Name").FillAsync("Dorothy Henderson");
        await Showcase.Input("AllRules_Email").FillAsync("dorothy@seniorcare.com");
        await Showcase.Input("AllRules_Age").FillAsync("82");
        await Showcase.Input("AllRules_Phone").FillAsync("503-555-1234");
        await Showcase.Input("AllRules_Salary").FillAsync("45000");
        await Showcase.Input("AllRules_Password").FillAsync("securepass123");

        await ClickWhenStable(Showcase.ValidateAllRulesButton);

        await Expect(Showcase.AllRulesResult).ToContainTextAsync("passed", new() { Timeout = 5000 });
        await Expect(Showcase.AllRulesResult).ToHaveClassAsync(new Regex("text-green-600"));

        AssertNoConsoleErrors();
    }
}
