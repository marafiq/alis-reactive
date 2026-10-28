namespace Alis.Reactive.Framework.PlaywrightTests.Vendors;

/// <summary>
/// A component whose element is on the page but whose object its script has not built: the handler of its
/// event fails to listen and stops no other handler from listening, an input of one cannot announce its commits
/// and stops no handler from listening (M4), and a step that reaches it fails only its own pipeline (the step
/// before it ran, the step after it did not, and the next gesture works).
/// </summary>
[TestFixture]
public sealed class WhenAComponentIsNotBuiltYet : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_handler_of_a_component_not_built_yet_stops_no_other_handler_from_listening()
    {
        await Open("/Vendors/Unbuilt");

        await Button("Say hello").ClickAsync();

        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
    }

    [Test]
    public async Task an_input_whose_component_is_not_built_yet_stops_no_handler_from_listening()
    {
        await Open("/Vendors/UnbuiltInput");

        await Button("Say hello").ClickAsync();

        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
    }

    [Test]
    public async Task a_step_reaching_a_component_not_built_yet_fails_only_its_own_pipeline()
    {
        await Open("/Vendors/Unbuilt");

        await Button("Retitle the slate").ClickAsync();
        await Expect(Page.Locator("#retitle-tried")).ToHaveTextAsync("tried");
        await Expect(Page.Locator("#retitled")).ToBeEmptyAsync();

        await Button("Say hello").ClickAsync();
        await Expect(Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await Expect(Page.Locator("#retitled")).ToBeEmptyAsync();
    }
}
