using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAViewsLiteralHoldsAScriptsEnd(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X3_0)]
    public async Task the_literal_is_written_as_text_and_the_plan_still_runs()
    {
        var markup = await NewWorld();
        await markup.Page.GotoAsync(CarePages.ResidentNameCardWithMarkupInTheName);
        await markup.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show name", Exact = true }).ClickAsync();
        await markup.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show room", Exact = true }).ClickAsync();
        await Expect(markup.Page.Locator("#resident-name")).ToHaveTextAsync("</script><b>Ada</b>");
        await Expect(markup.Page.Locator("#resident-room")).ToHaveTextAsync("4B");
        await Expect(markup.Page.GetByText("Ada", new PageGetByTextOptions { Exact = true })).ToHaveCountAsync(0);

        var plain = await NewWorld();
        await plain.Page.GotoAsync(CarePages.ResidentNameCardWithAPlainName);
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show name", Exact = true }).ClickAsync();
        await plain.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show room", Exact = true }).ClickAsync();
        await Expect(plain.Page.Locator("#resident-name")).ToHaveTextAsync("Ada Lee");
        await Expect(plain.Page.Locator("#resident-room")).ToHaveTextAsync("4B");
    }
}
