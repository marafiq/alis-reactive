using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Elements;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepCountsSeatsByTheirAttributes(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M5_F1.M5_X2_0)]
    public async Task an_attribute_a_seat_lacks_reads_null_and_an_attribute_with_no_value_is_held()
    {
        var table = await NewWorld();
        await table.Page.GotoAsync(CarePages.TableSupport);
        await Expect(table.Page.Locator("#close-supervision")).ToHaveTextAsync("2");
        await Expect(table.Page.Locator("#not-close")).ToHaveTextAsync("2");
        await Expect(table.Page.Locator("#wheelchair-seats")).ToHaveTextAsync("1");
        await Expect(table.Page.Locator("#hoist-seats")).ToHaveTextAsync("0");
    }
}
