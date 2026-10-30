namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// The objects a component holds and returns, reached by a page's pipelines: a nested object's members
/// (O1), an object a method returns (O2), a list of objects (O3), and a list a method returns (O4).
/// </summary>
[TestFixture]
public sealed class WhenAPageUsesAComponentsObjects : AppPageTest
{
    private ILocator Slate => Page.Locator("#slate");

    private ILocator Rows => Slate.GetByRole(AriaRole.Listitem);

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_nested_object_takes_a_call_and_a_write_and_reads_back_what_it_holds()
    {
        await Open("/Members/Objects");

        await Button("Rename the header").ClickAsync();
        await Expect(Slate.GetByText("Evening round")).ToBeVisibleAsync();

        await Button("Set the caption").ClickAsync();
        await Expect(Slate.GetByText("Late round")).ToBeVisibleAsync();
        await Expect(Slate.GetByText("Evening round")).ToHaveCountAsync(0);

        await Button("Read the caption").ClickAsync();
        await Expect(Page.Locator("#caption-read")).ToHaveTextAsync("Late round");
    }

    [Test]
    public async Task an_object_a_method_returns_is_the_one_its_members_act_on()
    {
        await Open("/Members/Objects");
        await Expect(Rows).ToHaveTextAsync(["Ada", "Ben", "Cyd"]);

        await Button("Mark the second row").ClickAsync();
        await Expect(Rows).ToHaveTextAsync(["Ada", "Ben (marked)", "Cyd"]);

        await Button("Read the second row").ClickAsync();
        await Expect(Page.Locator("#second-row")).ToHaveTextAsync("Ben");
    }

    [Test]
    public async Task an_object_a_method_of_two_arguments_returns_is_the_one_its_members_act_on()
    {
        await Open("/Members/Objects");

        await Button("Mark the last row").ClickAsync();

        await Expect(Rows).ToHaveTextAsync(["Ada", "Ben", "Cyd (marked)"]);
    }

    [Test]
    public async Task an_object_declared_for_an_interface_the_declaration_implements_is_the_object_it_names()
    {
        await Open("/Members/Headed");

        await Button("Set the caption through the board").ClickAsync();

        await Expect(Slate.GetByText("Board round")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_list_of_objects_gives_an_item_by_position_and_a_field_of_every_item()
    {
        await Open("/Members/Objects");

        await Button("Read the third row").ClickAsync();
        await Expect(Page.Locator("#third-row")).ToHaveTextAsync("Cyd");

        await Button("Add row").ClickAsync();
        await Button("Summarize the rows").ClickAsync();
        await Expect(Slate.GetByText("Ada · Ben · Cyd · Row 4")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_list_a_method_returns_is_read_as_it_stands_when_the_pipeline_runs()
    {
        await Open("/Members/Objects");

        await Button("Count the marked rows").ClickAsync();
        await Expect(Page.Locator("#marked-count")).ToHaveTextAsync("0");

        await Button("Mark the second row").ClickAsync();
        await Button("Count the marked rows").ClickAsync();
        await Expect(Page.Locator("#marked-count")).ToHaveTextAsync("1");
    }
}
