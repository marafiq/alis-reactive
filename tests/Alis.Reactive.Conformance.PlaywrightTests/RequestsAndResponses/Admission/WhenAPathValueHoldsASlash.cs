using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RequestsAndResponses;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPathValueHoldsASlash(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M6_F1.X9_10_0)]
    public async Task the_request_is_not_sent_and_its_finally_runs()
    {
        var desk = await NewWorld();
        await desk.Page.GotoAsync(CarePages.OpenRoom);
        await desk.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room code", Exact = true }).FillAsync("a/b");
        await desk.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open room", Exact = true }).ClickAsync();
        await Expect(desk.Page.Locator("#room-status")).ToHaveTextAsync("Done");
        await Expect(desk.Page.Locator("#room")).ToHaveTextAsync("-");

        var opened = await NewWorld();
        await opened.Page.GotoAsync(CarePages.OpenRoom);
        await opened.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Room code", Exact = true }).FillAsync("B12");
        var sent = await opened.Page.RunAndWaitForRequestAsync(() => opened.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open room", Exact = true }).ClickAsync(), "**/Rooms/*");
        Assert.That(sent.Method, Is.EqualTo("GET"));
        Assert.That(sent.Url, Does.EndWith("/Rooms/B12"));
        await Expect(opened.Page.Locator("#room")).ToHaveTextAsync("Room B12 is ready");
        await Expect(opened.Page.Locator("#room-status")).ToHaveTextAsync("Done");
    }
}
