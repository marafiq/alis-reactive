namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// The gathers a page is refused for when it renders, with the message the developer reads: a named input the
/// request has no member for, at its root or in a class it holds; a named input whose value the request member
/// cannot bind, a number into text or a value that may be empty into one that may not; a named input of a partial
/// sent to a request without the partial's prefix; a named form that names no input; and a header with no value.
/// </summary>
[TestFixture]
public sealed class WhenAGatherIsWrittenWrongly : AppPageTest
{
    // A refused page never boots, so it is opened as the developer meets it: the page the server answers.
    private async Task Render(string path) => await Page.GotoAsync(BaseUrl + path);

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_named_input_the_request_has_no_member_for_is_refused()
    {
        await Render("/Requests/Refused?mistake=NameAMemberTheRequestLacks");

        await Expect(Message("Form sends the input 'Visitor', but PassRequest, the request the endpoint binds, has no member 'Visitor'.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_named_input_a_class_of_the_request_has_no_member_for_is_refused_naming_that_class()
    {
        await Render("/Requests/Refused?mistake=NameAMemberTheRequestsNestedClassLacks");

        await Expect(Message("Form sends the input 'Guide.Phone', but GuideRequest, the request the endpoint binds, has no member 'Phone' on GuideName.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_named_input_of_another_type_than_its_request_member_is_refused()
    {
        await Render("/Requests/Refused?mistake=SendANumberToText");

        await Expect(Message("Form sends the input 'Party' as Int32, but PartyAsText.Party, which the endpoint binds, is String. Give the request member the input's type.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_named_input_that_may_be_empty_is_refused_for_a_request_member_that_may_not()
    {
        await Render("/Requests/Refused?mistake=SendAMaybeNumberToANumber");

        await Expect(Message("Form sends the input 'Room' as Int32?, but RoomRequired.Room, which the endpoint binds, is Int32. Give the request member the input's type.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_named_input_in_a_partial_is_refused_for_a_request_without_the_partials_prefix()
    {
        await Render("/Requests/Escort?gather=AsARequestWithoutThePrefix");

        await Expect(Message("Form sends the input 'Escort.Name', but EscortRequest, the request the endpoint binds, has no member 'Escort'.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_named_form_that_names_no_input_is_refused()
    {
        await Render("/Requests/Refused?mistake=NameNoInput");

        await Expect(Message("Form(..) takes at least one field; Form() sends every input.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_header_given_no_value_is_refused()
    {
        await Render("/Requests/Refused?mistake=GiveAHeaderNoValue");

        await Expect(Message("Header 'X-Shift' needs a value. A value read at runtime takes the source overload.")).ToBeVisibleAsync();
    }
}
