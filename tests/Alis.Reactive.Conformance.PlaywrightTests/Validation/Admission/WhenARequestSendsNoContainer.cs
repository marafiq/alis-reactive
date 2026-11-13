using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestSendsNoContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_15_0)]
    public async Task its_check_fails_in_the_notice_and_sends_nothing()
    {
        var unwarded = await NewWorld();
        await unwarded.Page.GotoAsync(CarePages.BedAssignment);
        await unwarded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assign", Exact = true }).ClickAsync();
        await Expect(unwarded.Page.GetByRole(AriaRole.Alert).GetByText("Ward is required.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(unwarded.Page.Locator("#assigned")).ToHaveTextAsync("");

        var warded = await NewWorld();
        await warded.Page.GotoAsync(CarePages.BedAssignmentInAWard);
        await warded.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assign", Exact = true }).ClickAsync();
        await Expect(warded.Page.Locator("#assigned")).ToHaveTextAsync("North, bed 4");
    }
}
