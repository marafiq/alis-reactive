using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPagesUrlNamesAKeyInAnotherCase(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F2.M2_X1_0)]
    public async Task the_member_reads_the_keys_value_and_its_start_when_the_url_holds_no_key()
    {
        var medications = await NewWorld();
        await medications.Page.GotoAsync(CarePages.ChartAtTheMedicationsTab);
        await Expect(medications.Page.Locator("#open-tab")).ToHaveTextAsync("medications");

        var unkeyed = await NewWorld();
        await unkeyed.Page.GotoAsync(CarePages.Chart);
        await Expect(unkeyed.Page.Locator("#open-tab")).ToHaveTextAsync("vitals");
    }
}
