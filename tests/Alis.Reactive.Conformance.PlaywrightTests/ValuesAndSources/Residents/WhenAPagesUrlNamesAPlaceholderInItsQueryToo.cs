using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ValuesAndSources;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPagesUrlNamesAPlaceholderInItsQueryToo(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M2_F2.M2_X3_0)]
    public async Task a_placeholder_member_is_read_from_the_path_and_without_the_template_from_the_query()
    {
        var byTemplate = await NewWorld();
        await byTemplate.Page.GotoAsync(CarePages.TheEastWardAskedAsWest);
        await Expect(byTemplate.Page.Locator("#ward")).ToHaveTextAsync("East");

        var byQuery = await NewWorld();
        await byQuery.Page.GotoAsync(CarePages.TheEastWardAskedAsWest);
        await Expect(byQuery.Page.Locator("#queried-ward")).ToHaveTextAsync("West");
    }
}
