namespace Alis.Reactive.Framework.PlaywrightTests.Partials;

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
