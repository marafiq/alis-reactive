using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormSendsAHiddenInputBesideAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.http_beside_hidden)]
    public async Task the_form_data_holds_the_hidden_batch_and_the_value_the_component_holds()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.HttpBesideHidden);
        var sent = await bench.Page.RunAndWaitForRequestAsync(() => bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync(), "**/notes/batch");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("B-2026-11-10: a-1");

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.HttpBesideHidden);
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Load", Exact = true }).ClickAsync();
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("B-2026-11-10: a-2");
    }
}
