using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionUnloadsItsPartial(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X8_0)]
    public async Task its_input_leaves_the_page_and_the_arrival_is_recorded_without_it()
    {
        var tickedOff = await NewWorld();
        await tickedOff.Page.GotoAsync(CarePages.ArrivalDesk);
        await tickedOff.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cy Wood");
        await tickedOff.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("North");
        await tickedOff.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Room", Exact = true }).SelectOptionAsync("11");
        await tickedOff.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Publicly funded", Exact = true }).CheckAsync();
        await Expect(tickedOff.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Funding number", Exact = true })).ToBeVisibleAsync();
        await tickedOff.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Publicly funded", Exact = true }).ClickAsync();
        await Expect(tickedOff.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Funding number", Exact = true })).ToBeHiddenAsync();
        await tickedOff.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record the arrival", Exact = true }).ClickAsync();
        await Expect(tickedOff.Page.Locator("#arrived-room")).ToHaveTextAsync("11");

        var leftTicked = await NewWorld();
        await leftTicked.Page.GotoAsync(CarePages.ArrivalDesk);
        await leftTicked.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cy Wood");
        await leftTicked.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("North");
        await leftTicked.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Room", Exact = true }).SelectOptionAsync("11");
        await leftTicked.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Publicly funded", Exact = true }).CheckAsync();
        await Expect(leftTicked.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Funding number", Exact = true })).ToBeVisibleAsync();
        await leftTicked.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record the arrival", Exact = true }).ClickAsync();
        await Expect(leftTicked.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Funding", Exact = true })
            .GetByText("Give the funding number.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(leftTicked.Page.Locator("#arrived-room")).ToHaveTextAsync("none yet");
    }
}
