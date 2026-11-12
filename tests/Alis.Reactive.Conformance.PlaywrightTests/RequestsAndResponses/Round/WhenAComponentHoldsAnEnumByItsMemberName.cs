using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentHoldsAnEnumByItsMemberName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_18_0)]
    public async Task the_held_form_sets_the_request_member_and_the_endpoint_reads_its_value()
    {
        var high = await NewWorld();
        await high.Page.GotoAsync(CarePages.StaffSearch);
        await high.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "High", Exact = true }).ClickAsync();
        var maximum = await high.Page.RunAndWaitForRequestAsync(() => high.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find staff", Exact = true }).ClickAsync(), "**/care/round/staff-search/staff**");
        Assert.That(maximum.Method, Is.EqualTo("GET"));
        Assert.That(maximum.Url, Does.EndWith("/care/round/staff-search/staff?Choice=Maximum"));
        await Expect(high.Page.Locator("#staffing-found")).ToHaveTextAsync("Maximum");

        var low = await NewWorld();
        await low.Page.GotoAsync(CarePages.StaffSearch);
        await low.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Low", Exact = true }).ClickAsync();
        var minimum = await low.Page.RunAndWaitForRequestAsync(() => low.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find staff", Exact = true }).ClickAsync(), "**/care/round/staff-search/staff**");
        Assert.That(minimum.Method, Is.EqualTo("GET"));
        Assert.That(minimum.Url, Does.EndWith("/care/round/staff-search/staff?Choice=Minimum"));
    }
}
