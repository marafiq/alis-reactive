namespace Alis.Reactive.Poc.PlaywrightTests.Texts;

[TestFixture]
public sealed class WhenADoseTextTestIsRefused : AppPageTest
{
    [Test]
    public async Task a_member_outside_the_table_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/TrimmedByChar");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Medicine.TrimStart(A)' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server. (Parameter 'predicate')");
    }

    [Test]
    public async Task starts_with_by_the_culture_is_refused_with_the_ordinal_door()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/StartsWithByCulture");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Medicine.StartsWith(\"Dig\")' compares by the culture in .NET, which the browser cannot match; pass StringComparison.Ordinal, such as d.Ward.StartsWith(\"N\", StringComparison.Ordinal). (Parameter 'predicate')");
    }

    [Test]
    public async Task a_comparison_other_than_ordinal_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/StartsWithIgnoringCase");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Medicine.StartsWith(\"dig\", OrdinalIgnoreCase)' compares text by OrdinalIgnoreCase; a list's lambda compares text by StringComparison.Ordinal. (Parameter 'predicate')");
    }

    [Test]
    public async Task trim_start_given_characters_to_trim_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/TrimmedByChars");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: 'd.Medicine.TrimStart(new [] {A, s})' has no browser form in a list's lambda; use a member the framework translates, or move the work to the server. (Parameter 'predicate')");
    }
}
