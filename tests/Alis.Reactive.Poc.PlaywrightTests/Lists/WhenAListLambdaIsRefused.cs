namespace Alis.Reactive.Poc.PlaywrightTests.Lists;

[TestFixture]
public sealed class WhenAListLambdaIsRefused : AppPageTest
{
    [Test]
    public async Task a_step_written_inside_a_lists_lambda_is_refused_with_where_the_step_belongs()
    {
        await Page.GotoAsync(BaseUrl + "/Stations/StepInsideAList");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline.");
    }
}
