using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenASmartPasteButtonNamesNoEndpoint(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F42.M21_X805_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_button_and_its_endpoint()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.SmartPasteWithNoEndpoint);
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: FusionSmartPasteButton 'intake-paste' names no AiAssistEndpoint: a Smart Paste button asks the endpoint for its answer",
            Exact = true,
        })).ToBeVisibleAsync();

        var rendered = await NewWorld();
        await rendered.Page.GotoAsync(RefusalPages.SmartPasteWithAnEndpoint);
        await Expect(rendered.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Smart Paste", Exact = true })).ToBeVisibleAsync();
    }
}
