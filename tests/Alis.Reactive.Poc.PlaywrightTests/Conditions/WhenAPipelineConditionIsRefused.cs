namespace Alis.Reactive.Poc.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAPipelineConditionIsRefused : AppPageTest
{
    [Test]
    public async Task a_condition_that_orders_dates_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/OrderedDates");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition compares dates or times, whose text neither orders nor equals as the instant does: compare them on the server.");
    }

    [Test]
    public async Task a_condition_that_orders_flags_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/OrderedFlags");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition orders values the browser cannot order as .NET does: order numbers or a char, or order on the server.");
    }

    [Test]
    public async Task a_condition_that_compares_objects_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/ComparedObjects");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A condition compares objects, and the browser compares objects by reference: compare a member of each.");
    }

    [Test]
    public async Task an_else_written_after_another_step_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/ElseAfterAStep");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence.");
    }

    [Test]
    public async Task a_result_read_outside_the_branch_that_called_its_method_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/ResultOutsideItsBranch");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it.");
    }
}
