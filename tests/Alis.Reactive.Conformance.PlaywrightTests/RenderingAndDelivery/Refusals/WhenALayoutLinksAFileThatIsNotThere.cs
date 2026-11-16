using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.RenderingAndDelivery;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net48))]
[Category(Hosts.Net48Name)]
public sealed class WhenALayoutLinksAFileThatIsNotThere(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M19_F1.M19_X2_0)]
    public async Task the_ward_page_is_refused_where_it_renders_naming_the_path_and_the_build_that_copies_the_file()
    {
        var misspelled = await NewWorld();
        await misspelled.Page.GotoAsync(RefusalPages.WardsLinkingAMisspelledRuntime);
        await misspelled.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the ward", Exact = true }).ClickAsync();
        await Expect(misspelled.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: No file is at ~/Content/AlisReactive/js/alis-reactive.jss (" + TwinSite.Folder + "\\Content\\AlisReactive\\js\\alis-reactive.jss). "
                + "A package's build copies its browser files into Content\\{PackageId}\\; build the app before it serves a page.",
            Exact = true,
        })).ToBeVisibleAsync();

        var linked = await NewWorld();
        await linked.Page.GotoAsync(RefusalPages.WardsLinkingTheirRuntime);
        await linked.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Open the ward", Exact = true }).ClickAsync();
        await Expect(linked.Page.Locator("#ward-name")).ToHaveTextAsync("North");
    }
}
