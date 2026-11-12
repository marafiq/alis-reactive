using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAPartialIsTypedToTheBaseOfThePagesModel(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F2.M7_X4_0)]
    public async Task the_pages_read_of_the_member_reaches_the_input_the_partial_rendered()
    {
        var lobby = await NewWorld();
        await lobby.Page.GotoAsync(CarePages.Lobby);
        await lobby.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await lobby.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Greet", Exact = true }).ClickAsync();
        await Expect(lobby.Page.Locator("#greeting")).ToHaveTextAsync("Ada");

        var emptied = await NewWorld();
        await emptied.Page.GotoAsync(CarePages.Lobby);
        await emptied.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Greet", Exact = true }).ClickAsync();
        await Expect(emptied.Page.Locator("#greeting")).ToHaveTextAsync("Ada");
        await emptied.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).ClearAsync();
        await emptied.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Greet", Exact = true }).ClickAsync();
        await Expect(emptied.Page.Locator("#greeting")).ToHaveTextAsync("");
    }
}
