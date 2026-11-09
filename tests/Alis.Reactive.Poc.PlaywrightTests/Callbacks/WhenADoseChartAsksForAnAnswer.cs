namespace Alis.Reactive.Poc.PlaywrightTests.Callbacks;

[TestFixture]
public sealed class WhenADoseChartAsksForAnAnswer : AppPageTest
{
    [Test]
    public async Task a_callback_property_answers_each_tick_with_the_member_the_answer_reads()
    {
        await Open("/Charts/Doses");
        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("1, 2, 3");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by day" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("Mon, Tue, Wed");
    }

    [Test]
    public async Task each_call_of_a_callback_reads_its_answer_as_the_page_is_when_the_chart_calls_it()
    {
        await Open("/Charts/Doses");
        await Page.GetByLabel("Unit").FillAsync("doses");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by the unit typed" }).ClickAsync();
        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("doses, doses, doses");

        await Page.GetByLabel("Unit").FillAsync("units");
        await Page.Locator("#chart").GetByRole(AriaRole.Button, new() { Name = "Redraw the chart" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("units, units, units");
    }

    [Test]
    public async Task a_method_called_in_an_answer_runs_on_each_call()
    {
        await Open("/Charts/ChartUnit");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by the chart's unit" }).ClickAsync();
        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("mg, mg, mg");

        await Page.Locator("#chart").GetByRole(AriaRole.Button, new() { Name = "Use millilitres" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("ml, ml, ml");
    }

    [Test]
    public async Task an_answer_reads_the_argument_its_vendor_passes_at_a_position()
    {
        await Open("/Charts/Doses");
        await Expect(Page.Locator("#chart .chart-values")).ToHaveTextAsync("8, 5, 6");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show each tick's label as its value" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-values")).ToHaveTextAsync("Mon, Tue, Wed");
    }

    [Test]
    public async Task an_answer_passed_as_an_argument_is_the_key_the_chart_sorts_by()
    {
        await Open("/Charts/Doses");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by day" }).ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sort the ticks by doses" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("Tue, Wed, Mon");
    }

    [Test]
    public async Task an_async_method_read_in_an_answer_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Charts/AwaitedInAnAnswer");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A callback answers its vendor at once, so its answer cannot wait for an async method or a request.");
    }

    [Test]
    public async Task a_step_written_in_an_answer_is_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Charts/StepInAnAnswer");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A step is written inside a callback's answer, which answers its vendor with one value. Write the step in a pipeline.");
    }

    [Test]
    public async Task a_ticks_arguments_kept_out_of_the_answer_are_refused_where_the_view_renders()
    {
        await Page.GotoAsync(BaseUrl + "/Charts/TickKeptOutOfItsAnswer");

        await Expect(Page.Locator(".titleerror")).ToHaveTextAsync(
            "InvalidOperationException: A callback's arguments are read outside the answer that receives them. Read them in the answer, such as tick => tick.Label.");
    }

    [Test]
    public async Task the_step_after_an_answer_the_chart_asked_for_runs()
    {
        await Open("/Charts/UnlabelledTick");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by day" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("Mon, Tue, Wed");
        await Expect(Page.Locator("#labelled")).ToHaveTextAsync("yes");
    }

    [Test]
    public async Task an_answer_that_stops_in_a_step_stops_the_pipeline_before_its_next_step()
    {
        await Open("/Charts/UnlabelledTick");
        await Page.Locator("#chart").GetByRole(AriaRole.Button, new() { Name = "Add a tick with no label" }).ClickAsync();
        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("1, 2, 3, 4");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by day" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("1, 2, 3, 4");
        await Expect(Page.Locator("#labelled")).ToHaveTextAsync("");
    }

    [Test]
    public async Task an_answer_that_stops_in_the_charts_own_turn_fails_the_charts_call()
    {
        await Open("/Charts/UnlabelledTick");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Label the ticks by day" }).ClickAsync();

        await Page.Locator("#chart").GetByRole(AriaRole.Button, new() { Name = "Add a tick with no label" }).ClickAsync();

        await Expect(Page.Locator("#chart .chart-count")).ToHaveTextAsync("4 ticks");
        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("Mon, Tue, Wed");
    }
}
