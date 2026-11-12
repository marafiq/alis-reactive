using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.TypeKnowledge;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenAReadNamesAnOverrideTheJsonOptionsIgnore(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M16_F1.X18_8_0)]
    public async Task the_page_is_refused_where_the_view_renders_naming_the_read_and_the_class_it_starts_from()
    {
        var refused = await NewWorld();
        await refused.Page.GotoAsync(RefusalPages.Index);
        await refused.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Ignored member", Exact = true }).ClickAsync();
        await Expect(refused.Page.GetByRole(AriaRole.Heading, new PageGetByRoleOptions
        {
            Name = "InvalidOperationException: 'b => b.Ignored' reads a member of 'Binding' that the app's JSON options do not write, so the browser never holds it.",
            Exact = true,
        })).ToBeVisibleAsync();

        var written = await NewWorld();
        await written.Page.GotoAsync(RefusalPages.Index);
        await written.Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Written member", Exact = true }).ClickAsync();
        await written.Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Show the ignored member", Exact = true }).ClickAsync();
        await Expect(written.Page.Locator("#ignored")).ToHaveTextAsync("Ward 4");
    }
}
