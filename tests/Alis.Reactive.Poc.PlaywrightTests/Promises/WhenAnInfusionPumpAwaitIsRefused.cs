namespace Alis.Reactive.Poc.PlaywrightTests.Promises;

[TestFixture]
public sealed class WhenAnInfusionPumpAwaitIsRefused : AppPageTest
{
    [Test]
    public async Task an_async_methods_result_read_inside_a_lists_lambda_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/SelfCheckedInALambda");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: An async method is called inside a list's lambda, which reads every item in one turn and waits for nothing. Call the method in the pipeline and read its result in the lambda.");
    }

    [Test]
    public async Task an_async_method_called_as_a_step_inside_a_lists_lambda_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/RecalibratedInALambda");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline.");
    }
}
