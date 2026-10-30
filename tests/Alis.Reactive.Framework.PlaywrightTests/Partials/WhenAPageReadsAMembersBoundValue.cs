namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

/// <summary>
/// <c>p.Bound(m =&gt; m.Member)</c> reads a member of the page's model as MVC binds it (ADR-0003, grammar.md M5):
/// the input a partial renders for it, whichever plan rendered it, and missing while no input renders the member,
/// where a component read would fail at the DOM boundary.
/// </summary>
[TestFixture]
public sealed class WhenAPageReadsAMembersBoundValue : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator Input(string label) => Page.GetByLabel(label, new() { Exact = true });

    [Test]
    public async Task the_bound_value_of_a_member_no_input_renders_reads_as_missing()
    {
        await Open("/Partials/Reception");

        await Button("Read the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-bound")).ToHaveTextAsync("no next of kin");
    }

    [Test]
    public async Task the_bound_value_of_a_member_a_loaded_template_renders_is_what_its_input_holds()
    {
        await Open("/Partials/Reception");
        await Button("Add the next of kin").ClickAsync();
        await Input("Next of kin").FillAsync("Bo");

        await Button("Read the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-bound")).ToHaveTextAsync("Bo");
    }
}
