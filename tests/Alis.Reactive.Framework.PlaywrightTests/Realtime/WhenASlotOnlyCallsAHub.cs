namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

[TestFixture]
public sealed class WhenASlotOnlyCallsAHub : AppPageTest
{
    private ILocator Ward(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private ILocator CallPoint => Page.Locator("#call-point-slot");

    [Test]
    public async Task a_hub_a_slot_only_calls_closes_when_the_slot_is_unloaded()
    {
        await Open("/Realtime/Ward");
        await Expect(Ward("hub-lines")).ToHaveTextAsync("0");
        await Button("Open the call point").ClickAsync();
        await Button("Call the nurse").ClickAsync();
        await Expect(Ward("call-answer")).ToHaveTextAsync("a nurse is on the way");
        await Expect(Ward("hub-lines")).ToHaveTextAsync("1");

        await Button("Close the call point").ClickAsync();

        await Expect(Ward("hub-lines")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_start_waiting_to_try_a_hub_again_tries_no_more_once_its_slot_is_unloaded()
    {
        await Open("/Realtime/Ward?refusals=1");
        await Expect(Ward("hub-lines")).ToHaveTextAsync("0");
        await Page.Clock.PauseAtAsync(DateTime.UtcNow.AddSeconds(1));
        await Button("Open the call point").ClickAsync();
        await Expect(CallPoint.GetByRole(AriaRole.Heading, new() { Name = "Call point, bed 4" })).ToBeVisibleAsync();
        await Button("Close the call point").ClickAsync();
        await Expect(CallPoint).ToBeEmptyAsync();

        await Page.Clock.RunForAsync(1_000);
        await Button("Open the call point").ClickAsync();
        await Button("Call the nurse").ClickAsync();
        await Expect(Ward("call-answer")).ToHaveTextAsync("a nurse is on the way");

        await Expect(Ward("hub-lines")).ToHaveTextAsync("1");
    }
}
