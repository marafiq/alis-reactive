using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnInputOutsideTheContainerBindsTheName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_9_0)]
    public async Task the_containers_read_finds_only_the_input_inside_it()
    {
        var respite = await NewWorld();
        await respite.Page.GotoAsync(CarePages.RespiteQuote);
        var nights = respite.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Nights", Exact = true });
        var quote = respite.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Quote", Exact = true });

        await nights.FillAsync("7");
        await respite.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Previous nights", Exact = true }).FillAsync("21");
        await quote.ClickAsync();
        await Expect(respite.Page.Locator("#quote")).ToHaveTextAsync("7");

        await nights.FillAsync("");
        await quote.ClickAsync();
        await Expect(respite.Page.Locator("#quote")).ToHaveTextAsync("14");
    }
}
