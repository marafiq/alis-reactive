using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentsMemberFillsADispatchedPayload(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_DispatchPayloadMember)]
    public async Task the_events_handler_reads_the_value_the_component_held()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceDispatchPayloadMember);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tell", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("a-1");

        var picked = await NewWorld();
        await picked.Page.GotoAsync(BenchPages.SourceDispatchPayloadMember);
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await picked.Page.GetByRole(AriaRole.Listbox, new PageGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await picked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Tell", Exact = true }).ClickAsync();
        await Expect(picked.Page.Locator("#shown")).ToHaveTextAsync("a-2");
    }
}
