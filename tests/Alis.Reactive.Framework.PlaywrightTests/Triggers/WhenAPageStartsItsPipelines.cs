namespace Alis.Reactive.Framework.PlaywrightTests.Triggers;

/// <summary>
/// What starts a page's pipelines: the page being ready, once (D1), read with the URL it was opened with (D3),
/// and the app's own events, raised synchronously (D2) with the payload a page composes, or by a click on
/// markup that carries the event's name and its payload (D4).
/// </summary>
[TestFixture]
public sealed class WhenAPageStartsItsPipelines : AppPageTest
{
    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_ready_pipeline_reads_the_query_string_the_way_the_endpoint_binds_it()
    {
        await Open("/Triggers/Arrival?Resident=Ada&wing=East&WING=West&wings=East&wings=West&floor=3");

        await Expect(Line("resident")).ToHaveTextAsync("Ada");
        await Expect(Line("wing")).ToHaveTextAsync("East");
        await Expect(Page.Locator("#slate").GetByText("East · West")).ToBeVisibleAsync();
        await Expect(Line("floor")).ToHaveTextAsync("upstairs");
        await Expect(Line("visitor")).ToHaveTextAsync("no visitor");
    }

    [Test]
    public async Task an_absent_list_key_reads_as_empty_and_a_lower_number_as_the_number_it_is()
    {
        await Open("/Triggers/Arrival?resident=Ben&floor=1&visitor=Tom");

        await Expect(Line("resident")).ToHaveTextAsync("Ben");
        await Expect(Page.Locator("#slate").GetByText("no wings")).ToBeVisibleAsync();
        await Expect(Line("floor")).ToHaveTextAsync("downstairs");
        await Expect(Line("visitor")).ToHaveTextAsync("Tom");
    }

    [Test]
    public async Task a_ready_pipeline_runs_once_for_its_page_and_once_for_each_partial_that_loads()
    {
        await Open("/Triggers/Arrival?resident=Ada");

        await Button("Count the ready handlers").ClickAsync();
        await Expect(Line("ready-count")).ToHaveTextAsync("1");

        await Button("Load a visit card").ClickAsync();
        await Expect(Page.Locator("#visit-slot").GetByRole(AriaRole.Heading)).ToBeVisibleAsync();
        await Button("Load a visit card").ClickAsync();
        await Expect(Page.Locator("#visit-slot").GetByRole(AriaRole.Heading)).ToBeVisibleAsync();
        await Button("Count the ready handlers").ClickAsync();
        await Expect(Line("ready-count")).ToHaveTextAsync("21");
    }

    [Test]
    public async Task an_announced_event_is_heard_before_the_next_step_of_the_pipeline_that_announced_it()
    {
        await Open("/Triggers/Announcements");

        await Button("Announce the round").ClickAsync();

        await Expect(Line("round")).ToHaveTextAsync("the round started");
        await Expect(Line("after-announcing")).ToHaveTextAsync("Round started");
    }

    [Test]
    public async Task an_announced_payload_written_in_the_view_reaches_the_listener()
    {
        await Open("/Triggers/Announcements");

        await Button("Announce Ada's admission").ClickAsync();

        await Expect(Line("admitted-name")).ToHaveTextAsync("Ada");
        await Expect(Line("admitted-room")).ToHaveTextAsync("12");
    }

    [Test]
    public async Task an_announced_payload_composed_of_values_read_when_it_is_announced_reaches_the_listener()
    {
        await Open("/Triggers/Announcements");
        await Button("Add 4 to the tally").ClickAsync();

        await Button("Announce the slate's admission").ClickAsync();

        await Expect(Line("admitted-name")).ToHaveTextAsync("Night");
        await Expect(Line("admitted-room")).ToHaveTextAsync("4");
    }

    [Test]
    public async Task an_announced_payload_composed_member_by_member_has_its_nested_members_and_its_list()
    {
        await Open("/Triggers/Announcements");

        await Button("Announce a move").ClickAsync();

        await Expect(Line("moved-from")).ToHaveTextAsync("12");
        await Expect(Line("moved-who")).ToHaveTextAsync("Ada");
    }

    [Test]
    public async Task an_announced_payloads_array_member_is_set_and_read_by_position()
    {
        await Open("/Triggers/Announcements");

        await Button("Announce a move").ClickAsync();

        await Expect(Line("moved-escort")).ToHaveTextAsync("Bea");
    }

    [Test]
    public async Task markup_carrying_an_events_name_raises_it_with_the_payload_it_carries_when_clicked()
    {
        await Open("/Triggers/Announcements");

        await Button("Admit Bea from the board").ClickAsync();

        await Expect(Line("admitted-name")).ToHaveTextAsync("Bea");
        await Expect(Line("admitted-room")).ToHaveTextAsync("4");
    }
}
