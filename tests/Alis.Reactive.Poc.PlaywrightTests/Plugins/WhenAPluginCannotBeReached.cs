namespace Alis.Reactive.Poc.PlaywrightTests.Plugins;

[TestFixture]
public sealed class WhenAPluginCannotBeReached : AppPageTest
{
    [Test]
    public async Task a_step_reaching_a_plugin_no_script_provides_stops_only_its_own_pipeline()
    {
        await Open("/Shifts/VisitorBook");

        await Button("Sign the book").ClickAsync();
        await Expect(Page.Locator("#sign-tried")).ToHaveTextAsync("tried");
        await Button("Greet the visitor").ClickAsync();

        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await Expect(Page.Locator("#signed")).ToHaveTextAsync("nobody");
    }

    [Test]
    public async Task a_plugin_a_script_provides_after_the_page_started_is_found_by_the_next_step()
    {
        await Open("/Shifts/VisitorBook");

        await Button("Open the visitor book").ClickAsync();
        await Button("Sign the book").ClickAsync();

        await Expect(Page.Locator("#signed")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task a_plugin_a_script_provides_after_the_page_started_raises_events_no_handler_hears()
    {
        await Open("/Shifts/VisitorBook");

        await Button("Open the visitor book").ClickAsync();
        await Button("Sign the book").ClickAsync();

        await Expect(Page.Locator("#signed")).ToHaveTextAsync("Ada Byron");
        await Expect(Page.Locator("#front-desk")).ToHaveTextAsync("no visitor");
    }

    [Test]
    public async Task a_plugin_name_two_scripts_provide_stops_each_step_that_reaches_it()
    {
        await Open("/Shifts/LogTwice");

        await Button("Read the last note").ClickAsync();
        await Expect(Page.Locator("#read-tried")).ToHaveTextAsync("tried");
        await Button("Greet the shift").ClickAsync();

        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await Expect(Page.Locator("#last-note")).ToHaveTextAsync("none read");
    }

    [Test]
    public async Task a_plugin_provided_as_a_number_stops_each_step_that_reads_a_member_a_number_lacks()
    {
        await Open("/Shifts/WardLantern");

        await Button("Read the lantern's colour").ClickAsync();
        await Expect(Page.Locator("#read-tried")).ToHaveTextAsync("tried");
        await Button("Greet the ward").ClickAsync();

        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await Expect(Page.Locator("#colour")).ToHaveTextAsync("none read");
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });
}
