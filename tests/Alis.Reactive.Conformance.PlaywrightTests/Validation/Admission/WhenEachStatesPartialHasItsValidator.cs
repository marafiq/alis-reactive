using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenEachStatesPartialHasItsValidator(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X16_5_0)]
    public async Task each_save_runs_only_the_validator_of_the_partial_that_holds_it()
    {
        var states = await NewWorld();
        await states.Page.GotoAsync(CarePages.StateMoveIn);
        await states.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Texas", Exact = true }).ClickAsync();
        await states.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(states.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Give the Texas license." });
        await states.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Massachusetts", Exact = true }).ClickAsync();
        await Expect(states.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Permit", Exact = true })).ToBeVisibleAsync();
        await states.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(states.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "Give the Massachusetts permit." });
        await Expect(states.Page.Locator("#moved-in")).ToBeEmptyAsync();

        var texas = await NewWorld();
        await texas.Page.GotoAsync(CarePages.StateMoveIn);
        await texas.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Texas", Exact = true }).ClickAsync();
        await texas.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "License", Exact = true }).FillAsync("TX-1");
        await texas.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(texas.Page.Locator("#moved-in")).ToHaveTextAsync("TX-1");
    }
}
