namespace Alis.Reactive.Poc.PlaywrightTests.Vendors;

[TestFixture]
public sealed class WhenAVendorFindsItsObjectsThroughAPlugin : AppPageTest
{
    [Test]
    public async Task a_step_reads_a_property_of_a_component_its_vendor_finds_through_a_plugin()
    {
        await Open("/Oxygen/Gauges");

        await Button("Read the gauge").ClickAsync();

        await Expect(Page.Locator("#reading")).ToHaveTextAsync("96%");
    }

    [Test]
    public async Task a_step_calls_a_method_of_a_component_its_vendor_finds_through_a_plugin()
    {
        await Open("/Oxygen/Gauges");
        await Expect(Page.Locator("#gauge .gauge-face")).ToHaveTextAsync("96%");

        await Button("Lower the gauge by 4").ClickAsync();

        await Expect(Page.Locator("#gauge .gauge-face")).ToHaveTextAsync("92%");
    }

    [Test]
    public async Task a_handler_runs_on_an_event_of_a_component_its_vendor_finds_through_a_plugin()
    {
        await Open("/Oxygen/Gauges");
        await Button("Lower the gauge by 4").ClickAsync();
        await Expect(Page.Locator("#alarm")).ToHaveTextAsync("quiet");

        await Button("Lower the gauge by 4").ClickAsync();

        await Expect(Page.Locator("#alarm")).ToHaveTextAsync("Saturation 88%");
    }

    [Test]
    public async Task a_step_writes_a_property_of_a_component_its_vendor_finds_through_a_plugin()
    {
        await Open("/Oxygen/Gauges");

        await Button("Alarm below 95%").ClickAsync();
        await Button("Lower the gauge by 4").ClickAsync();

        await Expect(Page.Locator("#alarm")).ToHaveTextAsync("Saturation 92%");
    }

    [Test]
    public async Task a_step_reaching_an_element_whose_lookup_finds_no_object_stops_only_its_own_pipeline()
    {
        await Open("/Oxygen/Gauges");

        await Button("Read the spare gauge").ClickAsync();
        await Expect(Page.Locator("#spare-tried")).ToHaveTextAsync("tried");
        await Button("Greet the nurse").ClickAsync();

        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await Expect(Page.Locator("#spare-reading")).ToHaveTextAsync("none read");
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
}
