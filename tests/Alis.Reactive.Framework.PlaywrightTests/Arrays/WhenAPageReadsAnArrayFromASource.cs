namespace Alis.Reactive.Framework.PlaywrightTests.Arrays;

/// <summary>
/// An array read from each source a page can give <c>p.From(..)</c>, and the arrays an element offers: a list
/// a response carries, a response that is an array, a sequence a response member holds, a list a response
/// does not hold, a component's list, a list a method returns, an element's children and its classes. The
/// residents endpoint answers Cyd Marsh, Ada Hartley, Ben Hart and Dot Ashton, in that order.
/// </summary>
[TestFixture]
public sealed class WhenAPageReadsAnArrayFromASource : AppPageTest
{
    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_list_a_response_carries_is_an_array_of_its_items_in_their_order()
    {
        await Open("/Arrays/Sources");

        await Button("Read the residents list").ClickAsync();

        await Expect(Page.Locator("#from-list")).ToHaveTextAsync("Cyd Marsh,Ada Hartley,Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task a_response_that_is_an_array_is_an_array_of_its_items_in_their_order()
    {
        await Open("/Arrays/Sources");

        await Button("Read the residents array").ClickAsync();

        await Expect(Page.Locator("#from-array")).ToHaveTextAsync("Cyd Marsh,Ada Hartley,Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task a_sequence_a_response_member_holds_is_an_array_of_its_items_in_their_order()
    {
        await Open("/Arrays/Sources");

        await Button("Read the census residents").ClickAsync();

        await Expect(Page.Locator("#from-sequence")).ToHaveTextAsync("Cyd Marsh,Ada Hartley,Ben Hart,Dot Ashton");
    }

    [Test]
    public async Task a_list_the_response_does_not_hold_is_an_empty_array()
    {
        await Open("/Arrays/Sources");

        await Button("Count the census visitors").ClickAsync();

        await Expect(Page.Locator("#visitor-count")).ToHaveTextAsync("0");
    }

    [Test]
    public async Task a_list_a_component_declares_is_read_as_it_stands_when_the_pipeline_runs()
    {
        await Open("/Arrays/Sources");

        await Button("Read the rows other than Ben").ClickAsync();
        await Expect(Page.Locator("#other-rows")).ToHaveTextAsync("Ada,Cyd");

        await Button("Add row").ClickAsync();
        await Button("Read the rows other than Ben").ClickAsync();
        await Expect(Page.Locator("#other-rows")).ToHaveTextAsync("Ada,Cyd,Row 4");
    }

    [Test]
    public async Task a_list_a_method_returns_is_read_when_the_pipeline_runs()
    {
        await Open("/Arrays/Sources");

        await Button("Mark the second row").ClickAsync();
        await Button("Read the marked rows").ClickAsync();

        await Expect(Page.Locator("#marked-rows")).ToHaveTextAsync("BEN");
    }

    [Test]
    public async Task an_elements_children_are_an_array_of_its_child_elements()
    {
        await Open("/Arrays/Sources");

        await Button("Count the seats").ClickAsync();

        await Expect(Page.Locator("#seat-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_childs_attribute_is_read_where_the_children_are_filtered_and_projected()
    {
        await Open("/Arrays/Sources");

        await Button("Read the seats under close supervision").ClickAsync();

        await Expect(Page.Locator("#close-seats")).ToHaveTextAsync("Ada Hartley,Cyd Marsh");
    }

    [Test]
    public async Task an_elements_classes_are_an_array_read_as_they_stand_when_the_pipeline_runs()
    {
        await Open("/Arrays/Sources");

        await Button("Read the wards on the badge").ClickAsync();
        await Expect(Page.Locator("#badge-wards")).ToHaveTextAsync("ward-north");

        await Button("Add the east ward to the badge").ClickAsync();
        await Button("Read the wards on the badge").ClickAsync();
        await Expect(Page.Locator("#badge-wards")).ToHaveTextAsync("ward-north,ward-east");
    }
}
