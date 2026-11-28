namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenAStepWritesOverASlot : AppPageTest
{
    [Test]
    public async Task a_cards_step_that_writes_over_the_slot_it_was_loaded_into_stops()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Write over this card");

        await Expect(Page.Locator("#write-asked")).ToHaveTextAsync("Asked to write over the card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("not done");
    }

    [Test]
    public async Task a_step_that_writes_over_an_element_holding_a_slot_stops()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Write over the cards");

        await Expect(Page.Locator("#write-asked")).ToHaveTextAsync("Asked to write over the cards");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("not done");
    }

    [Test]
    public async Task a_slot_an_open_cards_plan_unloads_stops_a_step_that_writes_over_it()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Write the ward notes");

        await Expect(Page.Locator("#write-asked")).ToHaveTextAsync("Asked to write the ward notes");
        await Expect(Page.Locator("#ward-notes")).ToHaveTextAsync("No ward notes");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("not done");
    }

    [Test]
    public async Task a_slot_only_a_closed_cards_plan_unloaded_takes_a_step_that_writes_over_it()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Write the ward notes");

        await Expect(Page.Locator("#ward-notes")).ToHaveTextAsync("Written by the ward");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("Wrote the ward notes");
    }

    [Test]
    public async Task ward_notes_a_closed_card_loaded_stop_a_step_that_writes_over_them()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Load the ward notes");
        await Expect(Page.Locator("#ward-notes")).ToHaveTextAsync("All quiet on the ward");
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Write the ward notes");

        await Expect(Page.Locator("#write-asked")).ToHaveTextAsync("Asked to write the ward notes");
        await Expect(Page.Locator("#ward-notes")).ToHaveTextAsync("All quiet on the ward");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("not done");
    }

    [Test]
    public async Task ward_notes_a_card_cleared_before_it_closed_take_a_step_that_writes_over_them()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Load the ward notes");
        await Expect(Page.Locator("#ward-notes")).ToHaveTextAsync("All quiet on the ward");
        await Click("Clear the ward notes");
        await Expect(Page.Locator("#ward-notes")).ToBeEmptyAsync();
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Write the ward notes");

        await Expect(Page.Locator("#ward-notes")).ToHaveTextAsync("Written by the ward");
        await Expect(Page.Locator("#write-done")).ToHaveTextAsync("Wrote the ward notes");
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
