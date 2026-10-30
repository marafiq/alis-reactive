namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A form's inputs sent as form data (<c>g.Form(formId)</c> with <c>AsFormData()</c>), each by its binding name, into
/// the endpoint's <c>[FromForm]</c> model as MVC binds a form post (H4, M2): a line per row of a list
/// (<c>Lines[0].Sku</c>, <c>Lines[1].Sku</c>), a nested member (<c>Address.City</c>), and a multi-value input
/// (a list of text) as its key repeated once per value, or no key while it holds none. Members the request sets
/// below a list item and a nested member (<c>Set(x =&gt; x.Lines[0].Sku, ..)</c>) are sent by the same names.
/// </summary>
[TestFixture]
public sealed class WhenAFormIsSentAsFormData : AppPageTest
{
    private ILocator Received => Page.Locator("#received");

    private Task Type(string label, string text) => Page.GetByLabel(label, new() { Exact = true }).FillAsync(text);

    private Task Tick(string option) => Page.GetByLabel(option, new() { Exact = true }).CheckAsync();

    private Task Send() => Page.GetByRole(AriaRole.Button, new() { Name = "Send the order", Exact = true }).ClickAsync();

    [Test]
    public async Task each_row_of_a_list_binds_into_its_own_item_of_the_endpoints_list()
    {
        await Open("/Requests/SupplyOrder");
        await Type("Item on line 1", "GLV-1");
        await Type("Quantity on line 1", "2");
        await Type("Item on line 2", "MSK-4");
        await Type("Quantity on line 2", "10");

        await Send();

        await Expect(Received).ToHaveTextAsync("lines GLV-1 x 2 and MSK-4 x 10; city none; wings none from 0 Wings fields");
    }

    [Test]
    public async Task a_nested_member_binds_into_the_endpoints_nested_object()
    {
        await Open("/Requests/SupplyOrder");
        await Type("Deliver to", "Albany");

        await Send();

        await Expect(Received).ToHaveTextAsync("lines nothing x 0 and nothing x 0; city Albany; wings none from 0 Wings fields");
    }

    [Test]
    public async Task a_multi_value_input_repeats_its_key_once_per_value_and_binds_every_value()
    {
        await Open("/Requests/SupplyOrder");
        await Tick("East");
        await Tick("North");

        await Send();

        await Expect(Received).ToHaveTextAsync("lines nothing x 0 and nothing x 0; city none; wings East and North from 2 Wings fields");
    }

    [Test]
    public async Task members_set_below_a_list_item_and_a_nested_member_bind_by_their_binding_names()
    {
        await Open("/Requests/SupplyOrder");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Reorder the usual gloves", Exact = true }).ClickAsync();

        await Expect(Received).ToHaveTextAsync("lines GLV-1 x 2; city Albany; wings none from 0 Wings fields");
    }

    [Test]
    public async Task a_multi_value_input_holding_no_value_sends_no_key()
    {
        await Open("/Requests/SupplyOrder");
        await Tick("East");
        await Page.GetByLabel("East", new() { Exact = true }).UncheckAsync();

        await Send();

        await Expect(Received).ToHaveTextAsync("lines nothing x 0 and nothing x 0; city none; wings none from 0 Wings fields");
    }
}
