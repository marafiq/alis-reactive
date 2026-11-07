namespace Alis.Reactive.Poc.PlaywrightTests.Comparisons;

[TestFixture]
public sealed class WhenASourceInItsOwnJsonIsRefused : AppPageTest
{
    [Test]
    public async Task an_ordering_of_a_found_number_its_class_writes_as_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceOrderedByItsHandling");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition orders 'd.Supply.Vials', which its class's JSON number handling writes unlike the app's options, so the browser does not order it as .NET does; order it on the server.");
    }

    [Test]
    public async Task a_found_object_its_own_converter_writes_compared_with_a_value_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceComparedThroughItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition compares 'd.Charter', which its own JSON converter writes unlike the app's options, and .NET compares the object it holds; compare it on the server.");
    }

    [Test]
    public async Task two_sources_compared_where_its_own_converter_writes_one_are_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourcesComparedWithItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition compares two values, and its own JSON converter writes 'd.Charted' unlike the app's options, so the browser does not compare them as .NET does; compare them on the server.");
    }

    [Test]
    public async Task an_equality_of_a_found_fraction_the_json_writes_as_text_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceComparedAsText");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition compares a fraction the JSON writes as text, whose text neither equals nor orders as the number does, as 0.10 and 0.1; compare it on the server.");
    }

    [Test]
    public async Task a_found_number_its_class_writes_as_text_set_into_a_component_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceSetIntoAComponent");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A live object takes a value as the app's options write its type, and 'd.Supply.Vials' is written by its class's JSON number handling; compare it with == or !=, or pass the value from the server.");
    }

    [Test]
    public async Task a_found_number_its_class_writes_as_text_passed_to_a_method_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourcePassedToAMethod");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A live object takes a value as the app's options write its type, and 'd.Supply.Vials' is written by its class's JSON number handling; compare it with == or !=, or pass the value from the server.");
    }

    [Test]
    public async Task a_found_text_its_own_converter_writes_answered_to_a_vendor_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceAnsweredToAVendor");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A live object takes a value as the app's options write its type, and 'd.Code' is written by its own JSON converter; compare it with == or !=, or pass the value from the server.");
    }

    [Test]
    public async Task a_sum_of_the_numbers_a_select_takes_from_such_a_member_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceSummedFromItsSelect");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Supply.Vials' is written by its class's JSON number handling, unlike the app's options, so the browser does not hold the value .NET computes with; take it whole in a Select, or compute on the server. (Parameter 'term')");
    }

    [Test]
    public async Task a_lambda_that_reads_past_an_item_its_own_converter_writes_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourceReadPastItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'charter.Name' reads past 'charter', which its own JSON converter writes, so the browser holds what the converter writes, not its members; read 'charter' itself, or read past it on the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task a_fallback_read_from_the_page_for_such_a_member_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/SourcesChosenWithItsConverter");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A choice joins 'd.Charted', which its own JSON converter writes unlike the app's options, with another value read from the page; choose between it and a value the view writes, or choose on the server.");
    }
}
