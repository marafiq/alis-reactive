namespace Alis.Reactive.PlaywrightTests.Components.Fusion.Breadcrumb;

[TestFixture]
public class WhenUsingFusionBreadcrumb : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/CareRecordBreadcrumb";
    private const string TrailId = "care-record-trail";

    private FusionBreadcrumbLocator Trail => new(Page, TrailId);

    private ILocator ViewingNow => Page.Locator("#viewing-now");
    private ILocator OpenSectionPanel => Page.Locator("#open-section-panel");
    private ILocator OpenSectionHeading => Page.Locator("#open-section-heading");
    private ILocator OpenSectionIcon => Page.Locator("#open-section-icon");
    private ILocator OpenSectionSummary => Page.Locator("#open-section-summary");
    private ILocator OpenSectionCode => Page.Locator("#open-section-code");
    private ILocator BackToResident => Page.Locator("#back-to-resident");

    private async Task OpenCareRecord()
    {
        await NavigateToAndWaitForBoot(Path);
        await Expect(Trail.Link("Sunrise Court")).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    [Test]
    public async Task the_care_record_opens_showing_the_full_trail_to_the_care_plan()
    {
        await OpenCareRecord();

        await Expect(Trail.Link("Sunrise Court")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Trail.Link("Eleanor Hughes")).ToBeVisibleAsync(new() { Timeout = 10000 });
        await Expect(Trail.CurrentItem).ToHaveTextAsync("Care Plan");
        await Expect(ViewingNow)
            .ToHaveTextAsync("You are in this resident's Care Plan. Use the trail to step back up the record.");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task stepping_up_to_the_resident_opens_that_section_of_the_record()
    {
        await OpenCareRecord();

        await Trail.ClickLink("Eleanor Hughes");

        await Expect(OpenSectionHeading).ToHaveTextAsync("Eleanor Hughes", new() { Timeout = 10000 });
        await Expect(OpenSectionPanel).ToBeVisibleAsync(new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_opened_section_is_tagged_with_its_record_icon()
    {
        await OpenCareRecord();

        await Trail.ClickLink("Eleanor Hughes");

        await Expect(OpenSectionIcon).ToHaveTextAsync("e-icons e-user", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_opened_section_loads_the_summary_for_that_records_url()
    {
        await OpenCareRecord();

        await Trail.ClickLink("Sunrise Court");

        await Expect(OpenSectionSummary)
            .ToHaveTextAsync("Sunrise Court is home to 84 residents across 3 neighborhoods.",
                new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task the_opened_section_shows_the_record_code_for_that_crumb_id()
    {
        await OpenCareRecord();

        await Trail.ClickLink("Eleanor Hughes");

        await Expect(OpenSectionCode).ToHaveTextAsync("RES-214", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task opening_a_section_posts_the_clicked_crumb_to_the_server()
    {
        await OpenCareRecord();

        var requestTask = Page.WaitForRequestAsync(request =>
            request.Url.Contains("/Sandbox/Components/CareRecordBreadcrumb/Open") && request.Method == "POST",
            new() { Timeout = 10000 });

        await Trail.ClickLink("Eleanor Hughes");

        var request = await requestTask;
        var body = request.PostData ?? "";

        Assert.That(body, Does.Contain("\"Text\":\"Eleanor Hughes\""),
            "the gather pipeline must carry the clicked crumb's Item.Text under its declared key");
        Assert.That(body, Does.Contain("\"Id\":\"resident\""),
            "the gather pipeline must carry the clicked crumb's Item.Id under its declared key");
        Assert.That(body, Does.Contain("\"Url\":\"/residents/eleanor-hughes\""),
            "the gather pipeline must carry the clicked crumb's Item.Url under its declared key");
        Assert.That(body, Does.Contain("\"Disabled\":false"),
            "the gather pipeline must carry the clicked crumb's Item.Disabled under its declared key");

        AssertNoConsoleErrors();
    }

    [Test]
    public async Task returning_to_the_resident_overview_moves_the_current_crumb()
    {
        await OpenCareRecord();

        await Expect(Trail.CurrentItem).ToHaveTextAsync("Care Plan");

        await BackToResident.ClickAsync();

        await Expect(Trail.CurrentItem).ToHaveTextAsync("Eleanor Hughes", new() { Timeout = 10000 });

        AssertNoConsoleErrors();
    }
}
