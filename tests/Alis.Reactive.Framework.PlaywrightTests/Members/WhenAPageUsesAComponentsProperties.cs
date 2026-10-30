namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// A component's properties, read and written by a page's pipelines: read-only (P1), write-only (P2),
/// read and written (P3) on a slate.
/// </summary>
[TestFixture]
public sealed class WhenAPageUsesAComponentsProperties : AppPageTest
{
    private ILocator Slate => Page.Locator("#slate");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_read_only_property_reads_what_the_component_holds_when_the_pipeline_runs()
    {
        await Open("/Members/Properties");

        await Button("Count rows").ClickAsync();
        await Expect(Page.Locator("#row-count")).ToHaveTextAsync("2");

        await Button("Add row").ClickAsync();
        await Expect(Slate.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(3);
        await Button("Count rows").ClickAsync();
        await Expect(Page.Locator("#row-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_write_only_property_written_by_a_pipeline_is_what_the_component_shows()
    {
        await Open("/Members/Properties");
        await Expect(Slate.GetByText("Back at three")).ToHaveCountAsync(0);

        await Button("Leave a note").ClickAsync();

        await Expect(Slate.GetByText("Back at three")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_read_write_property_reads_its_value_and_takes_a_new_one()
    {
        await Open("/Members/Properties");

        await Button("Read title").ClickAsync();
        await Expect(Page.Locator("#title-read")).ToHaveTextAsync("Day round");

        await Button("Rename").ClickAsync();
        await Expect(Slate.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Night round");

        await Button("Read title").ClickAsync();
        await Expect(Page.Locator("#title-read")).ToHaveTextAsync("Night round");
    }
}
