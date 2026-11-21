using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionTemplates;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAConditionComparesAConstantFirst(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M22_F2.M22_X2_0)]
    public async Task the_list_opened_after_a_condition_ending_on_a_number_still_shows_its_items()
    {
        var constantFirst = await NewWorld();
        await constantFirst.Page.GotoAsync(CarePages.WardRooms);

        await constantFirst.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Residents", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(constantFirst.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Ann under 90", "Bo 90 and over" });
        await constantFirst.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(constantFirst.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "North 1 spare", "North 2 full" });

        var memberFirst = await NewWorld();
        await memberFirst.Page.GotoAsync(CarePages.WardRoomsBedsFirst);

        await memberFirst.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Residents", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(memberFirst.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Ann under 90", "Bo 90 and over" });
        await memberFirst.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Rooms", Exact = true }).GetByRole(AriaRole.Combobox).ClickAsync();
        await Expect(memberFirst.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "North 1 spare", "North 2 full" });
    }
}
