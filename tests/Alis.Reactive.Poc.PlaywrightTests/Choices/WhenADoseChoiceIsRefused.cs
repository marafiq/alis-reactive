namespace Alis.Reactive.Poc.PlaywrightTests.Choices;

[TestFixture]
public sealed class WhenADoseChoiceIsRefused : AppPageTest
{
    [Test]
    public async Task a_coalesce_that_converts_its_left_side_by_a_user_defined_conversion_is_refused()
    {
        await Page.GotoAsync(BaseUrl + "/Doses/CoalescedByAConversion");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "ArgumentException: '(d.Seal ?? \"unsealed\")' converts its left side by a user-defined conversion, which the browser does not have; coalesce values of one type, or move the work to the server. (Parameter 'predicate')");
    }
}
