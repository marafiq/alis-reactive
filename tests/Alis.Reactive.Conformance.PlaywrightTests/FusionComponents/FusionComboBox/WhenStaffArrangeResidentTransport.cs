using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenStaffArrangeResidentTransport(string host) : FusionFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F13.M21_X1210_0)]
    public async Task weekend_services_replace_the_weekday_providers_in_the_open_list()
    {
        var transport = await NewWorld();
        await transport.Page.GotoAsync(CarePages.TransportProviders);
        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Browse weekday providers", Exact = true }).ClickAsync();
        await Expect(transport.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar Community Cars", "Northside Transit" });

        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Browse weekend services", Exact = true }).ClickAsync();

        await Expect(transport.Page.GetByRole(AriaRole.Listbox).GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Sunrise wheelchair van", "Harbour evening shuttle" });
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1211_0)]
    public async Task staff_browse_weekend_services_by_shift_and_weekday_providers_by_name()
    {
        var transport = await NewWorld();
        await transport.Page.GotoAsync(CarePages.TransportProviders);
        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Browse weekend services", Exact = true }).ClickAsync();
        var catalogue = transport.Page.GetByRole(AriaRole.Listbox);
        await Expect(catalogue.GetByText("Day shift", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(catalogue.GetByText("Evening shift", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Browse weekday providers", Exact = true }).ClickAsync();

        await Expect(catalogue.GetByRole(AriaRole.Option)).ToHaveTextAsync(new[] { "Cedar Community Cars", "Northside Transit" });
        await Expect(catalogue.GetByText("Day shift", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(catalogue.GetByText("Evening shift", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }

    [Test]
    [FailsFor(Promises.M21_F13.M21_X1212_0)]
    public async Task choosing_family_transport_clears_the_provider_from_the_booking()
    {
        var transport = await NewWorld();
        await transport.Page.GotoAsync(CarePages.TransportProviders);
        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Browse weekday providers", Exact = true }).ClickAsync();
        await transport.Page.GetByRole(AriaRole.Option, new PageGetByRoleOptions { Name = "Northside Transit", Exact = true }).ClickAsync();
        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save transport", Exact = true }).ClickAsync();
        await Expect(transport.Page.Locator("#booking-confirmation")).ToHaveTextAsync("Transport booked with Northside Transit.");

        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Use family transport", Exact = true }).ClickAsync();
        await transport.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save transport", Exact = true }).ClickAsync();

        await Expect(transport.Page.Locator("#booking-confirmation")).ToHaveTextAsync("Family will arrange transport.");
    }
}
