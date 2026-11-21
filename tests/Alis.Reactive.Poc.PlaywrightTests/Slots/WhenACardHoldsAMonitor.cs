namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenACardHoldsAMonitor : AppPageTest
{
    [Test]
    public async Task a_closed_card_unmounts_its_monitor()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator(".monitor-badge")).ToHaveTextAsync(["Ada Byron's monitor is on"]);

        await Click("Close the card");

        await Expect(Page.Locator(".monitor-badge")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task a_card_loaded_over_another_unmounts_the_first_cards_monitor()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator(".monitor-badge")).ToHaveTextAsync(["Ada Byron's monitor is on"]);

        await Click("Open Ben's card");

        await Expect(Page.Locator(".monitor-badge")).ToHaveTextAsync(["Ben Okafor's monitor is on"]);
    }

    [Test]
    public async Task a_closed_card_whose_monitor_left_at_its_unmount_empties()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ada Byron");

        await Click("Close the card");

        await Expect(Page.Locator("#card")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_card_whose_monitor_was_switched_off_closes()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Click("Switch off Ada Byron's monitor");
        await Expect(Page.Locator(".monitor-badge")).ToHaveCountAsync(0);

        await Click("Close the card");

        await Expect(Page.Locator("#card")).ToBeEmptyAsync();
    }

    [Test]
    public async Task a_card_whose_monitor_was_restarted_closes()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Click("Restart Ada Byron's monitor");
        await Expect(Page.Locator(".monitor-badge")).ToHaveTextAsync(["Ada Byron's monitor is on again"]);

        await Click("Close the card");

        await Expect(Page.Locator("#card")).ToBeEmptyAsync();
    }

    [Test]
    public async Task the_next_card_hears_its_monitor_after_one_was_switched_off()
    {
        await Open("/Cards/Ward");
        await Click("Open Ada's card");
        await Click("Switch off Ada Byron's monitor");
        await Expect(Page.Locator(".monitor-badge")).ToHaveCountAsync(0);
        await Click("Open Ben's card");
        await Expect(Page.Locator("#card h2")).ToHaveTextAsync("Ben Okafor");

        await Click("Raise Ben Okafor's alarm");

        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["Ben Okafor's monitor alarmed"]);
    }

    [Test]
    public async Task a_slot_no_plan_loaded_unmounts_what_the_server_rendered_in_it()
    {
        await Open("/Cards/Bedside");
        await Expect(Page.Locator(".monitor-badge")).ToHaveTextAsync(["The spare bed's monitor is on"]);

        await Click("Close the card");

        await Expect(Page.Locator(".monitor-badge")).ToHaveCountAsync(0);
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
