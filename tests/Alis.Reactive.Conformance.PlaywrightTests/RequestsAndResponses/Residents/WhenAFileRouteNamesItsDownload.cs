using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFileRouteNamesItsDownload(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.M6_X3_0)]
    public async Task the_name_is_the_extended_filename_before_the_filename_and_the_paths_last_segment_with_neither()
    {
        var exports = await NewWorld();
        await exports.Page.GotoAsync(CarePages.ResidentExports);
        var resume = await exports.Page.RunAndWaitForDownloadAsync(() => exports.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Export", Exact = true }).ClickAsync());
        Assert.That(resume.SuggestedFilename, Is.EqualTo("Résumé.pdf"));
        var census = await exports.Page.RunAndWaitForDownloadAsync(() => exports.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Export census", Exact = true }).ClickAsync());
        Assert.That(census.SuggestedFilename, Is.EqualTo("census.csv"));

        var planned = await NewWorld();
        await planned.Page.GotoAsync(CarePages.ResidentExports);
        var plan = await planned.Page.RunAndWaitForDownloadAsync(() => planned.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Export plan", Exact = true }).ClickAsync());
        Assert.That(plan.SuggestedFilename, Is.EqualTo("Plan.pdf"));
    }
}
