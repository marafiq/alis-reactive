namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A validator the app registers by name (a generic one, which scanning does not find) checks the page that
/// names it; one never registered refuses the page that names it, saying how to register it; and a rule that
/// reads through a null says which rule and how to guard it.
/// </summary>
[TestFixture]
public sealed class WhenAValidatorIsRegistered : AppPageTest
{
    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_validator_registered_by_name_checks_the_page_that_names_it()
    {
        await Open("/Validation/Guest");

        await Page.GetByLabel("Guest", new() { Exact = true }).FillAsync("");
        await Click("Save");
        await Expect(Page.Locator("#guest-form").GetByText("Name the guest.", new() { Exact = true })).ToBeVisibleAsync();

        await Page.GetByLabel("Guest", new() { Exact = true }).FillAsync("Cy");
        await Click("Save");
        await Expect(Page.Locator("#saved")).ToHaveTextAsync("saved Cy");
    }

    [Test]
    public async Task a_validator_never_registered_refuses_the_page_that_names_it()
    {
        await Page.GotoAsync($"{BaseUrl}/Validation/UnregisteredGuest");

        await Expect(Page.GetByText("is registered for the page's browser rules. Register it with services.AddReactiveFluentValidation(validation => validation.Add<TValidator>()).").First).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_rule_that_reads_through_a_null_says_which_rule_and_how_to_guard_it()
    {
        await Open("/Validation/Guest");

        await Click("Save with the unguarded rule");

        await Expect(Page.Locator("#saved")).ToHaveTextAsync(
            "A rule read x => x.Address.City through a null. If a member on the way can be missing, guard the rule with When(x => x.Member != null, ...).");
    }
}
