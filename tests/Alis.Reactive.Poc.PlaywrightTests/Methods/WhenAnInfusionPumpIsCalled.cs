namespace Alis.Reactive.Poc.PlaywrightTests.Methods;

[TestFixture]
public sealed class WhenAnInfusionPumpIsCalled : AppPageTest
{
    [Test]
    public async Task a_called_method_runs_and_the_step_after_it_runs()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Start the pump" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-state")).ToHaveTextAsync("running");
        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Started");
    }

    [Test]
    public async Task literal_arguments_reach_the_method_in_their_order()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Set the rate", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-rate")).ToHaveTextAsync("12.5 ml/h");
        await Expect(Page.Locator("#pump-status")).ToHaveTextAsync("Rate set");
    }

    [Test]
    public async Task an_argument_the_declaration_fixes_reaches_the_method_it_names_after_the_callers()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Set the rate to 8 ml/h" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-rate")).ToHaveTextAsync("8 ml/h");
    }

    [Test]
    public async Task a_list_passed_as_the_wider_type_the_method_takes_reaches_it_as_the_list()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the channel labels" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-labels")).ToHaveTextAsync("A, B, C");
    }

    [Test]
    public async Task an_argument_read_from_an_input_holds_what_the_input_holds_when_the_step_runs()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByLabel("Medicine").FillAsync("Heparin");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Look up the medicine's rate" }).ClickAsync();

        await Expect(Page.Locator("#census .census-charges")).ToHaveTextAsync("18");
    }

    [Test]
    public async Task a_result_read_twice_calls_its_method_once()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the summary", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-lead")).ToHaveTextAsync("rate none");
        await Expect(Page.Locator("#summary-copy")).ToHaveTextAsync("rate none");
        await Expect(Page.Locator("#pump .pump-summary-calls")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task two_calls_of_one_method_each_run_it()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the summary twice" }).ClickAsync();

        await Expect(Page.Locator("#summary-copy")).ToHaveTextAsync("rate none");
        await Expect(Page.Locator("#pump .pump-summary-calls")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_result_holds_what_its_method_returned_at_its_line_when_a_later_step_changes_the_pump()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Read the summary, then change the rate" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-rate")).ToHaveTextAsync("20 ml/h");
        await Expect(Page.Locator("#summary-copy")).ToHaveTextAsync("rate none");
    }

    [Test]
    public async Task a_member_of_the_data_a_method_returns_reads_past_the_result()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Show the volume limit" }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("500");
    }

    [Test]
    public async Task a_text_literal_argument_reaches_the_method()
    {
        await Open("/Pumps/Infusion");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Caption the pump" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("Night infusion");
    }

    [Test]
    public async Task a_null_literal_argument_reaches_the_method_as_null()
    {
        await Open("/Pumps/Infusion");
        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("Bed 4 pump");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Clear the caption" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("(no caption)");
    }

    [Test]
    public async Task a_null_the_returned_data_holds_reaches_the_method_as_null()
    {
        await Open("/Pumps/Infusion");
        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("Bed 4 pump");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Caption with the settings' note" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("(no caption)");
    }

    [Test]
    public async Task a_result_that_reads_nothing_reaches_the_method_as_null()
    {
        await Open("/Pumps/Infusion");
        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("Bed 4 pump");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Caption with the last alarm" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-caption")).ToHaveTextAsync("(no caption)");
    }
}
