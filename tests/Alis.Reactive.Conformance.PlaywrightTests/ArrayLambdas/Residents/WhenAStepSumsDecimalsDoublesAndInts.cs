using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.ArrayLambdas;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAStepSumsDecimalsDoublesAndInts(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M4_F1.X6_9_0)]
    public async Task each_sum_reads_as_dotnet_sums_its_type_and_an_int_total_past_its_range_stops_the_pipeline()
    {
        var pastAnInt = await NewWorld();
        await pastAnInt.Page.GotoAsync(CarePages.BillingWithUnitsPastAnInt);
        await pastAnInt.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sum", Exact = true }).ClickAsync();
        await Expect(pastAnInt.Page.Locator("#charge-total")).ToHaveTextAsync("0.3");
        await Expect(pastAnInt.Page.Locator("#rate-total")).ToHaveTextAsync("0.30000000000000004");
        await Expect(pastAnInt.Page.Locator("#refund-total")).ToHaveTextAsync("0");
        await Expect(pastAnInt.Page.Locator("#unit-total")).ToHaveTextAsync("-");
        await Expect(pastAnInt.Page.Locator("#billing-status")).ToHaveTextAsync("Waiting");

        var withinAnInt = await NewWorld();
        await withinAnInt.Page.GotoAsync(CarePages.BillingWithUnitsWithinAnInt);
        await withinAnInt.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sum", Exact = true }).ClickAsync();
        await Expect(withinAnInt.Page.Locator("#unit-total")).ToHaveTextAsync("2147483600");
        await Expect(withinAnInt.Page.Locator("#billing-status")).ToHaveTextAsync("Summed");
    }
}
