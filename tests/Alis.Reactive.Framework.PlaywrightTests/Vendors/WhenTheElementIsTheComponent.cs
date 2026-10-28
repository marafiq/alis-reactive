namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

/// <summary>
/// A component whose element is its object and whose events are DOM events (V3): its properties are the
/// element's, and a listener's payload is the event's detail.
/// </summary>
[TestFixture]
public sealed class WhenTheElementIsTheComponent : AppPageTest
{
    private ILocator Meter => Page.Locator("#meter");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task the_elements_own_properties_are_read_and_written()
    {
        await Open("/Vendors/Element");

        await Button("Read the level").ClickAsync();
        await Expect(Page.Locator("#level")).ToHaveTextAsync("3");
        await Expect(Page.Locator("#max")).ToHaveTextAsync("10");

        await Button("Set the level to 7").ClickAsync();
        await Expect(Meter.GetByText("Level 7 of 10")).ToBeVisibleAsync();
        await Button("Read the level").ClickAsync();
        await Expect(Page.Locator("#level")).ToHaveTextAsync("7");
    }

    [Test]
    public async Task a_dom_event_hands_the_pipeline_its_detail()
    {
        await Open("/Vendors/Element");

        await Meter.GetByRole(AriaRole.Button, new() { Name = "Raise" }).ClickAsync();

        await Expect(Page.Locator("#changed-to")).ToHaveTextAsync("4");
        await Expect(Page.Locator("#changed-from")).ToHaveTextAsync("3");
    }
}
