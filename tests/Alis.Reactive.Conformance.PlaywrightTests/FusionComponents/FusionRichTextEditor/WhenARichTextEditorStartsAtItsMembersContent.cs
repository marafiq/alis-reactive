using Alis.Reactive.Conformance.PlaywrightTests.Harness;
using Alis.Reactive.Conformance.World;
using Microsoft.Playwright;
using NUnit.Framework;
using static Microsoft.Playwright.Assertions;

namespace Alis.Reactive.Conformance.PlaywrightTests.FusionComponents;

[TestFixtureSource(typeof(Hosts), nameof(Hosts.Net10))]
[Category(Hosts.Net10Name)]
public sealed class WhenARichTextEditorStartsAtItsMembersContent(string host) : ConformanceFixture(host)
{
    [Test]
    [FailsFor(Promises.M21_F38.M21_X1023_0)]
    public async Task the_editor_shows_the_content_the_member_holds()
    {
        var written = await NewWorld();
        await written.Page.GotoAsync(CarePages.HandoverSummaryWritten);

        await Expect(written.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Summary", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Rich Text Editor", Exact = true })).ToHaveTextAsync("Ate a full lunch.");

        var blank = await NewWorld();
        await blank.Page.GotoAsync(CarePages.HandoverSummaryBlank);

        await Expect(blank.Page.GetByRole(AriaRole.Group, new PageGetByRoleOptions { Name = "Summary", Exact = true }).GetByRole(AriaRole.Textbox, new LocatorGetByRoleOptions { Name = "Rich Text Editor", Exact = true })).ToHaveTextAsync("");
    }
}
