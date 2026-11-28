namespace Alis.Reactive.Poc.PlaywrightTests.Events;

[TestFixture]
public sealed class WhenAWardSignals : AppPageTest
{
    [Test]
    public async Task a_bell_whose_event_is_a_callback_property_runs_its_handler_with_the_ring()
    {
        await Open("/Signals/Ward");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Ring the bell" }).ClickAsync();

        await Expect(Page.Locator("#room")).ToHaveTextAsync("12");
    }

    [Test]
    public async Task a_feed_whose_subscribe_returns_its_unsubscriber_runs_its_handler_with_the_update()
    {
        await Open("/Signals/Ward");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Post an update" }).ClickAsync();

        await Expect(Page.Locator("#update")).ToHaveTextAsync("Bed 3 is free");
    }

    [Test]
    public async Task a_custom_events_detail_is_a_member_of_its_payload()
    {
        await Open("/Signals/Ward");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assign bed 7" }).ClickAsync();

        await Expect(Page.Locator("#bed")).ToHaveTextAsync("7");
    }

    [Test]
    public async Task a_custom_event_with_no_detail_reads_nothing_past_it_on_every_dispatch()
    {
        await Open("/Signals/Ward");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assign no bed" }).ClickAsync();
        await Expect(Page.Locator("#assignment")).ToHaveTextAsync("Assigned");
        await Expect(Page.Locator("#bed")).ToHaveTextAsync("");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Assign bed 7" }).ClickAsync();
        await Expect(Page.Locator("#bed")).ToHaveTextAsync("7");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assign no bed" }).ClickAsync();

        await Expect(Page.Locator("#bed")).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_second_handler_on_a_callback_property_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Signals/RungTwice");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: 'onring' is a callback property, which holds one listener, so a second handler would replace the first. Write both pipelines in one handler.");
    }
}
