using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestSetsAMemberBesideAContainerItDoesNotSend(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F3.X16_6_0)]
    public async Task the_check_reads_the_status_it_sends_and_the_starts_never_the_name_typed_in_the_container()
    {
        var arrived = await NewWorld();
        await arrived.Page.GotoAsync(CarePages.ArrivalIntake);
        await arrived.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("");
        await arrived.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Mark arrived", Exact = true }).ClickAsync();
        await Expect(arrived.Page.Locator("#status")).ToHaveTextAsync("Unnamed, Arrived");
        await Expect(arrived.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var intake = await NewWorld();
        await intake.Page.GotoAsync(CarePages.ArrivalIntake);
        await intake.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("");
        await intake.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save intake", Exact = true }).ClickAsync();
        await Expect(intake.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Name", Exact = true })
            .GetByText("Name the resident.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(intake.Page.Locator("#status")).ToHaveTextAsync("Unnamed, Expected");
    }
}
