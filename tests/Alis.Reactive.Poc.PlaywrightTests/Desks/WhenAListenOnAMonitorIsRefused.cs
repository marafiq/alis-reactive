namespace Alis.Reactive.Poc.PlaywrightTests.Desks;

[TestFixture]
public sealed class WhenAListenOnAMonitorIsRefused : AppPageTest
{
    [Test]
    public async Task a_listen_on_a_component_reached_by_its_id_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Desks/ListenById");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: Listen reaches the component on the element 'ada-monitor' by its id. A component a plan renders is heard where it renders, with .Reactive; Listen hears a component an event's payload or a returned object hands over.");
    }

    [Test]
    public async Task a_listen_on_a_component_whose_vendor_names_no_element_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Desks/ListenWithoutElement");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: Listen reaches a component of the vendor 'older-desk-monitor', which names no element path, so the listen cannot tell when its host destroys it. Declare where the vendor's object holds its element: ComponentVendor.Define(.., elementPath: \"element\").");
    }
}
