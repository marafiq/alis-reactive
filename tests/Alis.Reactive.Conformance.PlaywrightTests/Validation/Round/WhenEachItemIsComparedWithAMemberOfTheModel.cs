using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10AndNet48))]
public sealed class WhenEachItemIsComparedWithAMemberOfTheModel(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F1.X16_39_0)]
    public async Task only_the_item_below_the_rounds_least_fails_before_sending()
    {
        var round = await NewWorld();
        await round.Page.GotoAsync(CarePages.LeastDoseRound);
        await round.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Least", Exact = true }).FillAsync("5");
        await round.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await round.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("4");
        await round.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(round.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second dose", Exact = true })
            .GetByText("Give at least the least dose.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(round.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First dose", Exact = true })
            .GetByText("Give at least the least dose.", new LocatorGetByTextOptions { Exact = true })).ToBeHiddenAsync();
        await Expect(round.Page.Locator("#saved")).ToBeEmptyAsync();
        Assert.That(round.SentTo(CareEndpoints.LeastDoseRounds), Is.EqualTo(0));

        var enough = await NewWorld();
        await enough.Page.GotoAsync(CarePages.LeastDoseRound);
        await enough.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Least", Exact = true }).FillAsync("5");
        await enough.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "First dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("5");
        await enough.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Second dose", Exact = true })
            .GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Amount", Exact = true }).FillAsync("6");
        await enough.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(enough.Page.Locator("#saved")).ToHaveTextAsync("5, 6");
        await Expect(enough.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
