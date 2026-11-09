namespace Alis.Reactive.Poc.PlaywrightTests.Slots;

[TestFixture]
public sealed class WhenAListenIsRefused : AppPageTest
{
    [Test]
    public async Task a_write_to_the_payload_around_a_listen_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/ListenWritesThePayload");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A write to the payload of the event around a Listen reaches no vendor: the vendor read it when its listener returned, before the listened object raised. Write it in the event's own pipeline, outside the Listen.");
    }

    [Test]
    public async Task a_listen_on_an_object_that_may_hold_nothing_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/ListenOnNothing");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: Listen reaches an object its declaration says may hold nothing (optional or nullable). A listen needs its object when the step runs: listen to an object declared present.");
    }

    [Test]
    public async Task a_returned_call_kept_out_of_its_branch_and_listened_to_after_it_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/CallKeptOutOfItsBranch");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it.");
    }

    [Test]
    public async Task a_listens_payload_kept_out_of_the_listen_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Cards/AnswerKeptOutOfItsListen");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A Listen's payload is read outside the Listen that receives it. Read the payload in the Listen's pipeline: pipeline.Listen(source, s => s.Raised, (raised, pipeline) => ..).");
    }
}
