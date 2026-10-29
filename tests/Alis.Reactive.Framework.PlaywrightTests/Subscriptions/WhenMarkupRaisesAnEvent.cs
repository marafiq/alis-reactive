namespace Alis.Reactive.Framework.PlaywrightTests.Subscriptions;

/// <summary>
/// Markup that raises an app's event when it is clicked (grammar.md 6.1, D4): without a detail the
/// listener's payload is an empty record, so each member it reads is missing; and an element whose
/// data-alis-raise names no event is not a source, so a click on it is the element's around it. A detail
/// the markup carries reaching the listener is proven by Triggers/WhenAPageStartsItsPipelines.
/// </summary>
[TestFixture]
public sealed class WhenMarkupRaisesAnEvent : AppPageTest
{
    private ILocator Announcement => Page.Locator("#announcement");

    [Test]
    public async Task markup_without_a_detail_hands_the_listener_a_payload_whose_members_are_missing()
    {
        await Open("/Subscriptions/Gate");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Ring the gate bell", Exact = true }).ClickAsync();

        await Expect(Announcement).ToHaveTextAsync("an announcement of no type");
    }

    [Test]
    public async Task a_click_on_markup_that_names_no_event_raises_the_event_of_the_markup_around_it()
    {
        await Open("/Subscriptions/Gate");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Details", Exact = true }).ClickAsync();

        await Expect(Announcement).ToHaveTextAsync("a visitor");
    }
}
