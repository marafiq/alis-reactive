using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenEachCardOfOnePartialReachesItsOwnInput(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.M7_X6_0)]
    public async Task each_cards_copy_reads_the_name_its_own_card_holds()
    {
        var contacts = await NewWorld();
        await contacts.Page.GotoAsync(CarePages.KinCards);
        await contacts.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await contacts.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Bo");
        await contacts.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Copy", Exact = true }).ClickAsync();
        await contacts.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Copy", Exact = true }).ClickAsync();
        await Expect(contacts.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Copied", Exact = true })).ToHaveTextAsync("Ada");
        await Expect(contacts.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Copied", Exact = true })).ToHaveTextAsync("Bo");

        var emptied = await NewWorld();
        await emptied.Page.GotoAsync(CarePages.KinCards);
        await emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Bo");
        await emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Copy", Exact = true }).ClickAsync();
        await emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Copy", Exact = true }).ClickAsync();
        await Expect(emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Copied", Exact = true })).ToHaveTextAsync("Bo");
        await emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).ClearAsync();
        await emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "Copy", Exact = true }).ClickAsync();
        await Expect(emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Alternate", Exact = true })
            .GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Copied", Exact = true })).ToHaveTextAsync("");
        await Expect(emptied.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Status, new LocatorGetByRoleOptions { Name = "Copied", Exact = true })).ToHaveTextAsync("Ada");
    }
}
