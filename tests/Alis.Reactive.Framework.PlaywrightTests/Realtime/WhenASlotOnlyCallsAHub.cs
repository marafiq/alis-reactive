namespace Alis.Reactive.Framework.PlaywrightTests.Realtime;

/// <summary>
/// A call point loaded into the ward's slot calls the ward's line (a hub) and listens to none of it; the page
/// neither listens to the line nor calls it. The server reports, live, how many of the page's line connections it
/// holds open. A plan that names a hub holds its connection for as long as its owner lives, so the connection
/// the call point's call went over closes when the call point is unloaded.
/// </summary>
[TestFixture]
public sealed class WhenASlotOnlyCallsAHub : AppPageTest
{
    private ILocator Ward(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

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
}
