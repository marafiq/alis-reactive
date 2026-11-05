namespace Alis.Reactive.Framework.PlaywrightTests.Inputs;

[TestFixture]
public sealed class WhenAnInputsValueIsAFieldOfTheDataItHolds : AppPageTest
{
    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task the_field_of_each_item_is_sent()
    {
        await Open("/Inputs/Tags");
        await Click("Pick the east wing tag");
        await Click("Pick the north wing tag");

        await Click("Send the tags");

        await Expect(Page.Locator("#tags-bound")).ToHaveTextAsync("east · north");
    }

    [Test]
    public async Task an_item_that_lacks_the_field_is_sent_as_nothing_in_its_place()
    {
        await Open("/Inputs/Tags");
        await Click("Pick the east wing tag");
        await Click("Pick a tag with no code");
        await Expect(Page.GetByText("Picked: East wing · Visitor", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Send the tags");

        await Expect(Page.Locator("#tags-bound")).ToHaveTextAsync("east · (no code)");
    }
}
