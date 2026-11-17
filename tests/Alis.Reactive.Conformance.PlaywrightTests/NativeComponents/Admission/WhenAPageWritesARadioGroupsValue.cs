using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPageWritesARadioGroupsValue(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M24_F7.M24_X11_0)]
    public async Task the_write_raises_no_change()
    {
        var transferred = await NewWorld();
        await transferred.Page.GotoAsync(CarePages.WingTransfer);
        await transferred.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Transfer to Oak", Exact = true }).ClickAsync();
        await Expect(transferred.Page.Locator("#transfer-status")).ToHaveTextAsync("Transfer to Oak requested");
        await Expect(transferred.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Oak", Exact = true })).ToBeCheckedAsync();
        await Expect(transferred.Page.Locator("#wing-echo")).ToBeEmptyAsync();

        var picked = await NewWorld();
        await picked.Page.GotoAsync(CarePages.WingTransfer);
        await picked.Page.GetByRole(AriaRole.Radio, new PageGetByRoleOptions { Name = "Lily", Exact = true }).ClickAsync();
        await Expect(picked.Page.Locator("#wing-echo")).ToHaveTextAsync("Lily");
    }
}
