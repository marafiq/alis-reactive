using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepReadsAMemberTheComponentLacks(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.Cells.absence_member)]
    public async Task the_read_stops_the_pipeline()
    {
        var older = await NewWorld();
        await older.Seed(Seed.TheComponentsVersion(ComponentVersion.Older));
        await older.Page.GotoAsync(BenchPages.AbsenceMember);
        await older.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(older.Page.Locator("#shown")).ToHaveTextAsync("");
        await Expect(older.Page.Locator("#done")).ToHaveTextAsync("");

        var current = await NewWorld();
        await current.Page.GotoAsync(BenchPages.AbsenceMember);
        await current.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show", Exact = true }).ClickAsync();
        await Expect(current.Page.Locator("#shown")).ToHaveTextAsync("RO-2026-11-10");
        await Expect(current.Page.Locator("#done")).ToHaveTextAsync("after");
    }
}
