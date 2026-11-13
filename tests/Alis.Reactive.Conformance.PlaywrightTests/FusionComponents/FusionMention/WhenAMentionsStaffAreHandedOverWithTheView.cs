using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAMentionsStaffAreHandedOverWithTheView(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F29.M21_X510_0)]
    public async Task the_popup_lists_each_staff_member_by_the_name_its_member_carries()
    {
        var okafor = await NewWorld();
        await okafor.Page.GotoAsync(CarePages.NoteMention);
        await okafor.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Note", Exact = true }).GetByRole(AriaRole.Textbox).PressSequentiallyAsync("@Ok");

        await Expect(okafor.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Nurse Okafor", Exact = true })).ToBeVisibleAsync();

        var lindqvist = await NewWorld();
        await lindqvist.Page.GotoAsync(CarePages.NoteMention);
        await lindqvist.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Note", Exact = true }).GetByRole(AriaRole.Textbox).PressSequentiallyAsync("@Li");
        await Expect(lindqvist.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dr. Lindqvist", Exact = true })).ToBeVisibleAsync();
        await lindqvist.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dr. Lindqvist", Exact = true }).ClickAsync();

        await Expect(lindqvist.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Note", Exact = true }).GetByRole(AriaRole.Textbox)).ToHaveValueAsync("Dr. Lindqvist");
    }
}
