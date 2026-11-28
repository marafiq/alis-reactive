using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASlotJoinsAndLeavesTheContainer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_8_0)]
    public async Task the_containers_read_takes_the_inputs_inside_it_when_it_runs()
    {
        var stay = await NewWorld();
        await stay.Page.GotoAsync(CarePages.OvernightStay);
        var quote = stay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quote", Exact = true });

        await quote.ClickAsync();
        await Expect(stay.Page.Locator("#quote")).ToHaveTextAsync("14");

        await stay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Add the overnight", Exact = true }).ClickAsync();
        await stay.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Nights", Exact = true }).FillAsync("21");
        await quote.ClickAsync();
        await Expect(stay.Page.Locator("#quote")).ToHaveTextAsync("21");

        await stay.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Remove the overnight", Exact = true }).ClickAsync();
        await Expect(stay.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Nights", Exact = true })).ToHaveCountAsync(0);
        await quote.ClickAsync();
        await Expect(stay.Page.Locator("#quote")).ToHaveTextAsync("14");
    }
}
