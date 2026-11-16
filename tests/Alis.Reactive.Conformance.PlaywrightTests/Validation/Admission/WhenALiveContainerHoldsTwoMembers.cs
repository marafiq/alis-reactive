using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenALiveContainerHoldsTwoMembers(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_41_0)]
    public async Task leaving_one_input_shows_its_own_members_message_and_not_the_others()
    {
        var kin = await NewWorld();
        await kin.Page.GotoAsync(CarePages.NextOfKinAdmission);
        await kin.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("A");
        await kin.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).PressAsync("Tab");
        await Expect(kin.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Write the kin's full name.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(kin.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Phone", Exact = true })
            .GetByText("Give the kin's phone number.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        await kin.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Phone", Exact = true }).PressAsync("Tab");
        await Expect(kin.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Phone", Exact = true })
            .GetByText("Give the kin's phone number.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(kin.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Write the kin's full name.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
    }
}
