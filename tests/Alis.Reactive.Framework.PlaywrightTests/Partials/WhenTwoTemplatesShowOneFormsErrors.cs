namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A form's summary renders once per page (ADR-0003, grammar.md F6): two editor templates, each with a plan of its
/// own that shows the server's answer on the page's form, share the form's one summary, which the first of them
/// renders, and the second's message lands in it.
/// </summary>
[TestFixture]
public sealed class WhenTwoTemplatesShowOneFormsErrors : AppPageTest
{
    [Test]
    public async Task the_second_templates_message_lands_in_the_forms_one_summary()
    {
        await Open("/Partials/Escort");
        await Page.GetByLabel("The aide's name", new() { Exact = true }).FillAsync("Mo");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check the aide", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#escort-form_validation_summary")).ToHaveTextAsync("Say which shift the aide covers.");
        await Expect(Page.Locator("[data-reactive-validation-summary]")).ToHaveCountAsync(1);
    }
}
