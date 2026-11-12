using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPagesUrlHoldsATextItsMemberCannotTake(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F2.M2_X2_0)]
    public async Task the_member_reads_its_start_for_a_text_that_does_not_convert_and_for_a_key_the_url_lacks()
    {
        var asked = await NewWorld();
        await asked.Page.GotoAsync(CarePages.WardBedsAskedAsNoNumber);
        await Expect(asked.Page.Locator("#beds")).ToHaveTextAsync("2");
        await asked.Page.GotoAsync(CarePages.WardBedsAskedAsSeven);
        await Expect(asked.Page.Locator("#beds")).ToHaveTextAsync("7");

        var unasked = await NewWorld();
        await unasked.Page.GotoAsync(CarePages.WardBeds);
        await Expect(unasked.Page.Locator("#beds")).ToHaveTextAsync("2");
    }
}
