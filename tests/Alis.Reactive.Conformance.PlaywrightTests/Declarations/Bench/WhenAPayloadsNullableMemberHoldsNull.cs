using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Declarations;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPayloadsNullableMemberHoldsNull(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M11_F2.X5_8_0)]
    public async Task the_text_it_sets_shows_nothing_and_the_handler_goes_on()
    {
        var bench = await NewWorld();
        await bench.Page.GotoAsync(BenchPages.DeclarationsNullablePayloadText);
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise Ben", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#raised-text")).ToHaveTextAsync("Ben Ng");
        await bench.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise Ada", Exact = true }).ClickAsync();
        await Expect(bench.Page.Locator("#raised-text")).ToHaveTextAsync("Ada Byron");
        await Expect(bench.Page.Locator("#nullable-text")).ToHaveTextAsync("");

        var ben = await NewWorld();
        await ben.Page.GotoAsync(BenchPages.DeclarationsNullablePayloadText);
        await ben.Page.Locator("#conformance").GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Raise Ben", Exact = true }).ClickAsync();
        await Expect(ben.Page.Locator("#nullable-text")).ToHaveTextAsync("Ireland");
        await Expect(ben.Page.Locator("#raised-text")).ToHaveTextAsync("Ben Ng");
    }
}
