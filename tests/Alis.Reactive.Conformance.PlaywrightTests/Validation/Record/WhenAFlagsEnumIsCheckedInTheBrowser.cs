using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFlagsEnumIsCheckedInTheBrowser(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F1.X16_22_0)]
    public async Task a_combination_of_its_days_is_saved_and_a_zero_it_has_no_value_for_sends_nothing()
    {
        var fromZero = await NewWorld();
        await fromZero.Page.GotoAsync(CarePages.CareRota);
        await fromZero.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await fromZero.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save care", Exact = true }).ClickAsync();
        await Expect(fromZero.Page.Locator("#saved")).ToHaveTextAsync("Ada, Mon, Tue");
        await fromZero.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save rota", Exact = true }).ClickAsync();
        await Expect(fromZero.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "'Rota' has a range of values which does not include '0'." });
        await Expect(fromZero.Page.Locator("#saved")).ToHaveTextAsync("Ada, Mon, Tue");

        var fromEarly = await NewWorld();
        await fromEarly.Page.GotoAsync(CarePages.CareRotaFromEarly);
        await fromEarly.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await fromEarly.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save rota", Exact = true }).ClickAsync();
        await Expect(fromEarly.Page.Locator("#saved")).ToHaveTextAsync("Ada, Early");
        await Expect(fromEarly.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
