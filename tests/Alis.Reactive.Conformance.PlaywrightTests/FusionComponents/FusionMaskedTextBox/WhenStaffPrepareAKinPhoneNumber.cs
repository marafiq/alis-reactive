using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffPrepareAKinPhoneNumber(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F25.M21_X2000_0)]
    public async Task the_contact_slip_uses_the_formatted_number_and_the_dialer_uses_digits()
    {
        var slip = await NewWorld();
        await slip.Page.GotoAsync(CarePages.KinHomePhone);
        var phone = slip.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Phone", Exact = true }).GetByRole(AriaRole.Textbox);
        await phone.FillAsync("2125550146");
        await slip.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare contact slip", Exact = true }).ClickAsync();

        await Expect(slip.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Contact slip number", Exact = true })).ToHaveTextAsync("212-555-0146");

        var dialer = await NewWorld();
        await dialer.Page.GotoAsync(CarePages.KinMobilePhone);
        await dialer.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Prepare dialer entry", Exact = true }).ClickAsync();

        await Expect(dialer.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Dialer number", Exact = true })).ToHaveTextAsync("5559876543");
    }
}
