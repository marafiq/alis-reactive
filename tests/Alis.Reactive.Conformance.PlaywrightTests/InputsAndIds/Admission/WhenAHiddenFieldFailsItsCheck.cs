using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.InputsAndIds;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAHiddenFieldFailsItsCheck(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M7_F1.M7_X1_0)]
    public async Task its_message_shows_in_the_forms_summary_and_its_label_is_not_rendered()
    {
        var noPass = await NewWorld();
        await noPass.Page.GotoAsync(CarePages.VisitorPass);
        await noPass.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync();
        await Expect(noPass.Page.GetByRole(AriaRole.Listitem)).ToHaveTextAsync(new[] { "The desk gives every visitor a pass." });
        await Expect(noPass.Page.GetByText("Pass", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);
        await Expect(noPass.Page.GetByText("Visitor", new PageGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(noPass.Page.Locator("#signed-in")).ToBeEmptyAsync();

        var holding = await NewWorld();
        await holding.Page.GotoAsync(CarePages.VisitorPassHoldingP7);
        await holding.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync();
        await Expect(holding.Page.Locator("#signed-in")).ToHaveTextAsync("pass P-7");
        await Expect(holding.Page.GetByRole(AriaRole.Listitem)).ToHaveCountAsync(0);
    }
}
