namespace Alis.Reactive.Poc.PlaywrightTests.Plugins;

[TestFixture]
public sealed class WhenAPageUsesAPlugin : AppPageTest
{
    [Test]
    public async Task a_plugin_method_a_step_calls_changes_what_the_plugin_property_reads()
    {
        await Open("/Shifts/Handover");
        await AddNote("Fall risk in room 4");

        await Button("Read the last note").ClickAsync();

        await Expect(Page.Locator("#last-note")).ToHaveTextAsync("Fall risk in room 4");
    }

    [Test]
    public async Task a_plugin_event_runs_its_handler_with_the_detail_the_plugin_raises()
    {
        await Open("/Shifts/Handover");

        await AddNote("Fall risk in room 4");

        await Expect(Page.Locator("#noted")).ToHaveTextAsync("Fall risk in room 4");
    }

    [Test]
    public async Task a_plugin_method_result_decides_a_condition_whose_branch_reads_the_plugin()
    {
        await Open("/Shifts/Handover");
        await Button("Check the log").ClickAsync();
        await Expect(Page.Locator("#closing-note")).ToHaveTextAsync("empty");
        await AddNote("Fall risk in room 4");
        await Button("Check the log").ClickAsync();
        await Expect(Page.Locator("#closing-note")).ToHaveTextAsync("room left");

        await AddNote("Night fluids charted");
        await AddNote("Doctor called for room 9");
        await Button("Check the log").ClickAsync();

        await Expect(Page.Locator("#closing-note")).ToHaveTextAsync("Doctor called for room 9");
    }

    [Test]
    public async Task a_plugin_property_a_step_writes_changes_what_the_plugin_method_answers()
    {
        await Open("/Shifts/Handover");
        await AddNote("Fall risk in room 4");
        await AddNote("Night fluids charted");
        await AddNote("Doctor called for room 9");
        await Button("Check the log").ClickAsync();
        await Expect(Page.Locator("#closing-note")).ToHaveTextAsync("Doctor called for room 9");

        await Button("Allow five notes").ClickAsync();
        await Button("Check the log").ClickAsync();

        await Expect(Page.Locator("#closing-note")).ToHaveTextAsync("room left");
    }

    [Test]
    public async Task a_plugin_that_is_itself_a_function_is_called_with_the_argument_a_step_reads()
    {
        await Open("/Shifts/Handover");
        await Page.GetByLabel("Resident").FillAsync("Helen Park");

        await Button("Show the resident's initials").ClickAsync();

        await Expect(Page.Locator("#initials")).ToHaveTextAsync("HP");
    }

    [Test]
    public async Task a_plugin_called_inside_a_callback_answer_answers_each_tick()
    {
        await Open("/Shifts/Handover");
        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("1, 2, 3");

        await Button("Label the ticks by their initials").ClickAsync();

        await Expect(Page.Locator("#chart .chart-labels")).ToHaveTextAsync("M, T, W");
    }

    [Test]
    public async Task a_plugin_event_raised_through_a_callback_property_runs_its_handler()
    {
        await Open("/Shifts/Handover");

        await Button("Strike the hour").ClickAsync();

        await Expect(Page.Locator("#hour")).ToHaveTextAsync("23:00");
    }

    [Test]
    public async Task a_plugin_event_raised_to_a_subscriber_whose_unsubscribe_is_returned_runs_its_handler()
    {
        await Open("/Shifts/Handover");

        await Button("Assign bed 12B").ClickAsync();

        await Expect(Page.Locator("#assigned-bed")).ToHaveTextAsync("12B");
    }

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    private async Task AddNote(string note)
    {
        await Page.GetByLabel("Note", new() { Exact = true }).FillAsync(note);
        await Button("Add the note").ClickAsync();
    }
}
