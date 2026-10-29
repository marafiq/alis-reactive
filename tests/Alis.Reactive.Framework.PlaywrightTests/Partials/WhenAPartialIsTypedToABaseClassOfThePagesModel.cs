namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// A partial typed to a base class of the page's model renders a member the base class declares (ADR-0003
/// rule 1): its input's id is scoped by that class, so the page reaches it by the member as well.
/// </summary>
[TestFixture]
public sealed class WhenAPartialIsTypedToABaseClassOfThePagesModel : AppPageTest
{
    [Test]
    public async Task the_page_reads_the_partials_input_by_the_member()
    {
        await Open("/Partials/Lobby");
        await Page.GetByLabel("Visitor", new() { Exact = true }).FillAsync("Ada");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the visitor", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#visitor-read")).ToHaveTextAsync("Ada");
    }
}
