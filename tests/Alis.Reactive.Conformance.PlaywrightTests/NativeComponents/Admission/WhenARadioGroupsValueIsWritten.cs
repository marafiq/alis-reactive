using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARadioGroupsValueIsWritten(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F7.M24_X3_0)]
    public async Task the_write_checks_the_radio_whose_value_it_holds()
    {
        var moved = await NewWorld();
        await moved.Page.GotoAsync(CarePages.WingMove);
        await moved.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Move to Oak", Exact = true }).ClickAsync();
        await Expect(moved.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Oak", Exact = true })).ToBeCheckedAsync();
        await Expect(moved.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Rose", Exact = true })).Not.ToBeCheckedAsync();

        var chosen = await NewWorld();
        await chosen.Page.GotoAsync(CarePages.WingMove);
        await chosen.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Lily", Exact = true }).ClickAsync();
        await Expect(chosen.Page.Locator("#wing-echo")).ToHaveTextAsync("Lily");
        await Expect(chosen.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Lily", Exact = true })).ToBeCheckedAsync();
        await Expect(chosen.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Rose", Exact = true })).Not.ToBeCheckedAsync();
        await Expect(chosen.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Oak", Exact = true })).Not.ToBeCheckedAsync();
    }
}
