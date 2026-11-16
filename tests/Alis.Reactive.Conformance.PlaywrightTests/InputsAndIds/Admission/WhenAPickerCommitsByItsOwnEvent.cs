using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPickerCommitsByItsOwnEvent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.M7_X3_0)]
    public async Task picking_a_level_rechecks_the_picker_while_it_keeps_the_focus()
    {
        var care = await NewWorld();
        await care.Page.GotoAsync(CarePages.CareLevelChoice);
        await care.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByText("Pick a care level.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        await care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowDown");
        await care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByText("Pick a care level.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true })).ToBeFocusedAsync();

        await care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("ArrowUp");
        await care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByRole(AriaRole.Listbox, new LocatorGetByRoleOptions { Name = "Value", Exact = true }).PressAsync("Enter");
        await Expect(care.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Care level", Exact = true })
            .GetByText("Pick a care level.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
