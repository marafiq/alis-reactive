using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnOtpInputStartsAtItsMembersCode(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F33.M21_X612_0)]
    public async Task the_boxes_show_the_code_the_member_holds()
    {
        var locked = await NewWorld();
        await locked.Page.GotoAsync(CarePages.MedicationCabinet);

        await Expect(locked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unlock code", Exact = true }).GetByRole(AriaRole.Spinbutton, new LocatorGetByRoleOptions { Name = "Enter Otp Character 1", Exact = true })).ToHaveValueAsync("4");
        await Expect(locked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unlock code", Exact = true }).GetByRole(AriaRole.Spinbutton, new LocatorGetByRoleOptions { Name = "Enter Otp Character 2", Exact = true })).ToHaveValueAsync("8");
        await Expect(locked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unlock code", Exact = true }).GetByRole(AriaRole.Spinbutton, new LocatorGetByRoleOptions { Name = "Enter Otp Character 3", Exact = true })).ToHaveValueAsync("2");
        await Expect(locked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unlock code", Exact = true }).GetByRole(AriaRole.Spinbutton, new LocatorGetByRoleOptions { Name = "Enter Otp Character 4", Exact = true })).ToHaveValueAsync("1");

        var unset = await NewWorld();
        await unset.Page.GotoAsync(CarePages.MedicationCabinetUnset);

        await Expect(unset.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unlock code", Exact = true }).GetByRole(AriaRole.Spinbutton, new LocatorGetByRoleOptions { Name = "Enter Otp Character 1", Exact = true })).ToHaveValueAsync("");
        await Expect(unset.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Unlock code", Exact = true }).GetByRole(AriaRole.Spinbutton, new LocatorGetByRoleOptions { Name = "Enter Otp Character 4", Exact = true })).ToHaveValueAsync("");
    }
}
