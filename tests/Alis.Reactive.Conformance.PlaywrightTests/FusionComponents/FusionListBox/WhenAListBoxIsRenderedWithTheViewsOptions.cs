using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAListBoxIsRenderedWithTheViewsOptions(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F27.M21_X1018_0)]
    public async Task the_list_starts_at_the_views_selection_and_a_step_reaches_it_at_its_id()
    {
        var withCarey = await NewWorld();
        await withCarey.Page.GotoAsync(CarePages.DiningCompanions);
        await Expect(withCarey.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dev", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
        await withCarey.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Seat with Carey", Exact = true }).ClickAsync();

        await Expect(withCarey.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Carey", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
        await Expect(withCarey.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dev", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
        await Expect(withCarey.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Ann", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).Not.ToHaveClassAsync("e-list-item e-selected");

        var everyone = await NewWorld();
        await everyone.Page.GotoAsync(CarePages.DiningCompanions);
        await Expect(everyone.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dev", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
        await everyone.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Seat everyone", Exact = true }).ClickAsync();

        await Expect(everyone.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Carey", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
        await Expect(everyone.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Dev", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
        await Expect(everyone.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option, new LocatorGetByRoleOptions { Name = "Ann", Exact = true }).Filter(new LocatorFilterOptions { Visible = true })).ToHaveClassAsync("e-list-item e-selected");
    }
}
