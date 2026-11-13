using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMaskedTextBoxIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F25.M21_X1016_0)]
    public async Task the_box_starts_at_the_value_the_member_holds_through_its_mask()
    {
        var home = await NewWorld();
        await home.Page.GotoAsync(CarePages.KinHomePhone);

        await Expect(home.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Phone", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("555-123-4567");

        var mobile = await NewWorld();
        await mobile.Page.GotoAsync(CarePages.KinMobilePhone);

        await Expect(mobile.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Phone", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("555-987-6543");
    }
}
