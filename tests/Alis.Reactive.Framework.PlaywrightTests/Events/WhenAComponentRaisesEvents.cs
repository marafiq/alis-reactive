namespace Alis.Reactive.Framework.PlaywrightTests.Events;

/// <summary>
/// The events a component raises, heard by a page's pipelines: without a payload (E1), with payload members
/// read (E2), written back before the component reads them (E3), and called (E4), and a nested object's
/// event (E5). Every event here is the slate's own: raised by its buttons or by what a method made it do.
/// </summary>
[TestFixture]
public sealed class WhenAComponentRaisesEvents : AppPageTest
{
    private ILocator Slate => Page.Locator("#slate");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task an_event_without_a_payload_runs_its_pipeline()
    {
        await Open("/Events/Slate");
        await Expect(Page.Locator("#cleared")).ToHaveTextAsync("");

        await Button("Clear").ClickAsync();

        await Expect(Page.Locator("#cleared")).ToHaveTextAsync("The slate was cleared");
    }

    [Test]
    public async Task a_pipeline_reads_the_members_of_the_payload_it_is_handed()
    {
        await Open("/Events/Slate");

        await Button("Add row").ClickAsync();

        await Expect(Page.Locator("#added-label")).ToHaveTextAsync("Row 3");
        await Expect(Page.Locator("#added-count")).ToHaveTextAsync("3");
    }

    [Test]
    public async Task a_payload_member_the_pipeline_writes_is_read_back_by_the_component()
    {
        await Open("/Events/Slate");

        await Button("Retitle as Night round").ClickAsync();

        await Expect(Slate.GetByText("Rounds keep their name")).ToBeVisibleAsync();
        await Expect(Slate.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Day round");
    }

    [Test]
    public async Task a_payload_method_the_pipeline_calls_acts_before_the_component_reads_the_payload()
    {
        await Open("/Events/Slate");

        await Button("Retitle as Afternoon extended round").ClickAsync();

        await Expect(Slate.GetByRole(AriaRole.Heading)).ToHaveTextAsync("Afternoon");
        await Expect(Slate.GetByText("Rounds keep their name")).ToHaveCountAsync(0);
    }

    [Test]
    public async Task an_event_of_a_nested_object_runs_its_pipeline_with_its_payload()
    {
        await Open("/Events/Slate");

        await Button("Rename the header").ClickAsync();

        await Expect(Page.Locator("#renamed")).ToHaveTextAsync("Evening round");
    }
}
