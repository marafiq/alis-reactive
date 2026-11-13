using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotsContainerBindsTheSameNameAsThePages(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_17_0)]
    public async Task the_servers_message_shows_only_at_the_container_whose_request_was_answered()
    {
        var contact = await NewWorld();
        await contact.Page.GotoAsync(CarePages.ResidentContact);
        await contact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open contact", Exact = true }).ClickAsync();
        await contact.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contact", Exact = true }).ClickAsync();
        await Expect(contact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact's name", Exact = true })
            .GetByText("Name the contact.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(contact.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Contact", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name the contact." });
        await Expect(contact.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Resident's name", Exact = true })
            .GetByText("Name the contact.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(contact.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);

        var resident = await NewWorld();
        await resident.Page.GotoAsync(CarePages.ResidentContact);
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Open contact", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save contact", Exact = true })).ToBeVisibleAsync();
        await resident.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save resident", Exact = true }).ClickAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Resident's name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Name the resident." });
        await Expect(resident.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Contact's name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(resident.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Contact", Exact = true })
            .GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
