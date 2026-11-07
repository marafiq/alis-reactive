namespace Alis.Reactive.Poc.PlaywrightTests.Choices;

[TestFixture]
public sealed class WhenADoseValueIsChosen : AppPageTest
{
    [Test]
    public async Task a_choice_reads_its_first_branch_when_its_test_holds_and_its_second_branch_when_it_fails()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses over 50 mg by mouth", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_choice_whose_test_reads_nothing_reads_its_second_branch()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses not given by Ruth", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task a_choice_does_not_read_its_second_branch_when_its_test_holds()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses held as Digoxin", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_choice_does_not_read_its_first_branch_when_its_test_fails()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses held unless not Digoxin", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }
}
