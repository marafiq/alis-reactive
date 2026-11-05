namespace Alis.Reactive.Framework.PlaywrightTests.Subscriptions;

[TestFixture]
public sealed class WhenAStreamHoldsAnObjectWithEvents : AppPageTest
{
    [Test]
    public async Task a_handler_of_an_event_of_an_object_a_stream_holds_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Subscriptions/HeldObjectEvent");

        await Expect(Page.GetByText("'cut' is an event of 'line', an object the stream or hub holds.").First).ToBeVisibleAsync();
    }
}
