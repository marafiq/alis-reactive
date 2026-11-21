namespace Alis.Reactive.Poc.PlaywrightTests.Comparisons;

[TestFixture]
public sealed class WhenAMemberWrittenByItsOwnJsonIsRefused : AppPageTest
{
    [Test]
    public async Task an_ordering_of_a_member_its_own_converter_writes_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/OrderedByItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(Convert(d.Charted, Int32) > 0)' orders 'd.Charted', which its own JSON converter writes unlike the app's options, so the browser does not order it as .NET does; order it on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task an_ordering_of_a_number_its_class_writes_as_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/OrderedByItsClassNumberHandling");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Supply.Vials > 1)' orders 'd.Supply.Vials', which its class's JSON number handling writes unlike the app's options, so the browser does not order it as .NET does; order it on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task two_members_compared_where_its_own_converter_writes_one_are_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComparedWithItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(Convert(d.Charted, Int32) == Convert(d.Status, Int32))' compares two members, and its own JSON converter writes 'd.Charted' unlike the app's options, so the browser does not compare them as .NET does; compare them on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task an_equality_of_a_fraction_the_json_writes_as_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComparedAsText");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Copay == 0.10)' compares a fraction the JSON writes as text, whose text neither equals nor orders as the number does, as 0.10 and 0.1; compare it on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task an_object_its_own_converter_writes_compared_with_a_value_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ComparedThroughItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Charter == Model.Lead)' compares 'd.Charter', which its own JSON converter writes unlike the app's options, and .NET compares the object it holds; compare it on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_lambda_that_reads_past_a_member_its_own_converter_writes_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/ReadPastItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Charter.Name' reads past 'd.Charter', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'd.Charter' itself, or read past it on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_field_found_past_a_member_its_own_converter_writes_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/FoundPastItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Charter.Name' reads past 'd.Charter', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'd.Charter' itself, or read past it on the server. (Parameter 'field')");
    }

    [Test]
    public async Task a_sort_key_its_own_number_handling_writes_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SortedByItsNumberHandling");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Copay' is written by its own JSON number handling, unlike the app's options, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server. (Parameter 'key')");
    }

    [Test]
    public async Task a_sum_term_that_casts_a_member_its_class_writes_as_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SummedThroughACast");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Supply.Vials' is written by its class's JSON number handling, unlike the app's options, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server. (Parameter 'term')");
    }

    [Test]
    public async Task a_lookup_of_a_member_its_own_converter_writes_in_a_list_the_view_holds_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/LookedUpByItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Charted' is written by its own JSON converter, unlike the app's options, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server. (Parameter 'predicate')");
    }
}
