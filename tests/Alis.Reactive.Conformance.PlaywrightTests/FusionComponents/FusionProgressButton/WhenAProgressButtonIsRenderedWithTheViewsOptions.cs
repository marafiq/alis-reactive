using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAProgressButtonIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F35.M21_X1021_0)]
    public async Task the_button_shows_the_views_content_and_a_step_reads_it_at_its_id()
    {
        var report = await NewWorld();
        await report.Page.GotoAsync(CarePages.ReportSendingReport);
        await Expect(report.Page.Locator("#send-report")).ToHaveTextAsync("Send the report");
        await report.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show label", Exact = true }).ClickAsync();

        await Expect(report.Page.Locator("#report-label")).ToHaveTextAsync("Send the report");

        var nightReport = await NewWorld();
        await nightReport.Page.GotoAsync(CarePages.ReportSendingNightReport);
        await Expect(nightReport.Page.Locator("#send-report")).ToHaveTextAsync("Send the night report");
        await nightReport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show label", Exact = true }).ClickAsync();

        await Expect(nightReport.Page.Locator("#report-label")).ToHaveTextAsync("Send the night report");
    }
}
