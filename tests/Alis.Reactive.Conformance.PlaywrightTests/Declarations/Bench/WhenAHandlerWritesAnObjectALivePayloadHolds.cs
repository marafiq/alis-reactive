using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandlerWritesAnObjectALivePayloadHolds(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F2.X5_10_0)]
    public async Task the_write_and_the_call_reach_the_components_own_object()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsPayloadOuter);
        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: Gamma");
        await Expect(bench.Page.GetByText("Outer acted", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();

        await Expect(bench.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Outer", Exact = true })).ToHaveTextAsync("Outer: seen at handover");
        await Expect(bench.Page.GetByText("Outer acted", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
