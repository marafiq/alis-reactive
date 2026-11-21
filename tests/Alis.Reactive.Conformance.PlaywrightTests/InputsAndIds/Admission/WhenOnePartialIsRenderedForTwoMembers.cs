using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenOnePartialIsRenderedForTwoMembers(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.X14_9_0)]
    public async Task each_card_posts_its_own_values_under_its_own_member()
    {
        var moveIn = await NewWorld();
        await moveIn.Page.GotoAsync(CarePages.KinMoveIn);
        await moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("0101");
        await moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Bo");
        await moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("0202");
        await moveIn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(moveIn.Page.Locator("#bound")).ToHaveTextAsync("next of kin Ada 0101 and alternate Bo 0202");

        var noAlternate = await NewWorld();
        await noAlternate.Page.GotoAsync(CarePages.KinMoveIn);
        await noAlternate.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await noAlternate.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Phone", Exact = true }).FillAsync("0101");
        await noAlternate.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(noAlternate.Page.Locator("#bound")).ToHaveTextAsync("next of kin Ada 0101 and alternate none");
    }
}
