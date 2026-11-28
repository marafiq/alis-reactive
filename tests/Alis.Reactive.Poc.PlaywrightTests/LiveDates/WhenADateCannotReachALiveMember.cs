namespace Alis.Reactive.Poc.PlaywrightTests.LiveDates;

[TestFixture]
public sealed class WhenADateCannotReachALiveMember : AppPageTest
{
    [Test]
    public async Task a_dictionary_of_dates_written_into_a_live_member_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Visits/PlannedDays");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A value written into a live member is a dictionary whose values hold dates. A date reaches a live member by its member's name: pass the entries as a list of objects that name each key.");
    }

    [Test]
    public async Task a_value_holding_its_own_type_and_a_date_written_into_a_live_member_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Visits/FamilyVisits");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A value written into a live member holds its own type and a date in it. A date reaches a live member through a shape that ends: pass the dates as a list of their own.");
    }
}
