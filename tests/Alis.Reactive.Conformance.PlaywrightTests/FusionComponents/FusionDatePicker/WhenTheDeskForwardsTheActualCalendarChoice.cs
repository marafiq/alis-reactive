using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheDeskForwardsTheActualCalendarChoice(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F15.M21_X1768_0)]
    public async Task a_linked_transport_calendar_preserves_the_origin_of_the_arrival_choice()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFamilyTransport);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transport instruction", Exact = true })).ToHaveTextAsync("Confirm the resident’s chosen transfer.");
        await resident.Page.GetByText("18", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transport date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-18");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalOfficeTransport);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Transport instruction", Exact = true })).ToHaveTextAsync("Review the office transport referral.");
        await referral.Page.GetByText("18", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Transport date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-18");
    }

    [Test]
    [FailsFor(Promises.M21_F15.M21_X1769_0)]
    public async Task finalizing_the_review_records_who_asked_to_close_the_calendar()
    {
        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ArrivalFamilyFinalization);
        await resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await resident.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await Expect(resident.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(resident.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Handover decision", Exact = true })).ToHaveTextAsync("The family chose the arrival date.");

        var referral = await NewWorld();
        await referral.Page.GotoAsync(CarePages.ArrivalOfficeFinalization);
        await referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox).PressAsync("Alt+ArrowDown");
        await referral.Page.GetByText("16", new PageGetByTextOptions { Exact = true }).ClickAsync();
        await Expect(referral.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Arrival date", Exact = true }).GetByRole(AriaRole.Combobox)).ToHaveValueAsync("2026-07-16");
        await Expect(referral.Page.GetByRole(AriaRole.Dialog)).ToHaveCountAsync(0);
        await Expect(referral.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Handover decision", Exact = true })).ToHaveTextAsync("The scheduling desk finalized the choice.");
    }
}
