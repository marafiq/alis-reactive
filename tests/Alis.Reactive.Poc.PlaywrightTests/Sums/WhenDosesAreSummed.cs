namespace Alis.Reactive.Poc.PlaywrightTests.Sums;

[TestFixture]
public sealed class WhenDosesAreSummed : AppPageTest
{
    [Test]
    public async Task an_int_sum_adds_each_doses_term()
    {
        await Open("/Doses/Sums");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the hours of the day doses are given", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("7");
    }

    [Test]
    public async Task an_int_sum_whose_running_total_passes_the_range_of_int_stops_the_steps_after_it()
    {
        await Open("/Doses/Sums");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the stock changes", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Summing stock");
        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("unknown");
    }

    [Test]
    public async Task a_decimal_sum_adds_as_decimal_arithmetic_does()
    {
        await Open("/Doses/Sums");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the charges", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-charges")).ToHaveTextAsync("0.3");
    }

    [Test]
    public async Task a_sum_over_no_items_is_zero()
    {
        await Open("/Doses/Sums");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the charges in the east ward", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-charges")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_decimal_sum_past_the_range_of_decimal_stops_the_steps_after_it()
    {
        await Open("/Doses/Sums");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the claims", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census-status")).ToHaveTextAsync("Summing claims");
        await Expect(Page.Locator("#census .census-charges")).ToHaveTextAsync("unknown");
    }

    [Test]
    public async Task a_double_sum_adds_as_binary_floating_point_does()
    {
        await Open("/Doses/Sums");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Sum the volumes", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-volume")).ToHaveTextAsync("0.30000000000000004");
    }
}
