namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenAPartialIsRenderedForAListItem : AppPageTest
{
    private ILocator Bound => Page.Locator("#circle-bound");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    [Test]
    public async Task a_rendering_sends_its_inputs_at_its_items_position_in_the_list()
    {
        await Open("/Inputs/Circle");
        await Input("Second member name").FillAsync("Bea");

        await Button("Save the second member").ClickAsync();

        await Expect(Bound).ToHaveTextAsync("member 0 none, member 1 Bea");
    }

    [Test]
    public async Task the_first_items_rendering_sends_its_inputs_at_the_first_position()
    {
        await Open("/Inputs/Circle");
        await Input("First member name").FillAsync("Ann");

        await Button("Save the first member").ClickAsync();

        await Expect(Bound).ToHaveTextAsync("member 0 Ann");
    }
}
