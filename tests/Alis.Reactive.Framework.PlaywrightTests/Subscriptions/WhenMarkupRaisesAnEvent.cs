namespace Alis.Reactive.Framework.PlaywrightTests.Subscriptions;

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
    public async Task a_member_named_its_own_way_reaches_the_listener_by_that_name()
    {
        await Open("/Subscriptions/Gate");

        await Button("A delivery at the gate").ClickAsync();

        await Expect(Page.Locator("#called-by")).ToHaveTextAsync("the porter");
    }

    [Test]
    public async Task a_click_inside_markup_that_raises_an_event_raises_that_markups_event()
    {
        await Open("/Subscriptions/Gate");

        await Button("Details").ClickAsync();

        await Expect(Announcement).ToHaveTextAsync("a visitor");
    }

    [Test]
    public async Task markup_raising_a_payload_that_holds_a_record_is_refused_naming_the_fix()
    {
        await Page.GotoAsync(BaseUrl + "/Subscriptions/EscortedVisit");

        await Expect(Page.GetByText(
            "'EscortedVisit.Visitor' is a GateVisitor, and markup carries each member of the payload it raises as the " +
            "text of one attribute, so a member it carries is text, a number, a bool, a date, a Guid or an enum. Raise " +
            "a payload that holds a record or a list from a pipeline, with p.Dispatch.").First)
            .ToBeVisibleAsync();
    }

    [Test]
    public async Task markup_raising_a_member_named_so_no_attribute_carries_it_back_is_refused_naming_the_fix()
    {
        await Page.GotoAsync(BaseUrl + "/Subscriptions/SignedVisit");

        await Expect(Page.GetByText(
            "'SignedVisit.Signature' is named 'Signature', and markup carries each member of the payload it raises as an " +
            "attribute whose name the browser lowercases, so a name it carries is camel case: a lowercase first letter, " +
            "then letters, digits or underscores. Rename it, or raise the payload from a pipeline, with p.Dispatch.").First)
            .ToBeVisibleAsync();
    }
}
