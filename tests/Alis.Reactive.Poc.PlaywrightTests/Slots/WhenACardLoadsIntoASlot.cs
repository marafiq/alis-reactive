using System.Text.RegularExpressions;

namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenACardLoadsIntoASlot : AppPageTest
{
    private const string WardHeard = "The ward heard the note";
    private const string AdaHeard = "Ada Byron heard the note";

    [Test]
    public async Task a_card_loaded_into_the_slot_shows_what_the_server_rendered()
    {
        await Open("/Cards/Ward");

        await Click("Open Ada's card");

        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task a_card_loaded_three_times_hears_a_note_once()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Open Ben's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ben Okafor");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard]);
    }

    [Test]
    public async Task a_closed_card_hears_no_more_notes()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Note a fall");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard]);
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard, WardHeard]);
    }

    [Test]
    public async Task closing_a_card_leaves_the_wards_own_handler_listening()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard]);
    }

    [Test]
    public async Task a_card_the_server_does_not_find_leaves_the_slot_as_it_was()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Open the missing card");
        await Expect(Page.Locator("#asked")).ToHaveTextAsync("Asked for the missing card");

        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task a_card_the_server_does_not_find_leaves_the_loaded_card_listening()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Open the missing card");
        await Expect(Page.Locator("#asked")).ToHaveTextAsync("Asked for the missing card");

        await Click("Note a fall");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync([WardHeard, AdaHeard]);
    }

    [Test]
    public async Task a_body_that_is_not_html_leaves_the_slot_as_it_was()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Open the listed card");
        await Expect(Page.Locator("#asked")).ToHaveTextAsync("Asked for the listed card");

        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task a_card_the_network_cannot_reach_leaves_the_slot_as_it_was()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Open the unreachable card");
        await Expect(Page.Locator("#asked")).ToHaveTextAsync("Asked for the unreachable card");

        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
    }

    [Test]
    public async Task a_step_after_a_request_with_no_route_for_its_outcome_runs()
    {
        await Open("/Cards/Ward");

        await Click("Open the missing card");

        await Expect(Page.Locator("#asked")).ToHaveTextAsync("Asked for the missing card");
    }

    [Test]
    public async Task a_closed_card_hears_no_more_hours()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Strike the hour");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron heard the hour"]);
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Strike the hour again");
        await Expect(Page.Locator("#struck")).ToHaveTextAsync("Struck again");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron heard the hour"]);
    }

    [Test]
    public async Task a_card_loaded_three_times_hears_a_bed_assigned_once()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Open Ben's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ben Okafor");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Assign bed 12B");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron heard a bed assigned"]);
    }

    [Test]
    public async Task a_closed_card_hears_no_more_beds()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");
        await Click("Assign bed 12B");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron heard a bed assigned"]);
        await Click("Close the card");
        await Expect(Page.Locator("#card")).ToBeEmptyAsync();

        await Click("Assign bed 14C");
        await Expect(Page.Locator("#assigned")).ToHaveTextAsync("Assigned again");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ada Byron heard a bed assigned"]);
    }

    [Test]
    public async Task a_card_loaded_into_a_slot_asks_for_html()
    {
        await Open("/Cards/Ward");

        var asked = await Page.RunAndWaitForRequestAsync(() => Click("Open Ada's card"), new Regex("/Cards/Card"));

        Assert.That(await asked.HeaderValueAsync("Accept"), Is.EqualTo("text/html"));
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
