namespace Alis.Reactive.Framework.PlaywrightTests.Members;

/// <summary>
/// A component's methods, called by a page's pipelines: with no argument (M1), with up to four (M2), with a
/// record (M3), for a result read where any value is read (M4), asynchronously (M5), and refusing (M6). A
/// method declared plain whose JavaScript returns a promise runs without the pipeline waiting for it (M11).
/// </summary>
[TestFixture]
public sealed class WhenAPageCallsAComponentsMethods : AppPageTest
{
    private ILocator Slate => Page.Locator("#slate");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    [Test]
    public async Task a_method_with_no_argument_does_what_it_names()
    {
        await Open("/Members/Methods");
        await Expect(Slate.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(2);

        await Button("Clear").ClickAsync();

        await Expect(Slate.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }

    [Test]
    public async Task four_arguments_arrive_each_as_the_type_its_method_declares()
    {
        await Open("/Members/Methods");

        await Button("Place a line").ClickAsync();

        await Expect(Slate.GetByText("Ada · 40.5 · yes · 2026-10-27")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_record_argument_arrives_as_the_record_its_method_declares_with_its_date_a_date()
    {
        await Open("/Members/Methods");

        await Button("Fill a card").ClickAsync();

        await Expect(Slate.GetByText("Ben due Tuesday 2026-10-27")).ToBeVisibleAsync();
    }

    [Test]
    public async Task a_method_result_decides_which_branch_runs()
    {
        await Open("/Members/Methods");

        await Button("Is Ada on the slate?").ClickAsync();
        await Expect(Page.Locator("#branch")).ToHaveTextAsync("Ada is on the slate");

        await Button("Is Zed on the slate?").ClickAsync();
        await Expect(Page.Locator("#branch")).ToHaveTextAsync("Zed is not on the slate");
    }

    [Test]
    public async Task a_method_result_fills_a_value_as_it_stands_when_the_pipeline_runs()
    {
        await Open("/Members/Methods");

        await Button("Read the total").ClickAsync();
        await Expect(Page.Locator("#total")).ToHaveTextAsync("0");

        await Button("Place a line").ClickAsync();
        await Button("Read the total").ClickAsync();
        await Expect(Page.Locator("#total")).ToHaveTextAsync("40.5");
    }

    [Test]
    public async Task an_async_method_is_finished_before_the_next_step_reads_what_it_did()
    {
        await Open("/Members/Methods");

        await Button("Save").ClickAsync();

        await Expect(Page.Locator("#save-status")).ToHaveTextAsync("saved");
    }

    [Test]
    public async Task a_promise_from_a_method_declared_plain_is_not_waited_for_by_the_next_step()
    {
        await Open("/Members/Methods");

        await Button("Start saving and go on").ClickAsync();

        await Expect(Page.Locator("#went-on-status")).ToHaveTextAsync("saving");
    }

    [Test]
    public async Task an_async_result_is_the_value_its_promise_settles_with()
    {
        await Open("/Members/Methods");

        await Button("Look up Ben").ClickAsync();

        await Expect(Page.Locator("#lookup")).ToHaveTextAsync("Ben is row 2");
    }

    [Test]
    public async Task a_method_that_throws_stops_the_rest_of_its_pipeline_and_the_page_goes_on()
    {
        await Open("/Members/Methods");

        await Button("Call the refusing method").ClickAsync();
        await Expect(Page.Locator("#before-throw")).ToHaveTextAsync("ran");

        await Button("Read the total").ClickAsync();
        await Expect(Page.Locator("#total")).ToHaveTextAsync("0");
        await Expect(Page.Locator("#after-throw")).ToHaveTextAsync("");
    }

    [Test]
    public async Task a_method_that_rejects_stops_the_rest_of_its_pipeline_and_the_page_goes_on()
    {
        await Open("/Members/Methods");

        await Button("Call the rejecting method").ClickAsync();
        await Expect(Page.Locator("#before-reject")).ToHaveTextAsync("ran");

        await Button("Look up Ben").ClickAsync();
        await Expect(Page.Locator("#lookup")).ToHaveTextAsync("Ben is row 2");
        await Expect(Page.Locator("#after-reject")).ToHaveTextAsync("");
    }
}
