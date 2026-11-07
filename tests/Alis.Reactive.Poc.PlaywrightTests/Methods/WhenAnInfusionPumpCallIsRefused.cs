namespace Alis.Reactive.Poc.PlaywrightTests.Methods;

[TestFixture]
public sealed class WhenAnInfusionPumpCallIsRefused : AppPageTest
{
    [Test]
    public async Task a_method_called_as_a_step_inside_a_lists_lambda_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/PausedInALambda");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A step is written inside a list's lambda, which reads each item and writes nothing. Write the step in the pipeline.");
    }
}
