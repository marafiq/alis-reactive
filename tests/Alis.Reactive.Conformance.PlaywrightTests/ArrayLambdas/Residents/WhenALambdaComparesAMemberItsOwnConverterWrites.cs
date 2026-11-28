using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesAMemberItsOwnConverterWrites(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_44_0)]
    public async Task the_literal_is_compared_in_the_members_own_form_and_an_omitted_member_reads_its_start()
    {
        var withTwoDosesDue = await NewWorld();
        await withTwoDosesDue.Page.GotoAsync(CarePages.ChartedRoundWithTwoDosesDue);
        await Expect(withTwoDosesDue.Page.Locator("#given-count")).ToHaveTextAsync("3");
        await Expect(withTwoDosesDue.Page.Locator("#not-given-count")).ToHaveTextAsync("2");
        await Expect(withTwoDosesDue.Page.Locator("#due-count")).ToHaveTextAsync("2");
        await Expect(withTwoDosesDue.Page.Locator("#two-vial-count")).ToHaveTextAsync("1");

        var withTwoDosesHeld = await NewWorld();
        await withTwoDosesHeld.Page.GotoAsync(CarePages.ChartedRoundWithTwoDosesHeld);
        await Expect(withTwoDosesHeld.Page.Locator("#due-count")).ToHaveTextAsync("0");
        await Expect(withTwoDosesHeld.Page.Locator("#not-given-count")).ToHaveTextAsync("2");
    }
}
