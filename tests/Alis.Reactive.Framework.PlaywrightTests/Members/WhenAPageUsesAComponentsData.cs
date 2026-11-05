namespace Alis.Reactive.Framework.PlaywrightTests.Members;

[TestFixture]
public sealed class WhenAPageUsesAComponentsData : AppPageTest
{
    private ILocator Slate => Page.Locator("#slate");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_member_of_a_data_value_the_component_holds_is_read()
    {
        await Open("/Members/Data");

        await Click("Fill Cy's card");
        await Click("Fill Ada's card");
        await Click("Read the last card's name");

        await Expect(Page.Locator("#last-card")).ToHaveTextAsync("Ada");
    }

    [Test]
    public async Task a_member_of_a_data_value_the_component_does_not_hold_yet_reads_as_nothing()
    {
        await Open("/Members/Data");

        await Click("Is a card filled?");

        await Expect(Page.Locator("#card-state")).ToHaveTextAsync("no card yet");
    }

    [Test]
    public async Task a_member_of_a_data_value_the_component_holds_is_tested_as_its_value()
    {
        await Open("/Members/Data");
        await Click("Fill Ada's card");

        await Click("Is a card filled?");

        await Expect(Page.Locator("#card-state")).ToHaveTextAsync("Ada");
    }

    [Test]
    public async Task an_item_of_a_list_of_data_that_lacks_the_field_reads_it_as_nothing()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Start a card due on the 31st");
        await Click("Fill Ada's card");
        await Expect(Slate.GetByText("Cards: Cy · (unnamed) · Ada", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the named cards");

        await Expect(Page.Locator("#named-count")).ToHaveTextAsync("2");
    }

    [Test]
    public async Task a_method_of_the_text_each_item_holds_is_called_and_an_item_without_the_text_reads_as_nothing()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Start a card due on the 31st");
        await Click("Fill Ada's card");
        await Expect(Slate.GetByText("Cards: Cy · (unnamed) · Ada", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the cards with a written name");

        await Expect(Page.Locator("#written-count")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#written-went-on")).ToHaveTextAsync("went on");
    }

    [Test]
    public async Task a_method_of_text_called_on_an_item_that_holds_no_text_stops_the_rest_of_its_pipeline()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Keep a card the old way");
        await Expect(Slate.GetByText("Cards: Cy · (kept the old way)", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the cards with a written name");
        await Click("Count the named cards");

        await Expect(Page.Locator("#named-count")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#written-count")).ToHaveTextAsync("");
        await Expect(Page.Locator("#written-went-on")).ToHaveTextAsync("");
    }

    [Test]
    public async Task the_length_of_the_text_each_item_holds_is_read_and_an_item_without_the_text_reads_as_nothing()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Start a card due on the 31st");
        await Click("Fill Ada's card");
        await Expect(Slate.GetByText("Cards: Cy · (unnamed) · Ada", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the cards with a name of any length");

        await Expect(Page.Locator("#long-count")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#long-went-on")).ToHaveTextAsync("went on");
    }

    [Test]
    public async Task the_length_of_text_read_on_an_item_that_holds_no_text_stops_the_rest_of_its_pipeline()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Keep a card the old way");
        await Expect(Slate.GetByText("Cards: Cy · (kept the old way)", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the cards with a name of any length");
        await Click("Count the named cards");

        await Expect(Page.Locator("#named-count")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#long-count")).ToHaveTextAsync("");
        await Expect(Page.Locator("#long-went-on")).ToHaveTextAsync("");
    }

    [Test]
    public async Task the_day_of_the_date_each_item_holds_is_read()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Start a card due on the 31st");
        await Click("Fill Ada's card");
        await Expect(Slate.GetByText("Cards: Cy · (unnamed) · Ada", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the cards due on the 31st");

        await Expect(Page.Locator("#due-count")).ToHaveTextAsync("1");
        await Expect(Page.Locator("#due-went-on")).ToHaveTextAsync("went on");
    }

    [Test]
    public async Task the_day_read_on_an_item_that_holds_no_date_stops_the_rest_of_its_pipeline()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Keep a card the old way");
        await Expect(Slate.GetByText("Cards: Cy · (kept the old way)", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Count the cards due on the 31st");
        await Click("Count the named cards");

        await Expect(Page.Locator("#named-count")).ToHaveTextAsync("2");
        await Expect(Page.Locator("#due-count")).ToHaveTextAsync("");
        await Expect(Page.Locator("#due-went-on")).ToHaveTextAsync("");
    }

    [Test]
    public async Task one_field_of_each_item_of_a_list_of_data_is_read()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Fill Ada's card");
        await Click("Fill Ben's card");
        await Expect(Slate.GetByText("Cards: Cy · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Summarize the cards");

        await Expect(Slate.GetByText("Cy · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_nullable_property_is_written_from_a_value_that_is_never_missing()
    {
        await Open("/Members/Data");
        await Expect(Slate.GetByText("No limit", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Limit to the rows");

        await Expect(Slate.GetByText("Limit: 2", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_write_only_property_is_written_from_a_value_read_when_the_step_runs()
    {
        await Open("/Members/Data");

        await Click("Note the title");

        await Expect(Slate.GetByText("Day round", new() { Exact = true })).ToHaveCountAsync(2);
    }

    [Test]
    public async Task a_field_named_by_a_member_of_the_pages_class_is_the_field_written()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Fill Ada's card");
        await Click("Fill Ben's card");
        await Expect(Slate.GetByText("Cards: Cy · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Rename the first card Bea");

        await Expect(Slate.GetByText("Cards: Bea · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_field_passed_alone_names_what_the_component_orders_by()
    {
        await Open("/Members/Data");
        await Click("Fill Cy's card");
        await Click("Fill Ada's card");
        await Click("Fill Ben's card");
        await Expect(Slate.GetByText("Cards: Cy · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Sort the cards by name");
        await Expect(Slate.GetByText("Cards: Ada · Ben · Cy", new() { Exact = true })).ToBeVisibleAsync();

        await Click("Sort the cards by due day");
        await Expect(Slate.GetByText("Cards: Ben · Cy · Ada", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task an_argument_the_declaration_always_passes_reaches_the_method()
    {
        await Open("/Members/Data");

        await Click("Add a row for Dot");

        await Expect(Slate.GetByRole(AriaRole.Listitem).Filter(new() { HasText = "Dot (by page)" })).ToHaveCountAsync(1);
    }

    [Test]
    public async Task a_value_passed_as_the_wider_type_the_method_declares_reaches_it()
    {
        await Open("/Members/Data");

        await Click("Summarize the rows as any list");

        await Expect(Slate.GetByText("Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();
    }
}
