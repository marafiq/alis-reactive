namespace Alis.Reactive.PlaywrightTests.Plugins.Pager;

/// <summary>
/// As the developer of the wing pager page, I want a view that registers the pager twice refused as it
/// renders, by the plugin's name, so that the mistake never reaches a nurse's browser.
/// </summary>
[TestFixture]
public class WhenAViewRegistersThePagerTwice : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Plugins/Pager/Duplicate";

    [Test]
    public async Task the_page_does_not_render_and_names_the_plugin_registered_twice()
    {
        var response = await Page.GotoAsync($"{BaseUrl}{Path}", new() { WaitUntil = WaitUntilState.DOMContentLoaded });

        Assert.That(response, Is.Not.Null);
        Assert.That(response!.Status, Is.EqualTo(500), "A view that registers a plugin twice does not render.");
        await Expect(Page.Locator("body")).ToContainTextAsync(
            "Plugin 'pager' is registered twice on the plan of /Areas/Sandbox/Views/Plugins/Pager/Duplicate.cshtml: register it once, and use the plugin RegisterPlugin returned wherever the view needs it.");
    }
}
