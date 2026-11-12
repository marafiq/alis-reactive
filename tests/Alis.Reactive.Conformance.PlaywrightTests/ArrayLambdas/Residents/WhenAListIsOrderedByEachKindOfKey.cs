using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListIsOrderedByEachKindOfKey(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_32_0)]
    public async Task each_key_orders_as_dotnet_orders_it_null_first_and_equal_keys_in_list_order()
    {
        var withAnUnprioritisedDose = await NewWorld();
        await withAnUnprioritisedDose.Page.GotoAsync(CarePages.DoseOrdersWithAnUnprioritisedDose);
        await Expect(withAnUnprioritisedDose.Page.Locator("#first-by-milligrams")).ToHaveTextAsync("B");
        await Expect(withAnUnprioritisedDose.Page.Locator("#first-by-priority")).ToHaveTextAsync("D");
        await Expect(withAnUnprioritisedDose.Page.Locator("#first-by-urgency")).ToHaveTextAsync("A");
        await Expect(withAnUnprioritisedDose.Page.Locator("#first-by-code")).ToHaveTextAsync("D");
        await Expect(withAnUnprioritisedDose.Page.Locator("#first-by-priority-descending")).ToHaveTextAsync("B");
        await Expect(withAnUnprioritisedDose.Page.Locator("#first-by-milligrams-descending")).ToHaveTextAsync("A");

        var withEveryDosePrioritised = await NewWorld();
        await withEveryDosePrioritised.Page.GotoAsync(CarePages.DoseOrdersWithEveryDosePrioritised);
        await Expect(withEveryDosePrioritised.Page.Locator("#first-by-priority")).ToHaveTextAsync("C");
    }
}
