using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.NativeAppLevelObjects;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARowsLinkIsClicked(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M25_F3.X17_34_0)]
    public async Task a_plain_click_loads_the_panel_and_a_modified_or_middle_click_opens_the_link()
    {
        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.ResidentLinks);
        await plain.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Ada");
        await Expect(plain.Page).ToHaveURLAsync(CarePages.ResidentLinks);

        var opened = await NewWorld();
        await opened.Page.GotoAsync(CarePages.ResidentLinks);
        var bea = opened.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Bea", Exact = true });
        var cy = opened.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Cy", Exact = true });
        await bea.ClickAsync(new LocatorClickOptions { Modifiers = [KeyboardModifier.ControlOrMeta] });
        await cy.ClickAsync(new LocatorClickOptions { Button = MouseButton.Middle });
        await Expect(bea).ToHaveAttributeAsync("href", "/Residents/2");
        await Expect(cy).ToHaveAttributeAsync("href", "/Residents/3");
        await opened.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ada", Exact = true }).ClickAsync();
        await Expect(opened.Page.Locator("#resident-detail").GetByRole(AriaRole.Heading)).ToHaveTextAsync("Ada");
        await Expect(opened.Page).ToHaveURLAsync(CarePages.ResidentLinks);
        Assert.That(opened.SentTo(CareEndpoints.BeasResidentPanel), Is.EqualTo(0));
        Assert.That(opened.SentTo(CareEndpoints.CysResidentPanel), Is.EqualTo(0));
    }
}
