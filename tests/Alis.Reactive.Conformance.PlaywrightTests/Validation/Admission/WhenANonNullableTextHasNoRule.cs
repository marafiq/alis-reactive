using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenANonNullableTextHasNoRule(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F2.X16_8_0)]
    public async Task it_saves_empty_and_the_endpoints_check_adds_no_required_message()
    {
        var north = await NewWorld();
        await north.Page.GotoAsync(CarePages.WardSignIn);
        await north.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).FillAsync("North");
        await north.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync();
        await Expect(north.Page.Locator("#signed-in")).ToHaveTextAsync("North");
        await Expect(north.Page.GetByText("The Name field is required.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var noWard = await NewWorld();
        await noWard.Page.GotoAsync(CarePages.WardSignIn);
        await noWard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync();
        await Expect(noWard.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true })
            .GetByText("'Ward' must not be empty.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(noWard.Page.Locator("#signed-in")).ToBeEmptyAsync();
    }
}
