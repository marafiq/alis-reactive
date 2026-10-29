namespace Alis.Reactive.Framework.PlaywrightTests.Subscriptions;

/// <summary>
/// A stream or a hub raises its own events only: a handler of an event declared on an object the stream
/// holds is refused when the page renders, with the message the developer reads. A stream's own events are
/// proven by Realtime/WhenTheServerPushesToAPage.
/// </summary>
[TestFixture]
public sealed class WhenAStreamHoldsAnObjectWithEvents : AppPageTest
{
    [Test]
    public async Task a_handler_of_an_event_of_an_object_a_stream_holds_is_refused()
    {
        // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
        await Page.GotoAsync(BaseUrl + "/Subscriptions/HeldObjectEvent");

        await Expect(Page.GetByText("'cut' is an event of 'line', an object the stream or hub holds.").First).ToBeVisibleAsync();
    }
}
