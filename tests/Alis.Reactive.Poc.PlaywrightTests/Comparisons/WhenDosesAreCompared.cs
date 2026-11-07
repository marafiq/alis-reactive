namespace Alis.Reactive.Poc.PlaywrightTests.Comparisons;

[TestFixture]
public sealed class WhenDosesAreCompared : AppPageTest
{
    [Test]
    public async Task an_equality_keeps_the_doses_whose_member_equals_the_value()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count priority 2 doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task an_inequality_keeps_the_doses_whose_member_differs_a_null_member_included()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses not at priority 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task less_than_keeps_the_doses_below_the_value_and_never_a_null_member()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses below priority 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task at_most_keeps_the_doses_at_or_below_the_value_and_never_a_null_member()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses at most priority 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task greater_than_keeps_the_doses_above_the_value()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses above priority 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task at_least_keeps_the_doses_at_or_above_the_value()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses at least priority 2", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_member_of_the_views_model_is_compared_as_the_view_rendered_it()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count this ward's doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_local_the_lambda_captures_is_compared_as_the_view_holds_it()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count the chosen ward's doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_static_member_the_lambda_reads_is_compared_as_the_view_holds_it()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses above routine priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_member_that_holds_null_equals_null()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with no carer", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task a_member_that_holds_a_value_differs_from_null()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses with a carer", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task an_inequality_holds_on_a_member_the_dose_lacks_whose_start_is_null()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses off the north ward", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task not_of_an_equality_holds_on_a_member_the_dose_lacks_whose_start_is_null()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses not on the north ward", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task or_holds_when_its_right_term_holds_though_its_left_reads_nothing()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses witnessed by Ruth or missed", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task and_is_false_when_its_right_term_is_false_though_its_left_reads_nothing()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses not both witnessed by Ruth and given", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task and_keeps_only_the_doses_that_hold_both_terms()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count due urgent doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("1");
    }

    [Test]
    public async Task or_keeps_the_doses_that_hold_either_term()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count missed or first priority doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task not_keeps_the_doses_its_term_does_not_hold()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses not given", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task an_enum_value_is_compared_as_the_apps_json_writes_it()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count due doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_char_value_is_compared_as_the_apps_json_writes_it()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count injected doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_member_compared_with_another_member_of_the_same_dose_equals_it_by_value()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses given as planned", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task two_number_members_are_ordered_and_a_null_member_never_holds()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses whose milligrams pass their priority", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_char_value_is_ordered_as_dotnet_orders_chars()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count doses on a route before L", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_decimal_is_compared_with_a_whole_number_through_dotnets_own_conversion()
    {
        await Open("/Doses/Round");

        await Page.GetByRole(AriaRole.Button, new() { Name = "Count charged doses", Exact = true }).ClickAsync();

        await Expect(Page.Locator("#census .census-count")).ToHaveTextAsync("2");
    }
}
