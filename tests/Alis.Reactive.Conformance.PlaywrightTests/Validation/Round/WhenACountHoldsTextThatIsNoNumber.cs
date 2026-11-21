using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenACountHoldsTextThatIsNoNumber(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F3.X16_4_0)]
    public async Task the_binders_message_shows_beside_the_count_and_nothing_is_sent()
    {
        var mistyped = await NewWorld();
        await mistyped.Page.GotoAsync(CarePages.SupplyCount);
        await mistyped.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Count", Exact = true }).FillAsync("12a");
        await mistyped.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(mistyped.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Count", Exact = true })
            .GetByText("The value '12a' is not valid for Count.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(mistyped.Page.Locator("#saved")).ToBeEmptyAsync();

        var counted = await NewWorld();
        await counted.Page.GotoAsync(CarePages.SupplyCount);
        await counted.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Count", Exact = true }).FillAsync("12");
        await counted.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(counted.Page.Locator("#saved")).ToHaveTextAsync("12");
        await Expect(counted.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
