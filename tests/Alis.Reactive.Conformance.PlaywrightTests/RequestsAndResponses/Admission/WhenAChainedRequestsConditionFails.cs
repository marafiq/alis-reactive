using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAChainedRequestsConditionFails(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F2.X10_11_0)]
    public async Task the_chained_request_is_not_sent_and_the_steps_after_the_first_request_run()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.BadgeDesk);
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#admit-status")).ToHaveTextAsync("Admitted");
        await Expect(desk.Page.Locator("#admit-log")).ToHaveTextAsync("After");
        await Expect(desk.Page.Locator("#badge")).ToHaveTextAsync("No badge printed");

        var printed = await NewWorld();
        await printed.Page.GotoAsync(CarePages.BadgeDesk);
        await printed.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Print badge", Exact = true }).CheckAsync();
        var badge = await printed.Page.RunAndWaitForRequestAsync(() => printed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Admit", Exact = true }).ClickAsync(), "**/admission/badge-desk/badge");
        Assert.That(badge.Method, Is.EqualTo("GET"));
        await Expect(printed.Page.Locator("#badge")).ToHaveTextAsync("Badge for Ada Hart");
        await Expect(printed.Page.Locator("#admit-log")).ToHaveTextAsync("After");
    }
}
