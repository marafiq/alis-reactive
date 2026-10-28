namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A file the person chose, as the browser hands it to a page (<c>BrowserFile</c>): what the page reads of it,
/// posted for an endpoint to check, and the file itself, which a form-data request sends, from an array or from
/// the browser's own file list (H4); a query or a JSON body has no form for a file, so a request that would
/// carry one stops before it is sent (H2, H3).
/// </summary>
[TestFixture]
public sealed class WhenAPageSendsAFile : AppPageTest
{
    private const string CarePlanText = "Walk daily.";

    private static readonly DateTime CarePlanChanged = new(2026, 10, 1, 9, 30, 0, DateTimeKind.Utc);

    private ILocator Line(string id) => Page.Locator($"#{id}");

    private ILocator Button(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true });

    // The care plan as the person's computer keeps it: a text file of its own, last changed when it was.
    private static string CarePlanFile()
    {
        var folder = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "alis-care-plan-" + Guid.NewGuid().ToString("N")));
        var file = Path.Combine(folder.FullName, "plan.txt");
        File.WriteAllText(file, CarePlanText);
        File.SetLastWriteTimeUtc(file, CarePlanChanged);
        return file;
    }

    private async Task Choose(string file) => await Page.GetByLabel("Care plan").SetInputFilesAsync(file);

    [Test]
    public async Task a_page_reads_the_name_size_media_type_and_last_change_of_the_file_the_person_chose()
    {
        await Open("/Requests/CarePlan");
        await Choose(CarePlanFile());

        await Button("Check the care plan").ClickAsync();

        await Expect(Line("checked")).ToHaveTextAsync("plan.txt, 11 bytes, text/plain, changed 2026-10-01 09:30 UTC");
    }

    [Test]
    public async Task a_form_data_request_sends_the_file_itself()
    {
        await Open("/Requests/CarePlan");
        await Choose(CarePlanFile());

        await Button("Send the care plan").ClickAsync();

        await Expect(Line("received")).ToHaveTextAsync("received plan.txt, 11 bytes of text/plain: Walk daily.");
    }

    [Test]
    public async Task a_form_data_request_sends_the_files_of_the_browsers_own_file_list()
    {
        await Open("/Requests/CarePlan");
        await Choose(CarePlanFile());

        await Button("Send the care plan as the browser lists it").ClickAsync();

        await Expect(Line("received")).ToHaveTextAsync("received plan.txt, 11 bytes of text/plain: Walk daily.");
    }

    [Test]
    public async Task a_file_in_a_query_stops_its_request_and_the_steps_after_it()
    {
        await Open("/Requests/CarePlan");
        await Button("Look up the care plan by the address").ClickAsync();
        await Expect(Line("looked-up")).ToHaveTextAsync("looked up with query nothing");
        await Expect(Line("lookup-after")).ToHaveTextAsync("1");
        await Choose(CarePlanFile());

        await Button("Look up the care plan by the address").ClickAsync();

        await Expect(Line("lookup-tried")).ToHaveTextAsync("2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Line("lookup-after")).ToHaveTextAsync("1");
        await Button("Send the care plan").ClickAsync();
        await Expect(Line("received")).ToHaveTextAsync("received plan.txt, 11 bytes of text/plain: Walk daily.");
    }

    [Test]
    public async Task a_file_in_a_json_body_stops_its_request_and_the_steps_after_it()
    {
        await Open("/Requests/CarePlan");
        await Button("Post the care plan as JSON").ClickAsync();
        await Expect(Line("posted-json")).ToHaveTextAsync("posted as application/json");
        await Expect(Line("json-after")).ToHaveTextAsync("1");
        await Choose(CarePlanFile());

        await Button("Post the care plan as JSON").ClickAsync();

        await Expect(Line("json-tried")).ToHaveTextAsync("2");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Expect(Line("json-after")).ToHaveTextAsync("1");
        await Button("Send the care plan").ClickAsync();
        await Expect(Line("received")).ToHaveTextAsync("received plan.txt, 11 bytes of text/plain: Walk daily.");
    }
}
