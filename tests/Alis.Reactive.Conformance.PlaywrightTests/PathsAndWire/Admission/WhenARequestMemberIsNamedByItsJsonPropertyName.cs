using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestMemberIsNamedByItsJsonPropertyName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X2_3_0)]
    public async Task the_member_reaches_the_endpoint_bound_under_the_name_its_attribute_declares()
    {
        var nextOfKin = await NewWorld();
        await nextOfKin.Page.GotoAsync(CarePages.GivenName);
        await nextOfKin.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Given name", Exact = true }).FillAsync("Ada");
        await nextOfKin.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(nextOfKin.Page.Locator("#saved-name")).ToHaveTextAsync("Ada");

        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.GivenName);
        await unnamed.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(unnamed.Page.Locator("#saved-name")).ToHaveTextAsync("(none)");
    }
}
