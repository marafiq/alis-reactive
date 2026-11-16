using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenATextKeyIsOrderedUnderTheViewsCulture(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_27_0)]
    public async Task the_names_order_as_dotnet_orders_them_under_the_culture_the_view_renders_under()
    {
        var turkish = await NewWorld();
        await turkish.Seed(Seed.Carer("DR İNCE").AndCarer("Dr Işık"));
        await turkish.Page.GotoAsync(CarePages.CarersOnDutyInTurkish);
        await Expect(turkish.Page.Locator("#rota-status")).ToHaveTextAsync("Sorted");
        await Expect(turkish.Page.Locator("#first-carer")).ToHaveTextAsync("Dr Işık");

        var english = await NewWorld();
        await english.Seed(Seed.Carer("DR İNCE").AndCarer("Dr Işık"));
        await english.Page.GotoAsync(CarePages.CarersOnDutyInEnglish);
        await Expect(english.Page.Locator("#rota-status")).ToHaveTextAsync("Sorted");
        await Expect(english.Page.Locator("#first-carer")).ToHaveTextAsync("DR İNCE");

        var hawaiian = await NewWorld();
        await hawaiian.Seed(Seed.Carer("DR İNCE").AndCarer("Dr Işık"));
        await hawaiian.Page.GotoAsync(CarePages.CarersOnDutyInHawaiian);
        await Expect(hawaiian.Page.Locator("#carers-count")).ToHaveTextAsync("2");
        await Expect(hawaiian.Page.Locator("#first-carer")).ToHaveTextAsync("-");
        await Expect(hawaiian.Page.Locator("#rota-status")).ToHaveTextAsync("Waiting");
    }
}
