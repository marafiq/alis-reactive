using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.PlaywrightTests.Components.AppLevel;

/// <summary>
/// As a nurse, I want a report I filed from the drawer to be filed even if I close the drawer while
/// it is still filing, so that I can move on and file the next report without reloading the page.
/// </summary>
/// <remarks>
/// Each filing request is held at the browser until the test releases it, then sent on unchanged to
/// the real server. What the page shows while a report is filing is then checked before the answer
/// can arrive, however fast the server is.
/// </remarks>
[TestFixture]
public class WhenNurseFilesIncidentReport : PlaywrightTestBase
{
    private const string Path = "/Sandbox/Components/Drawer/IncidentReport";
    private const string FilingUrl = "**/Sandbox/Components/Drawer/IncidentReport/File";

    private ConcurrentQueue<string> _reportsFiled = null!;
    private SemaphoreSlim _filingsReleased = null!;

    private ILocator Drawer => Page.Locator("#alis-drawer");
    private ILocator OpenIncidentReport => Page.Locator("#open-incident-report-btn");
    private ILocator Form => Page.Locator("#incident-report-form");
    private ILocator Summary => Page.Locator($"#{IdGenerator.For<IncidentReportModel>(m => m.Summary)}");
    private ILocator SummaryError => Page.Locator("[data-valmsg-for='Summary']");
    private ILocator FileReport => Page.Locator("#file-incident-report-btn");
    private ILocator IncidentStatus => Page.Locator("#incident-status");
    private ILocator Loader => Page.Locator("#alis-loader");
    private ILocator LoaderOverForm => Form.Locator("#alis-loader");

    // The drawer drops this class the moment it starts closing, while its slide-out keeps it
    // painted for a moment, so whether it is open is read from the class rather than from paint.
    private static readonly Regex DrawerOpen = new("alis-drawer--visible");

    private async Task NavigateAndBoot()
    {
        _reportsFiled = new ConcurrentQueue<string>();
        _filingsReleased = new SemaphoreSlim(0);
        Page.Request += (_, request) =>
        {
            if (request.Url.Contains("/Sandbox/Components/Drawer/IncidentReport/File")) _reportsFiled.Enqueue(request.Url);
        };
        await Page.RouteAsync(FilingUrl, async route =>
        {
            await _filingsReleased.WaitAsync();
            await route.ContinueAsync();
        });
        await NavigateTo(Path);
        await WaitForTraceMessage("booted", 10000);
    }

    private void ReleaseTheFiling() => _filingsReleased.Release();

    private async Task OpenTheDrawerAndWrite(string summary)
    {
        await ClickWhenStable(OpenIncidentReport);
        await Expect(Summary).ToBeVisibleAsync();
        await Summary.FillAsync(summary);
    }

    // The drawer lets go of the form once its slide-out ends; until then the loader is still inside it.
    private async Task CloseTheDrawer()
    {
        await Page.Keyboard.PressAsync("Escape");
        await Expect(Drawer).Not.ToHaveClassAsync(DrawerOpen);
        await Expect(Form).Not.ToBeAttachedAsync();
    }

    // A loader taken off the page also counts as hidden, so it must be attached as well.
    private async Task ExpectTheLoaderOnThePageAndHidden()
    {
        await Expect(Loader).ToBeAttachedAsync();
        await Expect(Loader).ToBeHiddenAsync();
    }

    [Test]
    public async Task closing_the_drawer_while_a_report_files_uncovers_the_page_and_still_files_it()
    {
        await NavigateAndBoot();
        await OpenTheDrawerAndWrite("Fall in the second-floor hallway.");
        await ClickWhenStable(FileReport);

        await CloseTheDrawer();

        await ExpectTheLoaderOnThePageAndHidden();
        await Expect(IncidentStatus).ToHaveTextAsync("No report filed yet.");
        ReleaseTheFiling();
        await Expect(IncidentStatus).ToHaveTextAsync("Incident report filed: Fall in the second-floor hallway.");
        Assert.That(_reportsFiled, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task reports_keep_filing_with_the_form_covered_each_time_the_drawer_closes_mid_filing()
    {
        await NavigateAndBoot();
        await OpenTheDrawerAndWrite("Fall in the second-floor hallway.");
        await ClickWhenStable(FileReport);
        await CloseTheDrawer();
        ReleaseTheFiling();
        await Expect(IncidentStatus).ToHaveTextAsync("Incident report filed: Fall in the second-floor hallway.");

        await OpenTheDrawerAndWrite("Skin tear on the left forearm.");
        await ClickWhenStable(FileReport);

        await Expect(LoaderOverForm).ToBeVisibleAsync();
        await CloseTheDrawer();
        await ExpectTheLoaderOnThePageAndHidden();
        ReleaseTheFiling();
        await Expect(IncidentStatus).ToHaveTextAsync("Incident report filed: Skin tear on the left forearm.");
        Assert.That(_reportsFiled, Has.Count.EqualTo(2), "Both reports must be sent.");
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task filing_a_report_covers_the_form_and_uncovers_it_once_filed()
    {
        await NavigateAndBoot();
        await OpenTheDrawerAndWrite("Fall in the second-floor hallway.");

        await ClickWhenStable(FileReport);

        await Expect(LoaderOverForm).ToBeVisibleAsync();
        ReleaseTheFiling();
        await Expect(IncidentStatus).ToHaveTextAsync("Incident report filed: Fall in the second-floor hallway.");
        await ExpectTheLoaderOnThePageAndHidden();
        Assert.That(_reportsFiled, Has.Count.EqualTo(1));
        AssertNoConsoleErrors();
    }

    [Test]
    public async Task an_empty_report_is_asked_for_and_files_once_written()
    {
        await NavigateAndBoot();
        await ClickWhenStable(OpenIncidentReport);
        await Expect(Summary).ToBeVisibleAsync();

        await ClickWhenStable(FileReport);

        await Expect(SummaryError).ToHaveTextAsync("Describe what happened.");
        await ExpectTheLoaderOnThePageAndHidden();

        await Summary.FillAsync("Fall in the second-floor hallway.");
        await ClickWhenStable(FileReport);
        ReleaseTheFiling();
        await Expect(IncidentStatus).ToHaveTextAsync("Incident report filed: Fall in the second-floor hallway.");
        Assert.That(_reportsFiled, Has.Count.EqualTo(1), "Only the written report may be sent.");
        AssertNoConsoleErrors();
    }
}
