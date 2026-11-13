using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenNoInputInsideTheContainerBindsTheMember(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_7_0)]
    public async Task the_containers_read_shows_the_start_the_endpoint_binds()
    {
        var standard = await NewWorld();
        await standard.Page.GotoAsync(CarePages.StandardShortStay);
        await standard.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quote", Exact = true }).ClickAsync();
        await Expect(standard.Page.Locator("#quote")).ToHaveTextAsync("14");

        var custom = await NewWorld();
        await custom.Page.GotoAsync(CarePages.CustomShortStay);
        await custom.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Nights", Exact = true }).FillAsync("21");
        await custom.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quote", Exact = true }).ClickAsync();
        await Expect(custom.Page.Locator("#quote")).ToHaveTextAsync("21");
    }
}
