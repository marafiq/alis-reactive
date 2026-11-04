namespace Alis.Reactive.Framework.PlaywrightTests.Plugins;

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

    [Test]
    public async Task a_data_member_named_its_own_way_reaches_the_plugin_by_that_name()
    {
        await Open("/Plugins/Tally");

        await Button("Add an entry of 5").ClickAsync();
        await Button("Read the total").ClickAsync();

        await Expect(Page.Locator("#total")).ToHaveTextAsync("5");
    }

    [Test]
    public async Task a_date_member_named_its_own_way_reaches_the_plugin_as_a_date()
    {
        await Open("/Plugins/Tally");

        await Button("Add the entry the office keeps").ClickAsync();

        await Expect(Page.Locator("#last-day")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_member_of_data_the_plugin_does_not_hold_yet_reads_as_nothing()
    {
        await Open("/Plugins/Tally");

        await Button("Read the last amount added").ClickAsync();

        await Expect(Page.Locator("#last-amount")).ToHaveTextAsync("nothing added yet");
    }

    [Test]
    public async Task a_member_of_data_the_plugin_holds_is_read()
    {
        await Open("/Plugins/Tally");
        await Button("Add 4").ClickAsync();

        await Button("Read the last amount added").ClickAsync();

        await Expect(Page.Locator("#last-amount")).ToHaveTextAsync("4");
    }
}
