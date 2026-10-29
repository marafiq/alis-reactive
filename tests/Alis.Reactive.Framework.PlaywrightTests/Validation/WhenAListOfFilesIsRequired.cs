namespace Alis.Reactive.Framework.PlaywrightTests.Validation;

/// <summary>
/// <c>NotEmpty</c> on a list of files, decided in the browser as the server decides it: a report with a photo
/// chosen is filed with it, one with none is held beside the photos, and the server's own rules refuse a
/// report with none with the same message.
/// </summary>
[TestFixture]
public sealed class WhenAListOfFilesIsRequired : AppPageTest
{
    private readonly List<string> _filings = new();

    // Every report that reached the filing endpoint, as the network carried it.
    [SetUp]
    public void WatchTheFilingEndpoint()
    {
        _filings.Clear();
        Page.Request += (_, request) =>
        {
            if (request.Method == "POST" && request.Url.EndsWith("/Validation/Incident", StringComparison.Ordinal))
                lock (_filings) _filings.Add(request.Method);
        };
    }

    private ILocator PhotoWanted =>
        Page.Locator("#incident-form > div")
            .Filter(new() { Has = Page.GetByLabel("Photos") })
            .GetByText("Attach a photo of the incident.", new() { Exact = true });

    private Task Click(string name) => Page.GetByRole(AriaRole.Button, new() { Name = name, Exact = true }).ClickAsync();

    [Test]
    public async Task a_report_with_a_photo_chosen_is_filed_with_it()
    {
        await Open("/Validation/Incident");
        await Page.GetByLabel("Photos").SetInputFilesAsync(new FilePayload
        {
            Name = "bruise.png",
            MimeType = "image/png",
            Buffer = [0x89, 0x50, 0x4E, 0x47],
        });

        await Click("File the report");

        await Expect(Page.Locator("#filed")).ToHaveTextAsync("filed with bruise.png");
    }

    [Test]
    public async Task a_report_with_no_photo_is_held_beside_the_photos()
    {
        await Open("/Validation/Incident");

        await Click("File the report");

        await Expect(PhotoWanted).ToBeVisibleAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        lock (_filings) Assert.That(_filings, Is.Empty, "the browser held the report");
    }

    [Test]
    public async Task the_server_refuses_a_report_with_no_photo_with_the_same_message()
    {
        await Open("/Validation/Incident");

        await Click("Ask the desk to file it");

        await Expect(PhotoWanted).ToBeVisibleAsync();
        lock (_filings) Assert.That(_filings, Is.EqualTo(new[] { "POST" }), "the server decided it");
    }
}
