namespace Alis.Reactive.Framework.PlaywrightTests.Slots;

/// <summary>
/// A partial of its own model loaded into a slot and unloaded from it: a component in the loaded HTML is
/// built before its handlers listen (L1), an input in it binds under the partial's prefix (I4), its handlers
/// hear events until the slot is unloaded (E6), an unloaded slot lets go of what it held and loads fresh
/// again (L2), and a step naming an element that is not on the page fails only its own pipeline (B1).
/// </summary>
[TestFixture]
public sealed class WhenAPartialIsLoadedIntoASlot : AppPageTest
{
    private ILocator Card => Page.Locator("#card-slot");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private async Task OpenTheCard()
    {
        await Button("Open the card").ClickAsync();
        await Expect(Card.GetByRole(AriaRole.Heading, new() { Name = "Next of kin" })).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_component_in_loaded_html_is_built_before_its_handlers_listen()
    {
        await Open("/Slots/Desk");
        await OpenTheCard();

        await Card.GetByRole(AriaRole.Button, new() { Name = "Add row" }).ClickAsync();

        await Expect(Card.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Monday", "Tuesday", "Row 3"]);
        await Expect(Page.Locator("#card-status")).ToHaveTextAsync("Row 3");
    }

    [Test]
    public async Task an_input_in_a_partial_binds_under_the_member_the_partial_was_rendered_for()
    {
        await Open("/Slots/Desk");
        await OpenTheCard();

        await Page.GetByLabel("Next of kin").FillAsync("Tom");
        await Button("Save the next of kin").ClickAsync();

        await Expect(Page.Locator("#kin-bound")).ToHaveTextAsync("next of kin Tom");
    }

    [Test]
    public async Task a_partials_handler_hears_events_until_its_slot_is_unloaded()
    {
        await Open("/Slots/Desk");
        await OpenTheCard();

        await Button("Ring the bell").ClickAsync();
        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#card-heard")).ToHaveTextAsync("1");

        await Button("Close the card").ClickAsync();
        await Expect(Card.GetByRole(AriaRole.Heading)).ToHaveCountAsync(0);
        await Button("Ring the bell").ClickAsync();

        await Expect(Page.Locator("#page-heard")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#card-heard")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task an_unloaded_slot_lets_go_of_what_it_held_and_loads_fresh_again()
    {
        await Open("/Slots/Desk");
        await OpenTheCard();
        await Card.GetByRole(AriaRole.Button, new() { Name = "Add row" }).ClickAsync();
        await Expect(Card.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(3);

        await Button("Close the card").ClickAsync();
        await Expect(Card).ToBeEmptyAsync();

        await OpenTheCard();
        await Expect(Card.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(["Monday", "Tuesday"]);
    }

    [Test]
    public async Task a_step_naming_an_element_not_on_the_page_fails_only_its_own_pipeline()
    {
        await Open("/Slots/Desk");
        await OpenTheCard();
        await Button("Ring the bell").ClickAsync();
        await Button("Write into the card").ClickAsync();
        await Expect(Page.Locator("#card-status")).ToHaveTextAsync("from the page");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("1");

        await Button("Close the card").ClickAsync();
        await Expect(Card).ToBeEmptyAsync();
        await Button("Ring the bell").ClickAsync();
        await Button("Write into the card").ClickAsync();

        await Expect(Page.Locator("#write-tried")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("1");
        await Button("Read the rings").ClickAsync();
        await Expect(Page.Locator("#bell-read")).ToHaveTextAsync("2");
    }
}
