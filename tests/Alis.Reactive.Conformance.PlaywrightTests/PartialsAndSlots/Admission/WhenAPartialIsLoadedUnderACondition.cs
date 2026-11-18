using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PartialsAndSlots;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartialIsLoadedUnderACondition(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M9_F2.M9_X7_0)]
    public async Task its_input_is_held_to_the_rule_of_the_container_it_is_loaded_into()
    {
        var unfunded = await NewWorld();
        await unfunded.Page.GotoAsync(CarePages.ArrivalDesk);
        await unfunded.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cy Wood");
        await unfunded.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("North");
        await unfunded.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Room", Exact = true }).SelectOptionAsync("11");
        await unfunded.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Publicly funded", Exact = true }).CheckAsync();
        await Expect(unfunded.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Funding number", Exact = true })).ToBeVisibleAsync();
        await unfunded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record the arrival", Exact = true }).ClickAsync();
        await Expect(unfunded.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Funding", Exact = true })
            .GetByText("Give the funding number.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unfunded.Page.Locator("#arrived-room")).ToHaveTextAsync("none yet");

        var funded = await NewWorld();
        await funded.Page.GotoAsync(CarePages.ArrivalDesk);
        await funded.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Cy Wood");
        await funded.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).SelectOptionAsync("North");
        await funded.Page.GetByRole(AriaRole.Combobox, new PageGetByRoleOptions { Name = "Room", Exact = true }).SelectOptionAsync("11");
        await funded.Page.GetByRole(AriaRole.Checkbox, new PageGetByRoleOptions { Name = "Publicly funded", Exact = true }).CheckAsync();
        await funded.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Funding number", Exact = true }).FillAsync("F-204");
        await funded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Record the arrival", Exact = true }).ClickAsync();
        await Expect(funded.Page.Locator("#arrived-room")).ToHaveTextAsync("11");
    }
}
