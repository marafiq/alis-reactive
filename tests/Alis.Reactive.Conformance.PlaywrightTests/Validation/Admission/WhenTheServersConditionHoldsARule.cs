using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Validation;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenTheServersConditionHoldsARule(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M8_F4.X8_3_0)]
    public async Task the_request_is_sent_and_the_endpoints_message_shows_beside_the_input()
    {
        var son = await NewWorld();
        await son.Page.GotoAsync(CarePages.VisitorSignIn);
        await son.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Relation", Exact = true }).FillAsync("Son");
        var sent = await son.Page.RunAndWaitForRequestAsync(() => son.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync(), "**/visitor-sign-in/sign-ins");
        Assert.That(sent.Method, Is.EqualTo("POST"));
        await Expect(son.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Visitor", Exact = true })
            .GetByText("Name the visitor.", new LocatorGetByTextOptions { Exact = true })).ToBeVisibleAsync();
        await Expect(son.Page.Locator("#signed-in")).ToBeEmptyAsync();

        var ada = await NewWorld();
        await ada.Page.GotoAsync(CarePages.VisitorSignIn);
        await ada.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Relation", Exact = true }).FillAsync("Son");
        await ada.Page.GetByRole(AriaRole.Textbox, new PageGetByRoleOptions { Name = "Visitor", Exact = true }).FillAsync("Ada");
        await ada.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Sign in", Exact = true }).ClickAsync();
        await Expect(ada.Page.Locator("#signed-in")).ToHaveTextAsync("Ada");
        await Expect(ada.Page.GetByText("Name the visitor.", new PageGetByTextOptions { Exact = true })).ToBeHiddenAsync();
    }
}
