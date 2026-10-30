namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// A form whose request no validator checks, whose error route shows the server's errors on it
/// (<c>e.ValidationErrors(form)</c>): a message for a member the plan renders an input for comes back beside
/// that input, and one for a member it renders no input for comes back to the plan's summary, for a root
/// view's plan and for an element's plan of its own alike.
/// </summary>
[TestFixture]
public sealed class WhenOnlyTheServerChecksAForm : AppPageTest
{
    private ILocator Summary => Page.Locator("[data-reactive-validation-summary]");

    // The field's own wrapper in the form: its label, its input and its message slot, and nothing of another field.
    private ILocator Field(string form, string label) =>
        Page.Locator($"#{form} > div").Filter(new() { Has = Page.GetByLabel(label, new() { Exact = true }) });

    private async Task SignOut(string resident)
    {
        await Page.GetByLabel("Resident", new() { Exact = true }).FillAsync(resident);
        await Page.GetByLabel("Companion", new() { Exact = true }).FillAsync("Grace");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Sign the resident out", Exact = true }).ClickAsync();
    }

    [Test]
    public async Task a_server_message_for_an_input_the_page_renders_shows_beside_that_input()
    {
        await Open("/Validation/Outing");

        await SignOut("Mallory");

        await Expect(Field("outing-form", "Resident").GetByText("No resident of that name lives here.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_server_message_for_a_member_the_page_renders_no_input_for_shows_in_the_summary()
    {
        await Open("/Validation/Outing");

        await SignOut("Ada Lovelace");

        await Expect(Summary.GetByText("Say when the resident will be back.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_element_plans_server_message_for_an_input_it_renders_shows_beside_that_input()
    {
        await Open("/Validation/OutingCard");

        await SignOut("Mallory");

        await Expect(Field("outing-card", "Resident").GetByText("No resident of that name lives here.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_element_plans_server_message_for_a_member_it_renders_no_input_for_shows_in_its_summary()
    {
        await Open("/Validation/OutingCard");

        await SignOut("Ada Lovelace");

        await Expect(Summary.GetByText("Say when the resident will be back.")).ToBeVisibleAsync();
    }
}
