namespace Alis.Reactive.Poc.PlaywrightTests.Comparisons;

[TestFixture]
public sealed class WhenAMemberIsWrittenByItsOwnJson : AppPageTest
{
    [Test]
    public async Task an_equality_keeps_the_doses_whose_member_its_own_converter_writes_equals_the_value()
    {
        await Open("/Doses/OwnJson");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the doses charted as given", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task an_inequality_keeps_the_doses_whose_member_its_own_converter_writes_differs_from_the_value()
    {
        await Open("/Doses/OwnJson");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the doses not charted as given", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_member_its_own_converter_writes_that_the_dose_omits_reads_its_start_in_that_form()
    {
        await Open("/Doses/OwnJson");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the doses charted as due", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_select_takes_a_member_its_own_number_handling_writes_as_the_browser_holds_it()
    {
        await Open("/Doses/OwnJson");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the copays taken whole", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-charges")).ToHaveTextAsync("0.3");
    }

    [Test]
    public async Task an_equality_keeps_the_doses_whose_number_its_class_writes_as_text_equals_the_value()
    {
        await Open("/Doses/OwnJson");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the doses supplied in two vials", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_text_member_of_a_class_that_writes_its_numbers_as_text_computes_as_dotnet_does()
    {
        await Open("/Doses/OwnJson");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the doses the pharmacy supplies", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }
}
