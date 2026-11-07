namespace Alis.Reactive.Poc.PlaywrightTests.Comparisons;

[TestFixture]
public sealed class WhenADoseComparisonIsRefused : AppPageTest
{
    [Test]
    public async Task an_object_compared_with_a_value_the_view_holds_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComparedObjects");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Carer == Model.Lead)' compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead. (Parameter 'predicate')");
    }

    [Test]
    public async Task two_members_that_hold_objects_compared_with_each_other_are_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComparedMemberObjects");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Carer == d.Witness)' compares objects, and the browser compares objects by reference; compare a member of each, such as d => d.Carer.Name == lead. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_date_compared_in_a_lists_lambda_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComparedDates");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Scheduled > Model.Since)' compares dates or times, whose text neither orders nor equals as the instant does; compare them on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task an_enum_the_app_writes_by_name_ordered_against_a_value_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/OrderedByName");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(Convert(d.Status, Int32) > 0)' orders a value the app's JSON writes as text, which does not order as the value does in .NET; compare it with == or !=, or order on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task two_members_the_browser_cannot_order_ordered_against_each_other_are_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/OrderedMembers");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(Convert(d.Status, Int32) > Convert(d.Planned, Int32))' orders values the browser cannot order as .NET does; order numbers or a char, or order on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_cast_that_narrows_a_number_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/NarrowingCast");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'Convert(d.Milligrams, Int32)' casts a number to a narrower one, which changes the value as C# computes it and not as the browser does; compare the member itself. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_view_value_that_computes_is_refused_with_the_local_that_carries_it()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComputedViewValue");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'Model.Ward.ToUpper()' computes a value inside a list's lambda; compute it into a local before the lambda, such as var ward = Model.Ward.ToUpper();. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_cast_of_a_view_value_that_is_null_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/CastNullViewValue");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'none' is null when the view renders, and the cast takes its value, which .NET refuses; compare the nullable itself, such as d => d.Priority == limit. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_view_value_read_through_a_null_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/NullViewValue");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'Model.Cover.Name' reads a member of a value that is null when the view renders. (Parameter 'predicate')");
    }
}
