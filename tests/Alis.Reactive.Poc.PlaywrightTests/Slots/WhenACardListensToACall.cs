namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenACardListensToACall : AppPageTest
{
    private const string WardHeard = "The ward heard a call answered";
    private const string AdaHeard = "Ada Byron heard a call answered";
    private const string BenHeard = "Ben Okafor heard a call answered";

    [Test]
    public async Task a_call_handed_over_twice_is_heard_once_per_answer()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Page room 12");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12");
        await Click("Page room 12 again");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12 again");

        await Click("Answer room 12");
        await Expect(Page.Locator("#answered")).ToHaveTextAsync("Answered room 12");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard]);

        await Click("Answer room 12 again");
        await Expect(Page.Locator("#answered")).ToHaveTextAsync("Answered room 12 again");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard, WardHeard, AdaHeard]);
    }

    [Test]
    public async Task another_call_is_heard_on_its_own_answer()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Page room 12");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12");
        await Click("Page room 14");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 14");

        await Click("Answer room 14");
        await Expect(Page.Locator("#answered")).ToHaveTextAsync("Answered room 14");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard]);

        await Click("Answer room 12");
        await Expect(Page.Locator("#answered")).ToHaveTextAsync("Answered room 12");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard, WardHeard, AdaHeard]);
    }

    [Test]
    public async Task a_closed_card_hears_no_more_answers()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Page room 12");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12");
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Answer room 12");

        await Expect(Page.Locator("#answered")).ToHaveTextAsync("Answered room 12");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard]);
    }

    [Test]
    public async Task a_card_loaded_over_another_hears_the_call_for_itself_alone()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Page room 12");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12");
        await Click("Open Ben's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ben Okafor");
        await Click("Page room 12 again");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12 again");

        await Click("Answer room 12");

        await Expect(Page.Locator("#answered")).ToHaveTextAsync("Answered room 12");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, BenHeard]);
    }

    [Test]
    public async Task a_listen_reads_its_own_payload_and_the_payload_that_handed_its_object_over()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Page room 12");
        await Expect(Page.Locator("#paged")).ToHaveTextAsync("Paged room 12");

        await Click("Answer room 12");

        await Expect(Page.Locator("#card-call-room")).ToHaveTextAsync("12");
        await Expect(Page.Locator("#card-call-by")).ToHaveTextAsync("Nurse Kim");
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
