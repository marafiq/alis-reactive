using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInPlaceEditorIsRenderedForAMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F24.M21_X1015_0)]
    public async Task the_editor_starts_at_the_value_the_member_holds()
    {
        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.ResidentRoomNamed);

        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true }).GetByText("North 12", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var unnamed = await NewWorld();
        await unnamed.Page.GotoAsync(CarePages.ResidentRoomUnnamed);

        await Expect(unnamed.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Room", Exact = true }).GetByText("Empty", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
