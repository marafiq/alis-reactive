namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A partial typed to a base class of the page's model renders a member the base class declares (ADR-0003
/// rule 1): its input's id is scoped by that class, so the page reaches it by the member, and a rule written
/// once for an interface the class implements, read on the page's model, finds the same input.
/// </summary>
[TestFixture]
public sealed class WhenAPartialIsTypedToABaseClassOfThePagesModel : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator VisitorField => Page.Locator("#visitor-line > div").Filter(new() { Has = Page.GetByLabel("Visitor", new() { Exact = true }) });

    [Test]
    public async Task the_page_reads_the_partials_input_by_the_member()
    {
        await Open("/Partials/Lobby");
        await Page.GetByLabel("Visitor", new() { Exact = true }).FillAsync("Ada");

        await Button("Read the visitor").ClickAsync();

        await Expect(Page.Locator("#visitor-read")).ToHaveTextAsync("Ada");
    }

    [Test]
    public async Task a_rule_written_for_an_interface_the_class_implements_shows_its_message_beside_the_partials_field()
    {
        await Open("/Partials/Lobby");

        await Button("Sign in").ClickAsync();

        await Expect(VisitorField.GetByText("Name the visitor who signs in.")).ToBeVisibleAsync();
        await Expect(Page.Locator("#lobby-messages [data-reactive-validation-summary]")).ToBeHiddenAsync();
    }
}
