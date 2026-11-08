namespace Alis.Reactive.Poc.PlaywrightTests.Handovers;

[TestFixture]
public sealed class WhenAHandoverRequestIsRefused : AppPageTest
{
    [Test]
    public async Task a_day_in_a_placeholder_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Handovers/DateInPath");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Handovers/Planned/{plannedFor}' fills its placeholder PlannedFor with a date, which a URL's path does not carry, since IIS on .NET Framework refuses a time in a path segment; take it out of the URL, and the request sends it in the query. (Parameter 'url')");
    }

    [Test]
    public async Task a_moment_in_a_placeholder_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Handovers/InstantInPath");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Handovers/Starts/{startsAt}' fills its placeholder StartsAt with an instant, which a URL's path does not carry, since IIS on .NET Framework refuses a time in a path segment; take it out of the URL, and the request sends it in the query. (Parameter 'url')");
    }

    [Test]
    public async Task a_list_in_a_placeholder_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Handovers/ListInPath");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: The URL '/Handovers/Beds/{beds}' fills its placeholder Beds with a list, which one path segment cannot hold; take it out of the URL, and the request sends each item in the query. (Parameter 'url')");
    }

    [Test]
    public async Task a_member_holding_an_object_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Handovers/ObjectMember");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'r => r.Handover' holds an object, and a request sends only text, numbers, booleans, dates, instants and lists of them. (Parameter 'member')");
    }

    [Test]
    public async Task a_member_holding_a_dictionary_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Handovers/DictionaryMember");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'r => r.Counts' holds a dictionary, which a request does not send in its query; send each entry the endpoint reads as a member of its own. (Parameter 'member')");
    }

    [Test]
    public async Task a_member_holding_a_list_of_lists_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Handovers/ListOfLists");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'r => r.Bays' holds a list of lists, whose items no query key names; send each inner list as a member of its own. (Parameter 'member')");
    }
}
