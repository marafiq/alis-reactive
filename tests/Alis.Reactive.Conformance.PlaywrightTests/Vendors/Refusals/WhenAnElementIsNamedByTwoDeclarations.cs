using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.Vendors;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAnElementIsNamedByTwoDeclarations(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M12_F2.M12_X2_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_element_and_both_vendors()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.CallPanelOfTwoDeclarations);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: Element 'conformance' is named as a component of vendor 'conformance-component' and as one of vendor "
                + "'conformance-peer', but an element holds one component: name it by the declaration of the component its view renders.",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.CallPanelOfOneDeclaration);
        await rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Ping and write" }).ClickAsync();
        await Expect(rendered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Ping" })).ToHaveTextAsync("Pinged 1 time");
        await Expect(rendered.Page.GetByRole(AriaRole.Status, new PageGetByRoleOptions { Name = "Ratio" })).ToHaveTextAsync("0.5");
    }
}
