using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAWristbandColorIsShown(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F12.M21_X1007_0)]
    public async Task the_picker_holds_the_color_the_model_holds()
    {
        var red = await NewWorld();
        await red.Page.GotoAsync(CarePages.Wristband);
        await red.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show band", Exact = true }).ClickAsync();

        await Expect(red.Page.Locator("#band-shown")).ToHaveTextAsync("#ff0000ff");

        var blue = await NewWorld();
        await blue.Page.GotoAsync(CarePages.WristbandBlue);
        await blue.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show band", Exact = true }).ClickAsync();

        await Expect(blue.Page.Locator("#band-shown")).ToHaveTextAsync("#0000ffff");
    }
}
