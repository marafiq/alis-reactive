using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.PathsAndWire;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartialsInputAgreesOnItsFourNames(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M15_F1.X2_4_0)]
    public async Task the_servers_message_for_the_kins_name_shows_at_the_input_the_partial_rendered()
    {
        var moveIn = await NewWorld();
        await moveIn.Page.GotoAsync(CarePages.MoveInWithKin);
        await moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Sam Lee");
        await moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada Lee");
        await moveIn.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByText("Ada Lee lives here; name another next of kin.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(moveIn.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByText("Ada Lee lives here; name another next of kin.", new LocatorGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(moveIn.Page.Locator("#move-in-status")).ToHaveTextAsync("Not saved");

        var otherKin = await NewWorld();
        await otherKin.Page.GotoAsync(CarePages.MoveInWithKin);
        await otherKin.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Resident", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Sam Lee");
        await otherKin.Page.GetByRole(AriaRole.Region, new PageGetByRoleOptions { Name = "Next of kin", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ben Lee");
        await otherKin.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(otherKin.Page.Locator("#move-in-status")).ToHaveTextAsync("Saved");
        await Expect(otherKin.Page.GetByText("Ada Lee lives here; name another next of kin.", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
    }
}
