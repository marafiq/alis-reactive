namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenAListenMeetsAnAwait : AppPageTest
{
    [Test]
    public async Task a_payload_written_after_a_listen_whose_pipeline_awaits_reaches_the_vendor()
    {
        await Open("/Cards/Handled");

        await Click("Page room 12");
        await Expect(Page.Locator("#handled")).ToHaveTextAsync("12");

        await Click("Answer room 12");
        await Expect(Page.Locator("#calls li")).ToHaveTextAsync(["The desk acknowledged the call"]);
    }

    [Test]
    public async Task a_listen_added_after_an_awaited_step_writes_its_own_payload_in_the_vendors_turn()
    {
        await Open("/Cards/Handled");
        await Click("Page room 12");
        await Expect(Page.Locator("#handled")).ToHaveTextAsync("12");

        await Click("Answer room 12");

        await Expect(Page.Locator("#logged")).ToHaveTextAsync("Nurse Kim");
    }

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();
}
