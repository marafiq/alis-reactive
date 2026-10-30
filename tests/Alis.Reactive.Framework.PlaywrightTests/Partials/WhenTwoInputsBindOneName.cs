namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// Midway through a changeover the desk renders its next of kin twice, through the next of kin's own card and
/// through the community's person line, so two inputs of the form bind <c>NextOfKin.Name</c>. The form sends the
/// one that joined the page last (grammar.md M2), and the page reads the member as that request binds it
/// (grammar.md M8): <c>p.Bound</c> and a named send take the same input.
/// </summary>
[TestFixture]
public sealed class WhenTwoInputsBindOneName : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    private async Task NameTheNextOfKinTwice()
    {
        await Open("/Partials/Changeover");
        await Input("Next of kin").FillAsync("Ann");
        await Input("Next of kin's name").FillAsync("Bo");
    }

    [Test]
    public async Task the_bound_value_of_the_member_is_the_input_the_form_sends()
    {
        await NameTheNextOfKinTwice();
        await Button("Save the next of kin").ClickAsync();
        await Expect(Page.Locator("#changeover-saved")).ToHaveTextAsync("next of kin Bo");

        await Button("Read the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-bound")).ToHaveTextAsync("Bo");
    }

    [Test]
    public async Task a_named_send_of_the_member_sends_the_input_the_form_sends()
    {
        await NameTheNextOfKinTwice();
        await Button("Save the next of kin").ClickAsync();
        await Expect(Page.Locator("#changeover-saved")).ToHaveTextAsync("next of kin Bo");

        await Button("Note the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-noted")).ToHaveTextAsync("next of kin Bo");
    }
}
