using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARequestSendsAnInputOutsideEveryContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_16_0)]
    public async Task the_servers_message_shows_beside_the_input_and_in_the_notice()
    {
        var empty = await NewWorld();
        await empty.Page.GotoAsync(CarePages.WardAssignment);
        await empty.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assign", Exact = true }).ClickAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true })
            .GetByText("Ward is required.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(empty.Page.GetByRole(AriaRole.Alert).GetByText("Ward is required.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();

        var named = await NewWorld();
        await named.Page.GotoAsync(CarePages.WardAssignment);
        await named.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Ward", Exact = true }).FillAsync("North");
        await named.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Assign", Exact = true }).ClickAsync();
        await Expect(named.Page.Locator("#assigned")).ToHaveTextAsync("North");
        await Expect(named.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Ward", Exact = true })
            .GetByText("Ward is required.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
