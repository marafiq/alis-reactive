using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Conditions;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesADecimalItsOwnNumberHandlingWritesAsText(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M3_F1.X8_26_0)]
    public async Task the_copay_is_compared_with_the_decimal_as_a_number_by_eq_and_by_not_eq()
    {
        var cart = await NewWorld();
        await cart.Page.GotoAsync(CarePages.DoseCartWithACopayOfOneTenth);
        await cart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the price", Exact = true }).ClickAsync();
        await cart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the note", Exact = true }).ClickAsync();
        await Expect(cart.Page.Locator("#copay-price")).ToHaveTextAsync("Ten pence");
        await Expect(cart.Page.Locator("#copay-note")).ToHaveTextAsync("Same");
        await cart.Page.GotoAsync(CarePages.DoseCartWithACopayOfOneEighth);
        await cart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the price", Exact = true }).ClickAsync();
        await cart.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the note", Exact = true }).ClickAsync();
        await Expect(cart.Page.Locator("#copay-price")).ToHaveTextAsync("Other");
        await Expect(cart.Page.Locator("#copay-note")).ToHaveTextAsync("Differs");

        var toTwoPlaces = await NewWorld();
        await toTwoPlaces.Page.GotoAsync(CarePages.DoseCartWithACopayOfTenHundredths);
        await toTwoPlaces.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the price", Exact = true }).ClickAsync();
        await toTwoPlaces.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Check the note", Exact = true }).ClickAsync();
        await Expect(toTwoPlaces.Page.Locator("#copay-price")).ToHaveTextAsync("Ten pence");
        await Expect(toTwoPlaces.Page.Locator("#copay-note")).ToHaveTextAsync("Same");
    }
}
