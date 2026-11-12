using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaMatchesAPattern(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_37_0)]
    public async Task each_pattern_matches_as_dotnet_reads_it_and_a_note_that_holds_null_matches_none()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.MealNotesOfTheRound);
        await Expect(round.Page.Locator("#ends-in-food-count")).ToHaveTextAsync("2");
        await Expect(round.Page.Locator("#priced-count")).ToHaveTextAsync("1");
        await Expect(round.Page.Locator("#pattern-count")).ToHaveTextAsync("2");
        await Expect(round.Page.Locator("#after-food-count")).ToHaveTextAsync("1");

        var withoutTheSingleLineBreak = await NewWorld();
        await withoutTheSingleLineBreak.Page.GotoAsync(CarePages.MealNotesWithoutTheSingleLineBreak);
        await Expect(withoutTheSingleLineBreak.Page.Locator("#ends-in-food-count")).ToHaveTextAsync("1");
        await Expect(withoutTheSingleLineBreak.Page.Locator("#pattern-count")).ToHaveTextAsync("1");
    }
}
