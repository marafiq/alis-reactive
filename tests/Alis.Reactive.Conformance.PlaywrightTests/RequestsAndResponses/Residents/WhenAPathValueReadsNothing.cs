using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPathValueReadsNothing(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_9_0)]
    public async Task no_request_is_sent_its_finally_runs_and_no_step_after_it_runs()
    {
        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.ResidentChartOfNoResident);
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show chart", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.Locator("#chart-status")).ToHaveTextAsync("Done");
        await Expect(unnamed.Page.Locator("#chart-log")).ToHaveTextAsync("-");

        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.ResidentChartOfAda);
        var sent = await named.Page.RunAndWaitForRequestAsync(() => named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show chart", Exact = true }).ClickAsync(), "**/care/residents/charts/**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/care/residents/charts/Ada"));
        await Expect(named.Page.Locator("#chart-log")).ToHaveTextAsync("After");
    }
}
