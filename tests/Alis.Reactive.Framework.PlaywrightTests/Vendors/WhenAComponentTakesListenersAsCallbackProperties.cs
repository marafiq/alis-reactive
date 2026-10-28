namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

/// <summary>
/// A component that takes listeners by assignment to a callback property (V2): the page's pipeline is
/// assigned beside the component's own callback, and both run.
/// </summary>
[TestFixture]
public sealed class WhenAComponentTakesListenersAsCallbackProperties : AppPageTest
{
    private ILocator Bell => Page.Locator("#bell");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task the_pipeline_assigned_to_the_callback_hears_each_ring_with_its_payload()
    {
        await Open("/Vendors/CallbackProperties");

        await Button("Ring the bell").ClickAsync();
        await Expect(Page.Locator("#rang")).ToHaveTextAsync("1");

        await Button("Ring the bell").ClickAsync();
        await Expect(Page.Locator("#rang")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task the_components_own_callback_keeps_running_beside_the_pipeline()
    {
        await Open("/Vendors/CallbackProperties");
        await Expect(Bell.GetByText("Not rung yet")).ToBeVisibleAsync();

        await Button("Ring the bell").ClickAsync();
        await Button("Ring the bell").ClickAsync();

        await Expect(Bell.GetByText("Rung 2 times")).ToBeVisibleAsync();
        await Expect(Page.Locator("#rang")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_method_call_from_the_page_raises_the_event_and_a_property_reads_the_count()
    {
        await Open("/Vendors/CallbackProperties");

        await Button("Ring from the page").ClickAsync();
        await Expect(Page.Locator("#rang")).ToHaveTextAsync("1");

        await Button("Read the rings").ClickAsync();
        await Expect(Page.Locator("#rings")).ToHaveTextAsync("1");
    }
}
