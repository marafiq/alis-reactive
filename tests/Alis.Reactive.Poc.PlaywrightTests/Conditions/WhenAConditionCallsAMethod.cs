namespace Alis.Reactive.Poc.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAConditionCallsAMethod : AppPageTest
{
    [Test]
    public async Task a_method_in_the_right_term_of_or_is_not_called_when_the_left_term_holds()
    {
        await Open("/Balances/Measured");
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Flag a high intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("flagged");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("not measured");
    }

    [Test]
    public async Task a_method_in_the_right_term_of_or_is_called_when_the_left_term_does_not_hold()
    {
        await Open("/Balances/Measured");
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Flag a high intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("flagged");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("measured at 100 ml");
    }

    [Test]
    public async Task a_method_in_the_right_term_of_and_is_not_called_when_the_left_term_is_false()
    {
        await Open("/Balances/Measured");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("nothing taken");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("not measured");
    }

    [Test]
    public async Task a_method_in_the_right_term_of_and_is_called_when_the_left_term_holds()
    {
        await Open("/Balances/Measured");
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Confirm the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("confirmed");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("measured at 100 ml");
    }

    [Test]
    public async Task a_method_in_an_else_if_is_not_called_when_an_earlier_branch_runs()
    {
        await Open("/Balances/MeasuredElseIf");
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("high");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("not measured");
    }

    [Test]
    public async Task a_method_in_an_else_if_is_called_when_its_else_if_is_reached()
    {
        await Open("/Balances/MeasuredElseIf");
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("measured");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("measured at 100 ml");
    }

    [Test]
    public async Task an_async_method_in_an_else_if_is_not_called_when_an_earlier_branch_runs()
    {
        await Open("/Balances/RecountedElseIf");
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("high");
        await Expect(Page.Locator("#balance .balance-recount")).ToHaveTextAsync("not recounted");
    }

    [Test]
    public async Task an_else_if_runs_its_branch_once_its_async_method_settles()
    {
        await Open("/Balances/RecountedElseIf");
        await Balance("Add 100 ml").ClickAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();
        await Expect(Page.Locator("#balance .balance-recount")).ToHaveTextAsync("recounting");

        await Balance("Finish the recount").ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("recounted");
    }

    [Test]
    public async Task a_second_else_if_is_not_reached_when_the_first_else_if_holds()
    {
        await Open("/Balances/RecountedElseIf");
        await Balance("Add 500 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("measured high");
        await Expect(Page.Locator("#balance .balance-recount")).ToHaveTextAsync("not recounted");
    }

    [Test]
    public async Task a_result_called_before_a_step_is_what_it_returned_at_its_line_when_a_term_reads_it()
    {
        await Open("/Balances/MeasuredBeforeAStep");
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Assess the intake measured first" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("under 500 ml");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("measured at 100 ml");
    }

    [Test]
    public async Task a_result_a_term_and_its_branch_read_is_what_it_returned_at_its_line()
    {
        await Open("/Balances/MeasuredAndRestored");
        await Balance("Add 500 ml").ClickAsync();
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Restore the measured intake" }).ClickAsync();

        await Expect(Page.Locator("#balance .balance-intake")).ToHaveTextAsync("600 ml");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("measured at 600 ml");
    }

    private ILocator Balance(string button) => Page.Locator("#balance").GetByRole(AriaRole.Button, new() { Name = button });
}
