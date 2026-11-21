using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHandWrittenInputFailsOnTheServer(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_18_0)]
    public async Task the_message_shows_in_the_forms_summary_and_nowhere_beside_the_input()
    {
        var handWritten = await NewWorld();
        await handWritten.Page.GotoAsync(CarePages.SupplyCodes);
        await handWritten.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Code", Exact = true }).FillAsync("ZZ9");
        await handWritten.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit", Exact = true }).ClickAsync();
        await Expect(handWritten.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Unknown code." });
        await Expect(handWritten.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true })
            .GetByText("Unknown code.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();

        var bound = await NewWorld();
        await bound.Page.GotoAsync(CarePages.SupplyCodesWithAFrameworkInput);
        await bound.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Code", Exact = true }).FillAsync("ZZ9");
        await bound.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit", Exact = true }).ClickAsync();
        await Expect(bound.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Code", Exact = true })
            .GetByText("Unknown code.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(bound.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Unknown code." });
    }
}
