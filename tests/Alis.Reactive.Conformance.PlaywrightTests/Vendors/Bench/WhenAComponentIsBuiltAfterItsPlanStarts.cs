using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAComponentIsBuiltAfterItsPlanStarts(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F2.M12_X4_0)]
    public async Task the_plans_other_handlers_run_and_the_component_built_late_is_not_heard()
    {
        var late = await NewWorld();
        await late.Page.GotoAsync(BenchPages.VendorsComponentBuiltLate);
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Say hello", Exact = true }).ClickAsync();
        await Expect(late.Page.Locator("#greeting")).ToHaveTextAsync("hello");
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Build the component", Exact = true }).ClickAsync();
        await late.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(late.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Raises", Exact = true })).ToHaveTextAsync("Raised 1");
        await Expect(late.Page.Locator("#raised-heard")).ToHaveTextAsync("");

        var mounted = await NewWorld();
        await mounted.Page.GotoAsync(BenchPages.VendorsComponentBuiltOnMount);
        await mounted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Raise", Exact = true }).ClickAsync();
        await Expect(mounted.Page.Locator("#raised-heard")).ToHaveTextAsync("raised");
    }
}
