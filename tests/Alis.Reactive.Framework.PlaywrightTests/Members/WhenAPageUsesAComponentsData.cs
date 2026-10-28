namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// A component's data: a value it holds whose members are read (P5), one field of each item of a list it
/// holds (P6), a nullable property written from a value that is never missing (P7), a write-only property
/// written from a value read when the step runs (P8), a field of the page's own class passed by its
/// JavaScript name with a value of its type (M7) or alone (M8), an argument the declaration always passes
/// (M9), and a value passed as the wider type the method declares (M10). Proven on the slate: Cy's card is
/// due 2026-10-29, Ada's the 30th and Ben's the 27th.
/// </summary>
[TestFixture]
public sealed class WhenAPageUsesAComponentsData : AppPageTest
{
    private ILocator Slate => Page.Locator("#slate");

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    private async Task FillThreeCards()
    {
        await Click("Fill Cy's card");
        await Click("Fill Ada's card");
        await Click("Fill Ben's card");
        await Expect(Slate.GetByText("Cards: Cy · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();
    }

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
    public async Task one_field_of_each_item_of_a_list_of_data_is_read()
    {
        await Open("/Members/Data");
        await FillThreeCards();

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
        await FillThreeCards();

        await Click("Rename the first card Bea");

        await Expect(Slate.GetByText("Cards: Bea · Ada · Ben", new() { Exact = true })).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_field_passed_alone_names_what_the_component_orders_by()
    {
        await Open("/Members/Data");
        await FillThreeCards();

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
