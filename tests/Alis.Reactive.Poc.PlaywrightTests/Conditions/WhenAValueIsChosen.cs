namespace Alis.Reactive.Poc.PlaywrightTests.Conditions;

[TestFixture]
public sealed class WhenAValueIsChosen : AppPageTest
{
    [Test]
    public async Task a_value_that_holds_null_reads_its_fallback_and_calls_its_method()
    {
        await Open("/Monitors/NotesShown");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise an unnoted alarm" }).ClickAsync();

        await Expect(Page.Locator("#note")).ToHaveTextAsync("rate none");
        await Expect(Page.Locator("#pump .pump-summary-calls")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_fallback_method_is_not_called_when_the_value_is_held()
    {
        await Open("/Monitors/NotesShown");

        await Page.Locator("#monitor").GetByRole(AriaRole.Button, new() { Name = "Raise a noted alarm" }).ClickAsync();

        await Expect(Page.Locator("#note")).ToHaveTextAsync("after surgery");
        await Expect(Page.Locator("#pump .pump-summary-calls")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_value_that_reads_nothing_passes_its_fallback_as_a_methods_argument()
    {
        await Open("/Pumps/LastAlarmNoted");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Note the last alarm" }).ClickAsync();

        await Expect(Page.Locator("#pump .pump-notes")).ToHaveTextAsync("no alarm");
    }

    [Test]
    public async Task a_condition_compares_the_fallback_of_a_null_value()
    {
        await Open("/Balances/LightPatients");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check for a light patient" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("light");
    }

    [Test]
    public async Task a_condition_compares_a_held_value_and_not_its_fallback()
    {
        await Open("/Balances/LightPatients");
        await Balance("Weigh 70 kg").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Check for a light patient" }).ClickAsync();

        await Expect(Page.Locator("#verdict")).ToHaveTextAsync("not light");
    }

    [Test]
    public async Task a_method_in_the_branch_not_chosen_is_not_called()
    {
        await Open("/Balances/FluidTargets");
        await Balance("Weigh 70 kg").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Set the fluid target" }).ClickAsync();

        await Expect(Page.Locator("#balance .balance-intake")).ToHaveTextAsync("1500 ml");
        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("not measured");
    }

    [Test]
    public async Task a_test_that_reads_nothing_chooses_its_second_value()
    {
        await Open("/Balances/FluidTargets");
        await Balance("Add 100 ml").ClickAsync();

        await Page.GetByRole(AriaRole.Button, new() { Name = "Set the fluid target" }).ClickAsync();

        await Expect(Page.Locator("#balance .balance-measured")).ToHaveTextAsync("measured at 100 ml");
        await Expect(Page.Locator("#balance .balance-intake")).ToHaveTextAsync("100 ml");
    }

    private ILocator Balance(string button) => Page.Locator("#balance").GetByRole(AriaRole.Button, new() { Name = button });
}
