namespace Alis.Reactive.Poc.PlaywrightTests.Sorts;

[TestFixture]
public sealed class WhenADoseSortIsRefused : AppPageTest
{
    [Test]
    public async Task text_ordered_by_the_culture_is_refused_with_the_ordinal_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SortedByCulture");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Prescriber' orders text by the culture in .NET, an order net48 and net10 do not share; pass StringComparer.Ordinal, such as OrderBy(d => d.Prescriber, StringComparer.Ordinal), or order on the server. (Parameter 'key')");
    }

    [Test]
    public async Task a_key_the_apps_json_writes_as_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SortedByStatus");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Status' orders a value the app's JSON writes as text, which does not order as the value does in .NET; order by a member the JSON writes as a number, or order on the server. (Parameter 'key')");
    }

    [Test]
    public async Task a_date_key_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SortedBySchedule");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Scheduled' orders dates or times, whose text does not order as the instant does; order them on the server. (Parameter 'key')");
    }

    [Test]
    public async Task an_object_key_is_refused_with_the_member_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SortedByCarer");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Carer' orders objects, which the browser does not order; order by a member, such as OrderBy(d => d.Carer.Name, StringComparer.Ordinal). (Parameter 'key')");
    }

    [Test]
    public async Task a_comparer_other_than_ordinal_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SortedIgnoringCase");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Prescriber' orders text by a comparer the browser does not have; pass StringComparer.Ordinal, or order on the server. (Parameter 'comparer')");
    }
}
