namespace Alis.Reactive.Framework.PlaywrightTests.Requests;

/// <summary>
/// A file the person chose, as the browser hands it to a page (<c>BrowserFile</c>): what the page reads of it,
/// posted for an endpoint to check, and the file itself, which a form-data request sends.
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
}
