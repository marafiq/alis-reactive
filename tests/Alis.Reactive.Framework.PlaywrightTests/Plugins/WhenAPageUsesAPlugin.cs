namespace Alis.Reactive.Framework.PlaywrightTests.Plugins;

/// <summary>
/// A plugin the app registers by name, with no element (G1): its methods change what its properties read,
/// a property is written, and a method's result decides a branch.
/// </summary>
[TestFixture]
public sealed class WhenAPageUsesAPlugin : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_plugin_method_changes_what_its_property_reads()
    {
        await Open("/Plugins/Tally");

        await Button("Read the total").ClickAsync();
        await Expect(Page.Locator("#total")).ToHaveTextAsync("0");

        await Button("Add 4").ClickAsync();
        await Button("Add 4").ClickAsync();
        await Button("Read the total").ClickAsync();
        await Expect(Page.Locator("#total")).ToHaveTextAsync("8");
    }

    [Test]
    public async Task a_property_written_on_the_plugin_changes_what_its_method_answers()
    {
        await Open("/Plugins/Tally");
        await Button("Add 4").ClickAsync();

        await Button("Has the tally passed the mark?").ClickAsync();
        await Expect(Page.Locator("#against-mark")).ToHaveTextAsync("not yet");

        await Button("Lower the mark to 3").ClickAsync();
        await Button("Has the tally passed the mark?").ClickAsync();
        await Expect(Page.Locator("#against-mark")).ToHaveTextAsync("passed");
    }
}
