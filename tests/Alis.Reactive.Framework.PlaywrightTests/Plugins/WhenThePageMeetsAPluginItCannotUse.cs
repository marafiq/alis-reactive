namespace Alis.Reactive.Framework.PlaywrightTests.Plugins;

[TestFixture]
public sealed class WhenThePageMeetsAPluginItCannotUse : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_step_reaching_a_plugin_no_script_registered_fails_only_its_own_pipeline()
    {
        await Open("/Plugins/Ledger");

        await Button("Add to the ledger").ClickAsync();
        await Expect(Page.Locator("#add-tried")).ToHaveTextAsync("tried");
        await Expect(Page.Locator("#added")).ToBeEmptyAsync();

        await Button("Say hello").ClickAsync();
        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await Expect(Page.Locator("#added")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_plugin_name_a_second_bundle_registers_again_stops_the_page_from_starting()
    {
        await Page.GotoAsync(BaseUrl + "/Plugins/Refused?bundle=SecondTally");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "A plugin the runtime refuses" })).ToBeVisibleAsync();
        await Expect(Page.Locator("#started")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_plugin_a_bundle_registers_as_something_that_holds_no_members_stops_the_page_from_starting()
    {
        await Page.GotoAsync(BaseUrl + "/Plugins/Refused?bundle=MemberlessLantern");

        await Expect(Page.GetByRole(AriaRole.Heading, new() { Name = "A plugin the runtime refuses" })).ToBeVisibleAsync();
        await Expect(Page.Locator("#started")).ToBeEmptyAsync();
    }
}
