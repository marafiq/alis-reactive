namespace Alis.Reactive.Poc.PlaywrightTests.Choices;

[TestFixture]
public sealed class WhenADoseValueMayBeNull : AppPageTest
{
    [Test]
    public async Task has_value_holds_for_a_number_and_fails_for_null()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with a priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task has_value_through_a_holder_that_is_null_reads_nothing()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose carer has no grade", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task the_value_of_a_nullable_that_holds_null_reads_nothing()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose priority value is not 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_cast_of_a_nullable_that_holds_null_reads_nothing()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose priority cast to a number is not 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_cast_of_a_nullable_that_holds_null_in_a_select_is_null_in_its_list()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the priorities, cast to numbers, that are not 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_cast_of_a_nullable_the_view_holds_reads_its_value()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses of the routine priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task coalesce_reads_its_right_side_when_its_left_side_holds_null()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no ward", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task coalesce_reads_its_right_side_when_a_nullable_number_on_its_left_holds_null()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task coalesce_reads_its_right_side_when_its_left_side_reads_nothing()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no carer named", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task coalesce_does_not_read_its_right_side_when_its_left_side_holds_a_value()
    {
        await Open("/Doses/Choices");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with an empty note", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }
}
