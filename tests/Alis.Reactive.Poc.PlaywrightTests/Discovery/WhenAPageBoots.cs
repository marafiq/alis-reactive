namespace Alis.Reactive.Poc.PlaywrightTests.Discovery;

[TestFixture]
public sealed class WhenAPageBoots : AppPageTest
{
    [Test]
    public async Task a_handler_runs_on_a_component_whose_script_the_page_lists_after_the_runtime()
    {
        await Open("/Discovery/RuntimeFirst");

        await Page.GetByRole(AriaRole.Button, new() { Name = "East" }).ClickAsync();

        await Expect(Page.Locator("#chosen-ward")).ToHaveTextAsync("East");
    }
}
