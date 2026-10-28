namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// An array operation's result used where a value is read beyond an element's text: a transformed array
/// passed to a component's method, a count written into a component's property, and a transformed array
/// sent as a request member the endpoint binds. The residents endpoint answers Cyd Marsh and Ben Hart
/// present, Ada Hartley and Dot Ashton away; the slate holds the rows Ada, Ben and Cyd.
/// </summary>
[TestFixture]
public sealed class WhenAPageUsesAnArraysResult : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_transformed_array_is_passed_to_a_method_as_the_array_it_declares()
    {
        await Open("/Arrays/Results");

        await Button("Show the present residents on the roster").ClickAsync();

        await Expect(Page.Locator("#roster").GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Ben Hart", "Cyd Marsh"]);
    }

    [Test]
    public async Task a_count_is_written_into_a_components_property()
    {
        await Open("/Arrays/Results");

        await Button("Set the meter to the number present").ClickAsync();

        await Expect(Page.Locator("#meter").GetByText("Level 2 of 5")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_transformed_array_is_sent_as_the_request_member_the_endpoint_binds()
    {
        await Open("/Arrays/Results");

        await Button("Send a notice about the rows other than Ben").ClickAsync();

        await Expect(Page.Locator("#noticed")).ToHaveTextAsync("2 named: Ada and Cyd");
    }
}
