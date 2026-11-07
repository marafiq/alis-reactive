namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenTheServerRendersACard : AppPageTest
{
    private const string WardHeard = "The ward heard the note";

    [Test]
    public async Task a_card_the_server_rendered_in_the_slot_listens_from_boot()
    {
        await Open("/Cards/Admitted");

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron's notes heard the note", "Ada Byron heard the note", WardHeard]);
    }

    [Test]
    public async Task a_card_the_server_rendered_in_the_slot_leaves_at_the_slots_first_load()
    {
        await Open("/Cards/Admitted");
        await Click("Open Ben's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ben Okafor");

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, "Ben Okafor heard the note"]);
    }

    [Test]
    public async Task a_card_the_server_rendered_in_the_slot_leaves_with_its_notes_when_it_closes()
    {
        await Open("/Cards/Admitted");
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard]);
    }

    [Test]
    public async Task notes_the_server_rendered_in_a_card_leave_when_the_notes_load_again()
    {
        await Open("/Cards/Admitted");
        await Click("Open the notes");
        await Expect(Page.Locator("#card-notes p")).ToHaveTextAsync("Latest notes for Ada Byron");

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron heard the note", WardHeard, "Ada Byron's notes heard the note"]);
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
