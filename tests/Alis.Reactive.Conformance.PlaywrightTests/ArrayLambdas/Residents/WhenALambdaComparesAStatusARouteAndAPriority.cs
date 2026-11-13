using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALambdaComparesAStatusARouteAndAPriority(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_16_0)]
    public async Task each_count_keeps_the_doses_dotnet_keeps_whatever_form_the_app_writes_its_statuses_in()
    {
        var enumNames = await NewWorld();
        await enumNames.Page.GotoAsync(CarePages.RoundDoseCounts);
        await Expect(enumNames.Page.Locator("#due-count")).ToHaveTextAsync("2");
        await Expect(enumNames.Page.Locator("#injected-count")).ToHaveTextAsync("1");
        await Expect(enumNames.Page.Locator("#as-planned-count")).ToHaveTextAsync("2");
        await Expect(enumNames.Page.Locator("#above-priority-count")).ToHaveTextAsync("3");
        await Expect(enumNames.Page.Locator("#early-route-count")).ToHaveTextAsync("2");

        var enumsAsNumbers = await NewWorldUnder(Hosts.EnumsAsNumbersName);
        await enumsAsNumbers.Page.GotoAsync(CarePages.RoundDoseCounts);
        await Expect(enumsAsNumbers.Page.Locator("#due-count")).ToHaveTextAsync("2");
        await Expect(enumsAsNumbers.Page.Locator("#as-planned-count")).ToHaveTextAsync("2");
    }
}
