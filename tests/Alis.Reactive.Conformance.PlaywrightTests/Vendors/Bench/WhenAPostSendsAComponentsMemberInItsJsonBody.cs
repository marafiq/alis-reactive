using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPostSendsAComponentsMemberInItsJsonBody(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_post)]
    public async Task the_body_carries_the_value_the_component_holds_and_its_answer_is_shown()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpPost);
        var sent = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/items");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        Assert.That(sent.PostData, Is.EqualTo("{\"code\":\"a-1\"}"));
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Recorded a-1");

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.HttpPost);
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        var sentAgain = await picked.Page.RunAndWaitForRequestAsync(() => picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/items");
        Assert.That(sentAgain.PostData, Is.EqualTo("{\"code\":\"a-2\"}"));
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("Recorded a-2");
    }
}
