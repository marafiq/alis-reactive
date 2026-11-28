namespace Alis.Reactive.Poc.PlaywrightTests.Plugins;

[TestFixture]
public sealed class WhenAPluginIsRefused : AppPageTest
{
    [Test]
    public async Task a_plugin_named_with_a_dot_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Shifts/DottedPlugin");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: Plugin name 'alis.log' must start with a letter and contain only ASCII letters, digits, underscore, or hyphen. A name with a dot is the framework's own.");
    }
}
