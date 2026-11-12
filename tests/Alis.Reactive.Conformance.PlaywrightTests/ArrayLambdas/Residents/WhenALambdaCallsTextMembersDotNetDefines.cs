using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaCallsTextMembersDotNetDefines(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_19_0)]
    public async Task each_member_reads_as_dotnet_reads_it_and_a_null_looked_for_stops_the_pipeline()
    {
        var lookedForNothing = await NewWorld();
        await lookedForNothing.Page.GotoAsync(CarePages.CareNotesLookedThroughForNothing);
        await Expect(lookedForNothing.Page.Locator("#one-character-count")).ToHaveTextAsync("1");
        await Expect(lookedForNothing.Page.Locator("#capitals-count")).ToHaveTextAsync("1");
        await Expect(lookedForNothing.Page.Locator("#small-letters-count")).ToHaveTextAsync("1");
        await Expect(lookedForNothing.Page.Locator("#no-text-count")).ToHaveTextAsync("1");
        await Expect(lookedForNothing.Page.Locator("#blank-count")).ToHaveTextAsync("2");
        await Expect(lookedForNothing.Page.Locator("#notes-status")).ToHaveTextAsync("Waiting");

        var lookedForALetter = await NewWorld();
        await lookedForALetter.Page.GotoAsync(CarePages.CareNotesLookedThroughForALetter);
        await Expect(lookedForALetter.Page.Locator("#notes-status")).ToHaveTextAsync("Looked");
    }
}
