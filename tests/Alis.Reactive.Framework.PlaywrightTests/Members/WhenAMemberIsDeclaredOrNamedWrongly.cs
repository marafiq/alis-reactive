namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// A member a declaration declares wrongly, or a field a page names wrongly, refused when the page renders with
/// the message the developer reads: an event on an object a method returns, an object declared for a member the
/// declaration does not have, a method declared for no member, and a field that selects no member of the row,
/// a call on a member, or a list position before the first.
/// </summary>
[TestFixture]
public sealed class WhenAMemberIsDeclaredOrNamedWrongly : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string mistake) => await Page.GotoAsync($"{BaseUrl}/Members/Refused?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task an_event_declared_on_an_object_a_method_returns_is_refused()
    {
        await Render("DeclareAnEventOnAReturnedObject");

        await Expect(Message(
            "'RingingRow.Rung' declares an event on an object a method returns. Events belong to the component and to the objects it holds (Object<T>() on a property).")).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_object_declared_for_a_member_the_declaration_lacks_is_refused()
    {
        await Render("DeclareAnObjectForAMemberTheDeclarationLacks");

        await Expect(Message(
            "'MisnamedHeaderSlate' has no member 'Heading'. Declare an object as a C# property or method, for example public Settings Settings => Object<Settings>(); or public Reading Reading() => Object<Reading>();")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_declared_for_no_member_is_refused()
    {
        await Render("DeclareAMethodForNoMember");

        await Expect(Message("Declare a member as a C# property or method, for example public void Show() => Method();")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_field_that_selects_no_member_of_the_row_is_refused()
    {
        await Render("SelectNoMemberOfTheRow");

        await Expect(Message("Select a member of the row, for example (Resident r) => r.Name.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_field_that_selects_a_call_on_a_member_is_refused()
    {
        await Render("SelectACallOnAMember");

        await Expect(Message(
            "Select a member by its path, like m => m.Address.City or m => m.Items[0].Sku; 'card.Name.ToUpper()' is not one.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_field_that_selects_a_position_before_the_first_is_refused()
    {
        await Render("SelectAPositionBeforeTheFirst");

        await Expect(Message("Path index must not be negative.")).ToBeVisibleAsync();
    }
}
