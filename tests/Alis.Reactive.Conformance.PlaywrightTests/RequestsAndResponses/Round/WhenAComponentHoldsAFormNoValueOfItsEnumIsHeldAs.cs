using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentHoldsAFormNoValueOfItsEnumIsHeldAs(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_19_0)]
    public async Task nothing_is_sent_no_later_step_runs_and_the_console_holds_one_error()
    {
        var medium = await NewWorld();
        await medium.Page.GotoAsync(CarePages.StaffSearchLogged);
        await medium.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Hold medium", Exact = true }).ClickAsync();
        await medium.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find staff", Exact = true }).ClickAsync();
        await Expect(medium.Page.Locator("#search-log")).ToHaveTextAsync("-");
        Assert.That(medium.ConsoleErrors, Has.Count.EqualTo(1));

        var high = await NewWorld();
        await high.Page.GotoAsync(CarePages.StaffSearchLogged);
        await high.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "High", Exact = true }).ClickAsync();
        var sent = await high.Page.RunAndWaitForRequestAsync(() => high.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Find staff", Exact = true }).ClickAsync(), "**/care/round/staff-search/staff**");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/care/round/staff-search/staff?Choice=Maximum"));
        await Expect(high.Page.Locator("#search-log")).ToHaveTextAsync("Sent");
    }
}
