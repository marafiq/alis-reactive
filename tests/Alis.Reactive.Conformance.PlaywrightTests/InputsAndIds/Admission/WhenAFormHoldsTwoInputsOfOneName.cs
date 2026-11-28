using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAFormHoldsTwoInputsOfOneName(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F3.X13_2_0)]
    public async Task the_form_sends_nothing_on_save_and_the_console_holds_one_error()
    {
        var twoNames = await NewWorld();
        await twoNames.Page.GotoAsync(CarePages.SignInWithTwoNames);
        await twoNames.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await twoNames.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Guest name", Exact = true }).FillAsync("Bo");
        await twoNames.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(twoNames.Page.Locator("#bound")).ToHaveTextAsync("-");
        Assert.That(twoNames.ConsoleErrors, Has.Count.EqualTo(1));

        var nickname = await NewWorld();
        await nickname.Page.GotoAsync(CarePages.SignInWithANickname);
        await nickname.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Name", Exact = true }).FillAsync("Ada");
        await nickname.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Guest nickname", Exact = true }).FillAsync("Bo");
        await nickname.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Save", Exact = true }).ClickAsync();
        await Expect(nickname.Page.Locator("#bound")).ToHaveTextAsync("name Ada");
    }
}
