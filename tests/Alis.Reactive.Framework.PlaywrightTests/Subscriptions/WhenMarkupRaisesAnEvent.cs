namespace Alis.Reactive.Framework.PlaywrightTests.Subscriptions;

/// <summary>
/// Markup that raises an app's event when it is clicked (grammar.md 6.1, D4), written with Html.Raises: an event
/// that carries nothing is raised; without its payload the listener reads each member as missing; each member the
/// markup carries reaches the listener as its type, a text as its text, a number as a number and an enum as its
/// member; and a click inside the markup is the markup's. A whole payload the markup carries reaching the listener
/// is proven by Triggers/WhenAPageStartsItsPipelines.
/// </summary>
[TestFixture]
public sealed class WhenMarkupRaisesAnEvent : AppPageTest
{
    private ILocator Announcement => Page.Locator("#announcement");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task markup_raising_an_event_that_carries_nothing_raises_it_when_clicked()
    {
        await Open("/Subscriptions/Gate");

        await Button("Open the gate").ClickAsync();

        await Expect(Page.Locator("#gate")).ToHaveTextAsync("open");
    }

    [Test]
    public async Task markup_without_a_payload_hands_the_listener_a_payload_whose_members_are_missing()
    {
        await Open("/Subscriptions/Gate");

        await Button("Ring the gate bell").ClickAsync();

        await Expect(Announcement).ToHaveTextAsync("an announcement of no type");
    }

    [Test]
    public async Task a_text_member_the_markup_carries_reaches_the_listener_as_its_text()
    {
        await Open("/Subscriptions/Gate");

        await Button("A delivery at the gate").ClickAsync();

        await Expect(Announcement).ToHaveTextAsync("a delivery");
    }

    // Twelve is more than nine as a number; as text it is not, and text is ordered against no number.
    [Test]
    public async Task a_number_member_the_markup_carries_reaches_the_listener_as_a_number()
    {
        await Open("/Subscriptions/Gate");

        await Button("A coach party at the gate").ClickAsync();

        await Expect(Page.Locator("#party")).ToHaveTextAsync("a large party");
    }

    [Test]
    public async Task an_enum_member_the_markup_carries_reaches_the_listener_as_its_member()
    {
        await Open("/Subscriptions/Gate");

        await Button("A coach party at the gate").ClickAsync();

        await Expect(Page.Locator("#urgency")).ToHaveTextAsync("urgent");
    }

    [Test]
    public async Task a_click_inside_markup_that_raises_an_event_raises_that_markups_event()
    {
        await Open("/Subscriptions/Gate");

        await Button("Details").ClickAsync();

        await Expect(Announcement).ToHaveTextAsync("a visitor");
    }
}
