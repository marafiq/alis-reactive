namespace Alis.Reactive.Framework.PlaywrightTests.Triggers;

/// <summary>
/// An enum member of an app event's payload is its member's name (grammar 1.4), as MVC binds it and a condition
/// compares it, however the page raises the event: a payload written in the view, or one composed member by
/// member. The listener on the call alarms board shows the urgency it read, and sends the nurse only for an
/// urgent alarm. (Markup that raises an event carrying an enum: Subscriptions, WhenMarkupRaisesAnEvent.)
/// </summary>
[TestFixture]
public sealed class WhenAnEventCarriesAnEnum : AppPageTest
{
    private Task Raise(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private ILocator Line(string id) => Page.Locator($"#{id}");

    [Test]
    public async Task an_enum_member_of_a_payload_written_in_the_view_reaches_the_listener_by_its_name()
    {
        await Open("/Triggers/Alarms");

        await Raise("Raise the fall alarm");

        await Expect(Line("alarm-urgency")).ToHaveTextAsync("Urgent");
    }

    [Test]
    public async Task an_enum_member_of_a_payload_written_in_the_view_is_compared_by_its_name()
    {
        await Open("/Triggers/Alarms");

        await Raise("Raise the fall alarm");

        await Expect(Line("alarm-response")).ToHaveTextAsync("send the nurse now");
    }

    [Test]
    public async Task an_enum_member_of_a_payload_written_in_the_view_that_names_another_member_is_not_equal()
    {
        await Open("/Triggers/Alarms");

        await Raise("Raise the dripping tap alarm");

        await Expect(Line("alarm-response")).ToHaveTextAsync("note it for the round");
    }

    [Test]
    public async Task an_enum_member_of_a_payload_composed_member_by_member_is_compared_by_its_name()
    {
        await Open("/Triggers/Alarms");

        await Raise("Raise the call alarm");

        await Expect(Line("alarm-response")).ToHaveTextAsync("send the nurse now");
    }
}
