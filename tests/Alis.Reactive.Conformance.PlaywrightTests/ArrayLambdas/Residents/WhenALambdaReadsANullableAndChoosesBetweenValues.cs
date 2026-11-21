using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaReadsANullableAndChoosesBetweenValues(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_34_0)]
    public async Task has_value_value_the_coalesce_and_the_conditional_each_read_null_and_nothing_as_dotnet_does()
    {
        var withAnUnprioritisedDose = await NewWorld();
        await withAnUnprioritisedDose.Page.GotoAsync(CarePages.AssignedDosesWithAnUnprioritisedDose);
        await Expect(withAnUnprioritisedDose.Page.Locator("#prioritised-count")).ToHaveTextAsync("2");
        await Expect(withAnUnprioritisedDose.Page.Locator("#on-file-off-routine-count")).ToHaveTextAsync("1");
        await Expect(withAnUnprioritisedDose.Page.Locator("#off-routine-count")).ToHaveTextAsync("2");
        await Expect(withAnUnprioritisedDose.Page.Locator("#unassigned-count")).ToHaveTextAsync("1");
        await Expect(withAnUnprioritisedDose.Page.Locator("#no-ward-count")).ToHaveTextAsync("1");
        await Expect(withAnUnprioritisedDose.Page.Locator("#low-count")).ToHaveTextAsync("2");

        var withEveryDosePrioritised = await NewWorld();
        await withEveryDosePrioritised.Page.GotoAsync(CarePages.AssignedDosesWithEveryDosePrioritised);
        await Expect(withEveryDosePrioritised.Page.Locator("#on-file-off-routine-count")).ToHaveTextAsync("2");
        await Expect(withEveryDosePrioritised.Page.Locator("#low-count")).ToHaveTextAsync("1");
    }
}
