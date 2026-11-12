using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesATextBoxWithAComponentsMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.source_ConditionRight)]
    public async Task the_branch_follows_whether_the_box_holds_the_components_value()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.SourceConditionRight);
        await bench.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check" }).ClickAsync();
        await Expect(bench.Page.Locator("#shown")).ToHaveTextAsync("Same value");

        var retyped = await NewWorld();
        await retyped.Page.GotoAsync(BenchPages.SourceConditionRight);
        await retyped.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Typed", Exact = true }).FillAsync("a-2");
        await retyped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check" }).ClickAsync();
        await Expect(retyped.Page.Locator("#shown")).ToHaveTextAsync("Other value");
    }
}
