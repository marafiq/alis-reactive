using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARadioGroupIsFocused(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F7.M24_X12_0)]
    public async Task the_focus_lands_on_the_chosen_radio_else_the_first()
    {
        var inLily = await NewWorld();
        await inLily.Page.GotoAsync(CarePages.WingChange);
        await inLily.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Change wing", Exact = true }).ClickAsync();
        await Expect(inLily.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Lily", Exact = true })).ToBeFocusedAsync();

        var withNoWing = await NewWorld();
        await withNoWing.Page.GotoAsync(CarePages.WingChangeWithNoWing);
        await withNoWing.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Change wing", Exact = true }).ClickAsync();
        await Expect(withNoWing.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Rose", Exact = true })).ToBeFocusedAsync();
    }
}
