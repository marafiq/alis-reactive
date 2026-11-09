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

    [Test]
    public async Task a_result_read_by_another_pipeline_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/SummaryReadByAnotherPipeline");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(ReadOutsideItsBlock);
    }

    [Test]
    public async Task a_returned_channel_read_by_another_pipeline_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/ChannelReadByAnotherPipeline");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(ReadOutsideItsBlock);
    }

    [Test]
    public async Task returned_channels_read_by_another_pipeline_are_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/ChannelsReadByAnotherPipeline");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(ReadOutsideItsBlock);
    }

    [Test]
    public async Task a_result_read_kept_out_of_its_branch_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/NoteKeptOutOfItsBranch");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(ReadOutsideItsBlock);
    }

    [Test]
    public async Task a_returned_channel_kept_out_of_its_branch_and_paused_after_it_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/ChannelKeptOutOfItsBranch");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(ReadOutsideItsBlock);
    }

    [Test]
    public async Task a_channel_kept_out_of_its_lambda_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Pumps/ChannelKeptOutOfItsLambda");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A list's item is read outside the lambda that receives it. Read the item in its lambda, such as beds.Any(bed => bed.Free).");
    }

    private const string ReadOutsideItsBlock =
        "InvalidOperationException: A method's result is read outside the block that called the method. Call the method in the block that reads its result, or in a block around it.";
}
