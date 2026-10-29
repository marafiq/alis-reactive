namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// <c>Empty</c> on a list of files, decided in the browser as the server decides it: a report with no photo
/// chosen is filed, one with a photo is held beside the photos, and the server's own rules return a report
/// with a photo with the same message.
/// </summary>
[TestFixture]
public sealed class WhenAListOfFilesMustBeEmpty : AppPageTest
{
    private readonly List<string> _filings = new();

    // Every report that reached the filing endpoint, as the network carried it.
    [SetUp]
    public void WatchTheFilingEndpoint()
    {
        _filings.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Unphotographed", StringComparison.Ordinal))
                lock (_filings) _filings.Add(request.Method);
        };
    }

    private ILocator PhotosRefused =>
        Page.Locator("#unphotographed-form > div")
            .Filter(new() { Has = Page.GetByLabel("Photos") })
            .GetByText("This resident declined photographs; attach none.", new() { Exact = true });

    private ILocator ReportStatus => Page.Locator("#report-status");

    private Task ChooseAPhoto() =>
        Page.GetByLabel("Photos").SetInputFilesAsync(new FilePayload
        {
            Name = "bruise.png",
            MimeType = "image/png",
            Buffer = [0x89, 0x50, 0x4E, 0x47],
        });

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_report_with_no_photo_chosen_is_filed()
    {
        await Open("/Validation/Unphotographed");

        await Click("File the report");

        await Expect(ReportStatus).ToHaveTextAsync("filed without photos");
    }

    [Test]
    public async Task a_report_with_a_photo_chosen_is_held_beside_the_photos()
    {
        await Open("/Validation/Unphotographed");
        await ChooseAPhoto();

        await Click("File the report");

        await Expect(PhotosRefused).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_filings) Assert.That(_filings, Is.Empty, "the browser held the report");
    }

    [Test]
    public async Task the_server_returns_a_report_with_a_photo_with_the_same_message()
    {
        await Open("/Validation/Unphotographed");
        await ChooseAPhoto();

        await Click("Ask the desk to file it");

        await Expect(ReportStatus).ToHaveTextAsync("returned by the desk");
        await Expect(PhotosRefused).ToBeVisibleAsync();
    }
}
