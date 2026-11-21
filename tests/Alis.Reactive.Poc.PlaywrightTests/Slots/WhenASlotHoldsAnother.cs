namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenASlotHoldsAnother : AppPageTest
{
    private const string WardHeard = "The ward heard the note";

    [Test]
    public async Task a_closed_card_takes_its_open_notes_with_it()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Open the notes");
        await Expect(Page.Locator("#card-notes p")).ToHaveTextAsync("Latest notes for Ada Byron");
        await Click("Note a fall");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, "Ada Byron heard the note", "Ada Byron's notes heard the note"]);
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, "Ada Byron heard the note", "Ada Byron's notes heard the note", WardHeard]);
    }

    [Test]
    public async Task a_card_loaded_over_one_with_open_notes_takes_the_notes_with_it()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Open the notes");
        await Expect(Page.Locator("#card-notes p")).ToHaveTextAsync("Latest notes for Ada Byron");
        await Click("Open Ben's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ben Okafor");

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, "Ben Okafor heard the note"]);
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
