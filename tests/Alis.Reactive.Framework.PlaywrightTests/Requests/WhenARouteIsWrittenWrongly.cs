namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A request's URL template, route values, headers and query reads are checked where the page renders: a
/// placeholder no member fills, a brace closed but never opened or opened but never closed, a placeholder
/// named outside [a-zA-Z0-9_], a placeholder filled with a list, a record sent as a header, and a record or
/// the whole query read from the URL are each refused with a message that names the mistake.
/// </summary>
[TestFixture]
public sealed class WhenARouteIsWrittenWrongly : AppPageTest
{
    private async Task Render(string mistake) =>
        await Page.GotoAsync($"{BaseUrl}/Requests/RouteRefusals?mistake={mistake}");

    private ILocator Message(string text) => Page.GetByText(text).First;

    [Test]
    public async Task a_placeholder_no_member_fills_is_refused_with_the_member_that_would_fill_it()
    {
        await Render("LeaveAPlaceholderUnfilled");

        await Expect(Message("URL template '/Requests/Rooms/{roomId}' has placeholder '{roomId}', but the gathered request sets no member named 'roomId'. Gather<TRequest>(g => g.Set(x => x.RoomId, ..)) fills it.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_brace_closed_but_never_opened_is_refused()
    {
        await Render("CloseABraceNeverOpened");

        await Expect(Message("URL template '/Requests/Rooms/roomId}' is invalid: unexpected closing brace '}'.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_brace_opened_but_never_closed_is_refused()
    {
        await Render("OpenABraceNeverClosed");

        await Expect(Message("URL template '/Requests/Rooms/{roomId' is invalid: missing closing brace '}'.")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_placeholder_named_outside_the_allowed_letters_is_refused()
    {
        await Render("NameAPlaceholderBadly");

        await Expect(Message("URL template '/Requests/Rooms/{room-id}' is invalid: invalid placeholder '{room-id}'. Names must match [a-zA-Z0-9_] (ASCII only).")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_placeholder_filled_with_a_list_is_refused()
    {
        await Render("FillAPlaceholderWithAList");

        await Expect(Message("Route placeholder '{rooms}' needs a scalar value, but the member that fills it reads shape")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_record_sent_as_a_header_is_refused()
    {
        await Render("SendARecordAsAHeader");

        await Expect(Message("header 'X-Guide' requires a scalar type, but got shape")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_record_read_from_the_query_is_refused()
    {
        await Render("ReadARecordFromTheQuery");

        await Expect(Message("URL param 'Guide' requires a scalar type, but got shape")).ToBeVisibleAsync();
    }

    [Test]
    public async Task the_whole_query_read_as_one_key_is_refused()
    {
        await Render("ReadTheWholeQuery");

        await Expect(Message("Select a member of the query, for example q => q.FacilityId.")).ToBeVisibleAsync();
    }
}
