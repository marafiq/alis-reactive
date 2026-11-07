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

    [Test]
    public async Task an_async_method_in_a_term_of_or_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/RecountInAnOr");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An async method is called in a term that runs only when it is reached (the right term of And, Or or IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its own, in the branch where its term is reached.");
    }

    [Test]
    public async Task a_method_called_between_a_when_and_its_else_if_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/MeasuredBetweenWhenAndElseIf");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An ElseIf or an Else continues the When it follows, before any other step of its block: write it in the same sentence.");
    }

    [Test]
    public async Task a_result_called_in_an_else_if_and_read_after_its_when_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/ElseIfResultAfterItsWhen");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A method's result called in an ElseIf's condition is read after its When, and the call runs only when the ElseIf is reached: call the method before the When to read its result after it.");
    }

    [Test]
    public async Task a_second_else_of_one_when_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/TwoElses");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An Else ends its When: write each ElseIf before it, and every step of the remaining case in that one Else.");
    }

    [Test]
    public async Task an_async_method_in_the_fallback_of_if_null_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Balances/RecountInAnIfNull");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An async method is called in a term that runs only when it is reached (the right term of And, Or or IfNull, or a branch of Choose), and the pipeline waits for an async method at its line: call it in a When of its own, in the branch where its term is reached.");
    }
}
